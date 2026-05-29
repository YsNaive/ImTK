using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImTK.Log;

namespace ImTK.Core
{
    /// <summary>
    /// The global driver and manager for the ImTK framework.
    /// It manages modules, objects, and enforces the strict lifecycle state machine.
    /// </summary>
    public static class ImTKApplication
    {


        /// <summary>
        /// Gets the current version of the ImTK framework (e.g., "0.1.0-alpha").
        /// </summary>
        public static string Version
        {
            get
            {
                string ret = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                ret = ret?.Substring(0, ret.LastIndexOf('+'));
                ret ??= "Unknow";
                return ret;
            }
        }
        public static ApplicationState CurrentState { get; private set; } = ApplicationState.Uninitialized;

        // Module storage
        private static readonly Dictionary<Type, ImTKModule> s_modules = new Dictionary<Type, ImTKModule>();

        // Object storage and pending queues
        private static readonly List<ImTKObject> s_objects = new List<ImTKObject>();
        private static readonly List<ImTKObject> s_pendingAdd = new List<ImTKObject>();
        private static readonly List<ImTKObject> s_pendingRemove = new List<ImTKObject>();

        private static readonly List<Action> s_deferredActions = new List<Action>();

        // Frame phase tracking to prevent reverse-order calls
        private static ApplicationState s_minAllowedFrameState = ApplicationState.LogicUpdate;

        /// <summary>
        /// Retrieves an initialized system module.
        /// Throws an exception if the module type cannot be found.
        /// </summary>
        public static T GetModule<T>() where T : ImTKModule
        {
            if (s_modules.TryGetValue(typeof(T), out var module))
            {
                return (T)module;
            }
            throw new InvalidOperationException($"Module of type {typeof(T).Name} is not registered or initialized.");
        }

        /// <summary>
        /// Manually register a dynamic runtime object.
        /// Note: ImTKObject auto-registers in its base constructor.
        /// </summary>
        public static void RegisterObject(ImTKObject obj)
        {
            if (obj == null) return;
            s_pendingAdd.Add(obj);
        }

        /// <summary>
        /// Manually unregister a dynamic runtime object.
        /// Note: Call obj.Destroy() instead for standard teardown.
        /// </summary>
        public static void UnregisterObject(ImTKObject obj)
        {
            if (obj == null) return;
            s_pendingRemove.Add(obj);
        }

        /// <summary>
        /// Schedules an action to be executed safely at the end of the current frame (LateUpdate).
        /// Useful for modifying UI structures or state during restricted phases like GuiRender.
        /// </summary>
        public static void ScheduleDeferred(Action action)
        {
            if (action == null) return;
            lock (s_deferredActions)
            {
                s_deferredActions.Add(action);
            }
        }

        /// <summary>
        /// The internal driver structure for lifecycle events.
        /// Should only be called by the root window/platform bridge (e.g., ImTKSilk).
        /// </summary>
        public static class Lifecycle
        {
            private static void SetState(ApplicationState newState)
            {
                CurrentState = newState;
            }

            private static void RequireState(ApplicationState expectedState)
            {
                if (CurrentState != expectedState)
                    throw new InvalidOperationException($"Lifecycle error: Expected state {expectedState}, but current state is {CurrentState}.");
            }

            private static void EnforceFrameOrder(ApplicationState phase)
            {
                RequireState(ApplicationState.Idle);
                if (phase < s_minAllowedFrameState)
                    throw new InvalidOperationException($"Lifecycle order violation: Cannot execute {phase} because minimum allowed state is {s_minAllowedFrameState}. Did you call phases out of order or repeat a phase?");
            }

