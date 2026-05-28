using System;
using System.Collections.Generic;
using ImTK.Core;
using ImTK.Log;
using ImGuiNET;
using System.Numerics;

namespace ImTK.UI
{
    public class Panel : ImTKModule
    {
        private static readonly LogContext s_log = new LogContext("Panel");
        private readonly List<(Func<Rect, Rect> func, int priority)> m_reservedAreas = new();

        private static readonly Dictionary<WindowKey, Window> s_windows = new Dictionary<WindowKey, Window>();
        private static readonly Queue<Window> s_windowsToAdd = new Queue<Window>();
        private static readonly Queue<Window> s_windowsToRemove = new Queue<Window>();

        private class WindowHostElement : VisualElement
        {
        }

        private static WindowHostElement s_hostElement;

        private float m_cacheSaveTimer = 0f;
        private const float CacheSaveInterval = 10f;

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
                    s_log.Error($"Failed to register window. Type '{key.Type.Name}' with ID '{key.WindowId}' is already open.");
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
                s_log.Trace("Workspace saved.");
            }
            catch (Exception e)
            {
                s_log.Error(e, "Failed to save workspace.");
            }
        }

        private static void RestoreWorkspace()
        {
            s_log.Info("Attempting to RestoreWorkspace...");
            try
            {
                var cache = ImTK.Database.ImTKDatabase.Load<ImTK.Database.ImTKCacheAsset>("imgui/imtk_cache.json");
                if (cache == null)
                {
                    s_log.Warning("RestoreWorkspace: cache is null.");
                    return;
                }
                
                if (cache.OpenWindows == null)
                {
                    s_log.Warning("RestoreWorkspace: cache.OpenWindows is null.");
                    return;
                }

                if (cache.OpenWindows.Count == 0)
                {
                    s_log.Info("RestoreWorkspace: No windows to restore (OpenWindows is empty).");
                    return;
                }

                s_log.Info($"Restoring {cache.OpenWindows.Count} windows from workspace cache.");

                foreach (var session in cache.OpenWindows)
                {
                    s_log.Info($"Trying to restore window type: '{session.TypeName}' with ID: '{session.WindowId}'");
                    try
                    {
                        Type type = Type.GetType(session.TypeName);
                        if (type == null)
                        {
                            string typeFullName = session.TypeName.Split(',')[0].Trim();
                            s_log.Debug($"Type.GetType returned null. Searching assemblies for '{typeFullName}'...");
                            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(typeFullName);
                                if (type != null) 
                                {
                                    s_log.Debug($"Found type in assembly: {asm.FullName}");
                                    break;
                                }
                            }
                        }

                        if (type != null)
                        {
                            s_log.Info($"Calling Window.Open for type {type.Name}");
                            Window.Open(type, session.WindowId);
                        }
                        else
                        {
                            s_log.Warning($"Failed to resolve window type: {session.TypeName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        s_log.Error(ex, $"Exception while restoring window {session.TypeName}");
                    }
                }
            }
            catch (Exception e)
            {
                s_log.Error(e, "Failed to restore workspace.");
            }
        }

        protected internal override void OnLogicUpdate()
        {
            while (s_windowsToAdd.Count > 0) RegisterWindow(s_windowsToAdd.Dequeue());
            while (s_windowsToRemove.Count > 0) UnregisterWindow(s_windowsToRemove.Dequeue());

            m_cacheSaveTimer += (float)Time.UnscaledDeltaTime;
            if (m_cacheSaveTimer >= CacheSaveInterval)
            {
                Persistence.ViewStatePersister.SaveAllWindowStates(s_windows.Values);
                m_cacheSaveTimer = 0f;
            }

            if (ImTKTheme.isGlobalThemeDirty)
            {
                ImTKTheme.GlobalTheme.ApplyToImGui();
                ImTKTheme.isGlobalThemeDirty = false;
            }

            foreach (var window in s_windows.Values)
            {
                try
                {
                    window.Update();
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Exception in Update of window: {window.imguiId}");
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
                s_log.Error($"Cannot require area outside of initialization phase. Current state: {ImTKApplication.CurrentState}");
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
            var font = ImTKFontManager.GetFont(globalFontFamilyHash, FontSize.Normal);
            bool pushedFont = false;

            unsafe
            {
                if (font.NativePtr != null)
                {
                    ImGui.PushFont(font);
                    RenderingContext.PushFontState(globalFontFamilyHash);
                    pushedFont = true;
                }
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.Begin("MainDockSpaceWindow", windowFlags);
            ImGui.PopStyleVar();

            uint dockspaceId = ImGui.GetID("MainDockSpace");
            ImGui.DockSpace(dockspaceId, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.None);

            ImGui.End();

            foreach (var window in s_windows.Values)
            {
                try
                {
                    window.UpdateRenderCache();
                    RenderEngine.ComputeStyleFlat(window.RenderCache.renderList);
                    RenderEngine.RenderFlat(window.RenderCache.renderList);
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Exception in Render of Window: {window.imguiId}");
                }
            }

            if (pushedFont)
            {
                ImGui.PopFont();
                RenderingContext.PopFontState();
            }
        }

        protected internal override void OnClose()
        {
            Persistence.ViewStatePersister.SaveAllWindowStates(s_windows.Values);
            ImTKTheme.onGlobalThemeChanged -= OnGlobalThemeChanged;
            s_hostElement = null;
            s_windows.Clear();
        }
    }
}
