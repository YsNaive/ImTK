using System;
using System.Collections.Generic;
using ImTK.Core;
using ImTK.Log;
using Hexa.NET.ImGui;
using System.Numerics;

namespace ImTK.UI
{
    public class Panel : ImTKModule
    {

        private readonly List<(Func<Rect, Rect> func, int priority)> m_reservedAreas = new();

        private static readonly Dictionary<WindowKey, Window> s_windows = new Dictionary<WindowKey, Window>();
        private static readonly Queue<Window> s_windowsToAdd = new Queue<Window>();
        private static readonly Queue<Window> s_windowsToRemove = new Queue<Window>();

        private class WindowHostElement : VisualElement
        {
        }

        private static WindowHostElement s_hostElement;

        protected Panel() { }

        internal static void RegisterWindow(Window window)
        {
            if (ImTKApplication.CurrentState == ApplicationState.GuiRender)
            {
                s_windowsToAdd.Enqueue(window);
            }
            else
            {
                WindowKey key = new WindowKey(window.GetType(), window.windowId);
                if (s_windows.ContainsKey(key))
                {
                    ImTKLog.Error($"Failed to register window. Type '{key.Type.Name}' with ID '{key.WindowId}' is already open.");
                    throw new InvalidOperationException($"A window of type '{key.Type}' with windowId '{key.WindowId}' is already open.");
                }
                s_windows[key] = window;
                if (s_hostElement != null)
                {
                    s_hostElement.hierarchy.Add(window);
                }
                ScheduleSaveWorkspace();
            }
        }

        internal static void UnregisterWindow(Window window)
        {
            if (ImTKApplication.CurrentState == ApplicationState.GuiRender)
            {
                s_windowsToRemove.Enqueue(window);
            }
            else
            {
                WindowKey key = new WindowKey(window.GetType(), window.windowId);
                
                // Save window state before unregistering
                RenderEngine.SaveAllPersistentStates();

                s_windows.Remove(key);
                if (s_hostElement != null)
                {
                    s_hostElement.hierarchy.Remove(window);
                }
                ScheduleSaveWorkspace();
            }
        }

        internal static bool TryGetWindow(WindowKey key, out Window window)
        {
            return s_windows.TryGetValue(key, out window);
        }

        private static bool s_isSaveWorkspaceScheduled = false;

        private static void ScheduleSaveWorkspace()
        {
            if (s_isSaveWorkspaceScheduled) return;
            s_isSaveWorkspaceScheduled = true;
            ImTKApplication.ScheduleDeferred(() =>
            {
                s_isSaveWorkspaceScheduled = false;
                SaveWorkspace();
            });
        }

        private static void SaveWorkspace()
        {
            try
            {
                var cache = Database.ImTKDatabase.Load<Database.ImTKCacheAsset>("imgui/imtk_cache.json");
                if (cache == null) return;
                
                cache.OpenWindows.Clear();

                foreach (var window in s_windows.Values)
                {
                    if (!window.flags.dontSaveOpenState)
                    {
                        cache.OpenWindows.Add(new Database.WindowSession
                        {
                            TypeName = window.GetType().AssemblyQualifiedName,
                            WindowId = window.windowId
                        });
                    }
                }

                cache.MarkDirty();
                ImTKLog.Trace("Workspace saved.");
            }
            catch (Exception e)
            {
                ImTKLog.Error(e, "Failed to save workspace.");
            }
        }

