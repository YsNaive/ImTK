using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ImTK.Core
{
    /// <summary>
    /// The global driver and manager for the ImTK framework.
    /// It manages modules, objects, and enforces the strict lifecycle state machine.
    /// </summary>
    public static class ImTKApplication
    {
        public static ApplicationState CurrentState { get; private set; } = ApplicationState.Uninitialized;

        // Module storage
        private static readonly Dictionary<Type, ImTKModule> s_modules = new Dictionary<Type, ImTKModule>();

        // Object storage and pending queues
        private static readonly List<ImTKObject> s_objects = new List<ImTKObject>();
        private static readonly List<ImTKObject> s_pendingAdd = new List<ImTKObject>();
        private static readonly List<ImTKObject> s_pendingRemove = new List<ImTKObject>();

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

                // Scanning and instantiation
                SetState(ApplicationState.InitializeSelf);

                var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(ImTKModule).IsAssignableFrom(t));

                foreach (var type in moduleTypes)
                {
                    // Strict constructor checking (must be exactly one parameterless non-public constructor)
                    var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (constructors.Length > 1 || constructors[0].GetParameters().Length > 0 || constructors[0].IsPublic)
                    {
                        throw new InvalidOperationException($"ImTKModule rule violation: {type.FullName} must have exactly one parameterless non-public constructor.");
                    }

                    var instance = (ImTKModule)Activator.CreateInstance(type, true);
                    s_modules[type] = instance;
                }

                // Phase 1: Initialize Self
                foreach (var module in s_modules.Values)
                {
                    module.OnInitializeSelf();
                }

                // Phase 2: Initialize Dependencies
                SetState(ApplicationState.InitializeDependencies);
                foreach (var module in s_modules.Values)
                {
                    module.OnInitializeDependencies();
                }

                SetState(ApplicationState.AwaitingGraphicsSetup);
            }

            public static void GraphicsSetup()
            {
                RequireState(ApplicationState.AwaitingGraphicsSetup);
                SetState(ApplicationState.GraphicsSetup);

                foreach (var module in s_modules.Values)
                {
                    module.OnGraphicsSetup();
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.LogicUpdate;
            }

            public static void LogicUpdate(double rawDeltaTime)
            {
                EnforceFrameOrder(ApplicationState.LogicUpdate);

                Time.Update(rawDeltaTime);

                SetState(ApplicationState.LogicUpdate);

                foreach (var module in s_modules.Values)
                {
                    if (module.m_activeInHierarchy) module.OnLogicUpdate();
                }

                foreach (var obj in s_objects)
                {
                    if (obj.m_activeInHierarchy) obj.OnLogicUpdate();
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.GuiRender;
            }

            public static void GuiRender()
            {
                EnforceFrameOrder(ApplicationState.GuiRender);
                SetState(ApplicationState.GuiRender);

                foreach (var module in s_modules.Values)
                {
                    if (module.m_activeInHierarchy) module.OnGuiRender();
                }

                foreach (var obj in s_objects)
                {
                    if (obj.m_activeInHierarchy) obj.OnGuiRender();
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.GizmoRender;
            }

            public static void GizmoRender()
            {
                EnforceFrameOrder(ApplicationState.GizmoRender);
                SetState(ApplicationState.GizmoRender);

                foreach (var module in s_modules.Values)
                {
                    if (module.m_activeInHierarchy) module.OnGizmoRender();
                }

                foreach (var obj in s_objects)
                {
                    if (obj.m_activeInHierarchy) obj.OnGizmoRender();
                }

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.LateUpdate;
            }

            public static void LateUpdate()
            {
                EnforceFrameOrder(ApplicationState.LateUpdate);
                SetState(ApplicationState.LateUpdate);

                // Run normal LateUpdate
                foreach (var module in s_modules.Values)
                {
                    if (module.m_activeInHierarchy) module.OnLateUpdate();
                }

                foreach (var obj in s_objects)
                {
                    if (obj.m_activeInHierarchy) obj.OnLateUpdate();
                }

                // Process pending collections and Enable/Disable state changes
                ProcessPendingQueuesAndStateChanges();

                SetState(ApplicationState.Idle);
                s_minAllowedFrameState = ApplicationState.LogicUpdate; // Reset frame lock
            }

            public static void Close()
            {
                if (CurrentState == ApplicationState.Closed || CurrentState == ApplicationState.Close) return;

                SetState(ApplicationState.Close);

                // Disable all objects and modules
                foreach (var obj in s_objects)
                {
                    if (obj.m_activeInHierarchy) obj.OnDisable();
                    obj.OnDestroy();
                }

                foreach (var module in s_modules.Values)
                {
                    if (module.m_activeInHierarchy) module.OnDisable();
                    module.OnClose();
                }

                s_objects.Clear();
                s_modules.Clear();

                SetState(ApplicationState.Closed);
            }

            private static void ProcessPendingQueuesAndStateChanges()
            {
                // Add pending objects
                if (s_pendingAdd.Count > 0)
                {
                    foreach (var obj in s_pendingAdd)
                    {
                        s_objects.Add(obj);
                        if (obj.m_enabled)
                        {
                            obj.m_activeInHierarchy = true;
                            obj.OnEnable();
                        }
                    }
                    s_pendingAdd.Clear();
                }

                // Check enable/disable state changes for modules
                foreach (var module in s_modules.Values)
                {
                    if (module.m_enabled && !module.m_activeInHierarchy)
                    {
                        module.m_activeInHierarchy = true;
                        module.OnEnable();
                    }
                    else if (!module.m_enabled && module.m_activeInHierarchy)
                    {
                        module.m_activeInHierarchy = false;
                        module.OnDisable();
                    }
                }

                // Check enable/disable state changes for objects
                foreach (var obj in s_objects)
                {
                    if (obj.m_enabled && !obj.m_activeInHierarchy)
                    {
                        obj.m_activeInHierarchy = true;
                        obj.OnEnable();
                    }
                    else if (!obj.m_enabled && obj.m_activeInHierarchy)
                    {
                        obj.m_activeInHierarchy = false;
                        obj.OnDisable();
                    }
                }

                // Remove pending objects
                if (s_pendingRemove.Count > 0)
                {
                    foreach (var obj in s_pendingRemove)
                    {
                        if (obj.m_activeInHierarchy)
                        {
                            obj.m_activeInHierarchy = false;
                            obj.OnDisable();
                        }
                        obj.OnDestroy();
                        s_objects.Remove(obj);
                    }
                    s_pendingRemove.Clear();
                }
            }
        }
    }
}