            public static void Initialize()
            {
                RequireState(ApplicationState.Uninitialized);

                // --- Reflection Phase 0: Log Sinks ---
                // Sinks must be initialized before anything else to ensure early logs (and fatal errors) are captured.
                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
                    .SelectMany(a => 
                    {
                        try { return a.GetTypes(); }
                        catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
                    });

                var sinkTypes = allTypes.Where(t => t.IsClass && !t.IsAbstract && typeof(ILogSink).IsAssignableFrom(t));

                foreach (var type in sinkTypes)
                {
                    // Enforce one parameterless non-public constructor for automatic sinks
                    var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (constructors.Length == 1 && constructors[0].GetParameters().Length == 0 && !constructors[0].IsPublic)
                    {
                        var sinkInstance = (ILogSink)Activator.CreateInstance(type, true);
                        ImTKLog.AddSink(sinkInstance);
                    }
                }

                ImTKLog.Info($"ImTK Framework Version {Version} Initializing...");

                // --- Dispatcher Initialization ---
                ImTK.Event.ImTKDispatcher.InitializeMainThread();

                // --- Reflection Phase 1: ImTKModules ---
                SetState(ApplicationState.InitializeSelf);

                ImTKLog.Debug("Discovering ImTKModules via reflection...");

                var moduleTypes = allTypes.Where(t => t.IsClass && !t.IsAbstract && typeof(ImTKModule).IsAssignableFrom(t));

                foreach (var type in moduleTypes)
                {
                    try
                    {
                        // Strict constructor checking (must be exactly one parameterless non-public constructor)
                        var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (constructors.Length > 1 || constructors[0].GetParameters().Length > 0 || constructors[0].IsPublic)
                        {
                            throw new InvalidOperationException($"ImTKModule rule violation: {type.FullName} must have exactly one parameterless non-public constructor.");
                        }

                        var instance = (ImTKModule)Activator.CreateInstance(type, true);
                        s_modules[type] = instance;
                        ImTKLog.Trace($"Instantiated module: {type.Name}");
                    }
                    catch (Exception ex)
                    {
                        ImTKLog.Fatal(ex, $"Failed to instantiate module: {type.Name}");
                        throw;
                    }
                }

                ImTKLog.Info($"Discovered and instantiated {s_modules.Count} modules.");

                // Phase 1: Initialize Self
                ImTKLog.Debug("Executing module OnInitializeSelf...");
                foreach (var module in s_modules.Values)
                {
                    module.OnInitializeSelf();
                }

                // Phase 2: Initialize Dependencies
                SetState(ApplicationState.InitializeDependencies);
                ImTKLog.Debug("Executing module OnInitializeDependencies...");
                foreach (var module in s_modules.Values)
                {
                    module.OnInitializeDependencies();
                }

                // Phase 3: Enable
                ImTKLog.Debug("Executing module OnEnable...");
                foreach (var module in s_modules.Values)
                {
                    if (module.m_enabled)
                    {
                        module.m_activeInHierarchy = true;
                        module.InternalOnEnable();
                    }
                }

                SetState(ApplicationState.AwaitingGraphicsSetup);
                ImTKLog.Info("ImTKApplication initialized successfully.");
            }