        private static void RestoreWorkspace()
        {
            ImTKLog.Info("Attempting to RestoreWorkspace...");
            try
            {
                var cache = ImTK.Database.ImTKDatabase.Load<ImTK.Database.ImTKCacheAsset>("imgui/imtk_cache.json");
                if (cache == null)
                {
                    ImTKLog.Warning("RestoreWorkspace: cache is null.");
                    return;
                }
                
                if (cache.OpenWindows == null)
                {
                    ImTKLog.Warning("RestoreWorkspace: cache.OpenWindows is null.");
                    return;
                }

                if (cache.OpenWindows.Count == 0)
                {
                    ImTKLog.Info("RestoreWorkspace: No windows to restore (OpenWindows is empty).");
                    return;
                }

                ImTKLog.Info($"Restoring {cache.OpenWindows.Count} windows from workspace cache.");

                foreach (var session in cache.OpenWindows)
                {
                    ImTKLog.Info($"Trying to restore window type: '{session.TypeName}' with ID: '{session.WindowId}'");
                    try
                    {
                        Type type = Type.GetType(session.TypeName);
                        if (type == null)
                        {
                            string typeFullName = session.TypeName.Split(',')[0].Trim();
                            ImTKLog.Debug($"Type.GetType returned null. Searching assemblies for '{typeFullName}'...");
                            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(typeFullName);
                                if (type != null) 
                                {
                                    ImTKLog.Debug($"Found type in assembly: {asm.FullName}");
                                    break;
                                }
                            }
                        }

                        if (type != null)
                        {
                            ImTKLog.Info($"Calling Window.Open for type {type.Name}");
                            Window.Open(type, session.WindowId);
                        }
                        else
                        {
                            ImTKLog.Warning($"Failed to resolve window type: {session.TypeName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ImTKLog.Error(ex, $"Exception while restoring window {session.TypeName}");
                    }
                }
            }
            catch (Exception e)
            {
                ImTKLog.Error(e, "Failed to restore workspace.");
            }
        }

        protected internal override void OnLogicUpdate()
        {
            while (s_windowsToAdd.Count > 0) RegisterWindow(s_windowsToAdd.Dequeue());
            while (s_windowsToRemove.Count > 0) UnregisterWindow(s_windowsToRemove.Dequeue());

            if (ImTKTheme.isGlobalThemeDirty)
            {
                ImTKTheme.GlobalTheme.ApplyToImGui();
                ImTKTheme.isGlobalThemeDirty = false;
            }

            foreach (var kvp in s_windows)
            {
                var window = kvp.Value;
                using (ImTKProfiler.ScopeRelative(window.GetType().Name))
                {
                    try
                    {
                        window.Update();
                    }
                    catch (Exception ex)
                    {
                        ImTKLog.Error(ex, $"Exception in Update of window: {window.imguiId}");
                    }
                }
            }
        }

        protected internal override void OnEnable()
        {
            RestoreWorkspace();
        }

        protected internal override void OnInitializeSelf()
        {
            s_hostElement = new WindowHostElement();
            ImTKTheme.onGlobalThemeChanged += OnGlobalThemeChanged;
        }

        private void OnGlobalThemeChanged()
        {
            if (s_hostElement != null)
            {
                s_hostElement.MarkStyleDirty();
            }
        }

        protected internal override void OnInitializeDependencies()
        {
        }

        public void RequireArea(Func<Rect, Rect> reservedFunc, int priority = 0)
        {
            if (ImTKApplication.CurrentState != ApplicationState.InitializeSelf && ImTKApplication.CurrentState != ApplicationState.InitializeDependencies)
            {
                ImTKLog.Error($"Cannot require area outside of initialization phase. Current state: {ImTKApplication.CurrentState}");
                return;
            }

            m_reservedAreas.Add((reservedFunc, priority));
            m_reservedAreas.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        protected internal override void OnGuiRender()
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();

            Rect currentRect = new Rect(viewport.WorkPos, viewport.WorkSize);

            foreach (var area in m_reservedAreas)
            {
                currentRect = area.func(currentRect);
            }

            ImGui.SetNextWindowPos(currentRect.min);
            ImGui.SetNextWindowSize(currentRect.size);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

            int globalFontFamilyHash = ImTKTheme.GlobalTheme.fontFamilyHash;
            var font = ImTKFontManager.GetFont(globalFontFamilyHash);
            bool pushedFont = false;

            unsafe
            {
                if (font.Handle != null)
                {
                    if (globalFontFamilyHash != ImTKFontManager.DefaultFontFamilyHash)
                    {
                        ImGui.PushFont((Hexa.NET.ImGui.ImFont*)font.Handle, ((Hexa.NET.ImGui.ImFont*)font.Handle)->LegacySize);
                        RenderEngine.Context.PushFontState(globalFontFamilyHash);
                        pushedFont = true;
                    }
                }
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.Begin("MainDockSpaceWindow", windowFlags);
            ImGui.PopStyleVar();

            uint dockspaceId = ImGui.GetID("MainDockSpace");
            var dockSize = ImGui.GetContentRegionAvail();
            if (dockSize.X > 0f && dockSize.Y > 0f)
            {
                ImGui.DockSpace(dockspaceId, dockSize, ImGuiDockNodeFlags.None);
            }

            ImGui.End();

            foreach (var kvp in s_windows)
            {
                var window = kvp.Value;
                try
                {
                    RenderEngine.Context.CurrentDpiScale = window.CurrentDpiScale;
                    RenderEngine.Render(window);
                }
                catch (Exception ex)
                {
                    ImTKLog.Error(ex, $"Exception in Render of Window: {window.imguiId}");
                }
            }

            if (pushedFont)
            {
                ImGui.PopFont();
                RenderEngine.Context.PopFontState();
            }
        }

        protected internal override void OnClose()
        {
            RenderEngine.SaveAllPersistentStates();
            ImTKTheme.onGlobalThemeChanged -= OnGlobalThemeChanged;
            s_hostElement = null;
            s_windows.Clear();
        }
    }
}
