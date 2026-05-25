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
        private static readonly List<Window> s_windowsToAdd = new List<Window>();
        private static readonly List<Window> s_windowsToRemove = new List<Window>();

        private class WindowHostElement : VisualElement
        {
        }

        private static WindowHostElement s_hostElement;

        protected Panel() { }

        internal static void RegisterWindow(Window window)
        {
            WindowKey key = new WindowKey(window.GetType(), window.windowId);
            if (s_windows.ContainsKey(key))
            {
                s_log.Error($"Failed to register window. Type '{key.Type.Name}' with ID '{key.WindowId}' is already open.");
                throw new InvalidOperationException($"A window of type '{key.Type}' with windowId '{key.WindowId}' is already open.");
            }
            s_windows[key] = window;
            s_windowsToAdd.Add(window);

            s_log.Trace($"Window registered in Panel: {window.imguiId}");
        }

        internal static void UnregisterWindow(Window window)
        {
            s_windowsToRemove.Add(window);
        }

        internal static bool TryGetWindow(WindowKey key, out Window window)
        {
            return s_windows.TryGetValue(key, out window);
        }

        protected internal override void OnLogicUpdate()
        {
            if (ImTKTheme.isGlobalThemeDirty)
            {
                ImTKTheme.GlobalTheme.ApplyToImGui();
                ImTKTheme.isGlobalThemeDirty = false;
            }

            foreach (var window in s_windowsToAdd)
            {
                if (s_hostElement != null)
                {
                    s_hostElement.hierarchy.Add(window);
                }
            }
            s_windowsToAdd.Clear();

            foreach (var window in s_windowsToRemove)
            {
                WindowKey key = new WindowKey(window.GetType(), window.windowId);
                s_windows.Remove(key);
                if (s_hostElement != null)
                {
                    s_hostElement.hierarchy.Remove(window);
                }
                s_log.Trace($"Window unregistered from Panel: {window.imguiId}");
            }
            s_windowsToRemove.Clear();

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

        protected internal override void OnInitializeSelf()
        {
            s_hostElement = new WindowHostElement();
            ImTKTheme.onGlobalThemeChanged += OnGlobalThemeChanged;

            foreach (var window in s_windows.Values)
            {
                // Push existing windows to the add queue to be processed safely
                if (!s_windowsToAdd.Contains(window))
                {
                    s_windowsToAdd.Add(window);
                }
            }
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
            m_reservedAreas.Sort((a, b) => b.priority.CompareTo(a.priority)); // Higher priority first
        }

        protected internal override void OnGuiRender()
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();

            Rect currentRect = new Rect(viewport.WorkPos, viewport.WorkSize);

            foreach (var area in m_reservedAreas)
            {
                currentRect = area.func(currentRect);
            }

            // The remaining area is for the DockSpace
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

            try
            {
                foreach (var window in s_windows.Values)
                {
                    RenderEngine.ComputeStyleRecursive(window);
                    RenderEngine.ExecuteLayoutPhase(window);
                    RenderEngine.RenderNode(window);
                }
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Exception in Render of Windows");
            }

            if (pushedFont)
            {
                ImGui.PopFont();
                RenderingContext.PopFontState();
            }
        }

        protected internal override void OnClose()
        {
            ImTKTheme.onGlobalThemeChanged -= OnGlobalThemeChanged;
            s_hostElement = null;
        }
    }
}