            public static void GraphicsSetup()
            {
                RequireState(ApplicationState.AwaitingGraphicsSetup);
                SetState(ApplicationState.GraphicsSetup);

                ImTKLog.Debug("Executing module OnGraphicsSetup...");
                foreach (var module in s_modules.Values)
                {
                    module.OnGraphicsSetup();
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.LogicUpdate;
                ImTKLog.Info("Graphics setup completed. Entering runtime loop.");
            }

            public static void LogicUpdate(double rawDeltaTime)
            {
                EnforceFrameOrder(ApplicationState.LogicUpdate);

                Time.Update(rawDeltaTime);

                SetState(ApplicationState.LogicUpdate);

                using (ImTKProfiler.Scope("Lifecycle/LogicUpdate"))
                {
                    foreach (var module in s_modules.Values)
                    {
                        if (!module.m_activeInHierarchy) continue;
                        try { module.OnLogicUpdate(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during LogicUpdate by {module.GetType().Name}"); }
                    }

                    foreach (var obj in s_objects)
                    {
                        if (!obj.m_activeInHierarchy) continue;
                        try { obj.OnLogicUpdate(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during LogicUpdate by {obj.GetType().Name}"); }
                    }
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.GuiRender;
            }

            public static void GuiRender()
            {
                EnforceFrameOrder(ApplicationState.GuiRender);
                SetState(ApplicationState.GuiRender);

                using (ImTKProfiler.Scope("Lifecycle/Gui"))
                {
                    foreach (var module in s_modules.Values)
                    {
                        if (!module.m_activeInHierarchy) continue;
                        try { module.OnGuiRender(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during GuiRender by {module.GetType().Name}"); }
                    }

                    foreach (var obj in s_objects)
                    {
                        if (!obj.m_activeInHierarchy) continue;
                        try { obj.OnGuiRender(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during GuiRender by {obj.GetType().Name}"); }
                    }
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.GizmoRender;
            }

            public static void GizmoRender()
            {
                EnforceFrameOrder(ApplicationState.GizmoRender);
                SetState(ApplicationState.GizmoRender);

                using (ImTKProfiler.Scope("Lifecycle/Gizmo"))
                {
                    foreach (var module in s_modules.Values)
                    {
                        if (!module.m_activeInHierarchy) continue;
                        try { module.OnGizmoRender(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during GizmoRender by {module.GetType().Name}"); }
                    }

                    foreach (var obj in s_objects)
                    {
                        if (!obj.m_activeInHierarchy) continue;
                        try { obj.OnGizmoRender(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during GizmoRender by {obj.GetType().Name}"); }
                    }
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.LateUpdate;
            }

            public static void LateUpdate()
            {
                EnforceFrameOrder(ApplicationState.LateUpdate);
                SetState(ApplicationState.LateUpdate);

                using (ImTKProfiler.Scope("Lifecycle/LateUpdate"))
                {
                    // Run normal LateUpdate
                    foreach (var module in s_modules.Values)
                    {
                        if (!module.m_activeInHierarchy) continue;
                        try { module.OnLateUpdate(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during LateUpdate by {module.GetType().Name}"); }
                    }

                    foreach (var obj in s_objects)
                    {
                        if (!obj.m_activeInHierarchy) continue;
                        try { obj.OnLateUpdate(); }
                        catch (Exception ex) { ImTKLog.Error(ex, $"Exception thrown during LateUpdate by {obj.GetType().Name}"); }
                    }

                    // Process main thread dispatcher queue
                    ImTK.Event.ImTKDispatcher.ProcessQueue();

                    // Process deferred actions scheduled during the frame
                    ProcessDeferredActions();

                    // Process pending collections and Enable/Disable state changes
                    ProcessPendingQueuesAndStateChanges();
                }

                // --- Font System Resolution ---
                // Resolve fonts at the very end of the frame when ImGui is completely unlocked
                ImTK.UI.ImTKFontManager.ResolveFont();

                ImTKProfiler.EndFrame();

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.LogicUpdate; // Reset frame lock
            }

            public static void Close()
            {
                if (CurrentState == ApplicationState.Closed || CurrentState == ApplicationState.Close) return;

                ImTKLog.Info("Application closing. Teardown initiated...");
                SetState(ApplicationState.Close);

                // Clear event bus subscriptions
                ImTK.Event.ImTKEventBus.ClearAll();

                ImTKLog.Debug($"Disabling and destroying active ImTKObjects ({s_objects.Count}).");
                // Disable all objects and modules
                foreach (var obj in s_objects)
                {
                    if (obj.m_activeInHierarchy) obj.InternalOnDisable();
                    obj.OnDestroy();
                }

                ImTKLog.Debug($"Disabling and closing ImTKModules ({s_modules.Count}).");
                foreach (var module in s_modules.Values)
                {
                    if (module.m_activeInHierarchy) module.InternalOnDisable();
                    module.OnClose();
                }

                s_objects.Clear();
                s_modules.Clear();
                s_deferredActions.Clear();

                SetState(ApplicationState.Closed);
                ImTKLog.Info("ImTKApplication shutdown complete.");
            }

            private static void ProcessDeferredActions()
            {
                Action[] actionsToRun = null;
                lock (s_deferredActions)
                {
                    if (s_deferredActions.Count > 0)
                    {
                        actionsToRun = s_deferredActions.ToArray();
                        s_deferredActions.Clear();
                    }
                }

                if (actionsToRun != null)
                {
                    foreach (var action in actionsToRun)
                    {
                        try { action(); }
                        catch (Exception ex) { ImTKLog.Error(ex, "Exception occurred during deferred action execution."); }
                    }
                }
            }

            private static void ProcessPendingQueuesAndStateChanges()
            {
                // Add pending objects (copy to array to prevent modification during iteration)
                if (s_pendingAdd.Count > 0)
                {
                    var adding = s_pendingAdd.ToArray();
                    s_pendingAdd.Clear();
                    foreach (var obj in adding)
                    {
                        s_objects.Add(obj);
                        ImTKLog.Trace($"Registered new ImTKObject: {obj.GetType().Name}");
                        if (obj.m_enabled)
                        {
                            obj.m_activeInHierarchy = true;
                            obj.InternalOnEnable();
                        }
                    }
                }

                // Check enable/disable state changes for modules
                foreach (var module in s_modules.Values)
                {
                    if (module.m_enabled && !module.m_activeInHierarchy)
                    {
                        module.m_activeInHierarchy = true;
                        module.InternalOnEnable();
                    }
                    else if (!module.m_enabled && module.m_activeInHierarchy)
                    {
                        module.m_activeInHierarchy = false;
                        module.InternalOnDisable();
                    }
                }

                // Check enable/disable state changes for objects (copy to array)
                var currentObjects = s_objects.ToArray();
                foreach (var obj in currentObjects)
                {
                    if (obj.m_enabled && !obj.m_activeInHierarchy)
                    {
                        obj.m_activeInHierarchy = true;
                        obj.InternalOnEnable();
                    }
                    else if (!obj.m_enabled && obj.m_activeInHierarchy)
                    {
                        obj.m_activeInHierarchy = false;
                        obj.InternalOnDisable();
                    }
                }

                // Remove pending objects
                if (s_pendingRemove.Count > 0)
                {
                    var removing = s_pendingRemove.ToArray();
                    s_pendingRemove.Clear();
                    foreach (var obj in removing)
                    {
                        if (obj.m_activeInHierarchy)
                        {
                            obj.m_activeInHierarchy = false;
                            obj.InternalOnDisable();
                        }
                        obj.OnDestroy();
                        s_objects.Remove(obj);
                        ImTKLog.Trace($"Destroyed ImTKObject: {obj.GetType().Name}");
                    }
                }
            }
        }
    }
}
