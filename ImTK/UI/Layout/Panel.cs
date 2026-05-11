using System;
using System.Collections.Generic;
using ImTK.Core;
using ImTK.Log;
using ImGuiNET;
using System.Numerics;

namespace ImTK.UI
{
    public struct ImRect
    {
        public Vector2 min;
        public Vector2 max;
        public ImRect(Vector2 min, Vector2 max) { this.min = min; this.max = max; }
    }

    public class Panel : ImTKModule
    {
        private static readonly LogContext s_log = new LogContext("Panel");
        private readonly List<(Func<ImRect, ImRect> func, int priority)> m_reservedAreas = new();

        protected Panel() { }

        protected internal override void OnInitializeSelf()
        {
        }

        protected internal override void OnInitializeDependencies()
        {
        }

        public void RequireArea(Func<ImRect, ImRect> reservedFunc, int priority = 0)
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

            ImRect currentRect = new ImRect(viewport.WorkPos, new Vector2(viewport.WorkPos.X + viewport.WorkSize.X, viewport.WorkPos.Y + viewport.WorkSize.Y));

            foreach (var area in m_reservedAreas)
            {
                currentRect = area.func(currentRect);
            }

            // The remaining area is for the DockSpace
            ImGui.SetNextWindowPos(currentRect.min);
            ImGui.SetNextWindowSize(new Vector2(currentRect.max.X - currentRect.min.X, currentRect.max.Y - currentRect.min.Y));
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

            ImGui.Begin("MainDockSpaceWindow", windowFlags);

            ImGui.PopStyleVar(3);

            var io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0)
            {
                uint dockspaceId = ImGui.GetID("MainDockSpace");
                ImGui.DockSpace(dockspaceId, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.PassthruCentralNode);
            }

            ImGui.End();
        }

        protected internal override void OnClose()
        {
        }
    }
}
