using ImTK.Core;
using ImTK.UI;
using ImTK.Log;
using ImGuiNET;
using System.Numerics;

namespace ImTK.Test
{
    public class TestMenuBarModule : ImTKModule
    {
        private TestMenuBarModule() { }

        protected override void OnInitializeSelf()
        {
            var panel = ImTKApplication.GetModule<Panel>();
            panel.RequireArea(ReserveTopMenuBar, 100);
        }

        protected override void OnInitializeDependencies()
        {
        }

        private ImRect ReserveTopMenuBar(ImRect currentSpace)
        {
            float height = 30f;
            ImRect reserved = new ImRect(currentSpace.min, new Vector2(currentSpace.max.X, currentSpace.min.Y + height));
            // Return remaining space
            return new ImRect(new Vector2(currentSpace.min.X, currentSpace.min.Y + height), currentSpace.max);
        }

        protected override void OnGuiRender()
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();

            // Re-calculate the position just like reserved, this time to draw
            ImRect currentRect = new ImRect(viewport.WorkPos, new Vector2(viewport.WorkPos.X + viewport.WorkSize.X, viewport.WorkPos.Y + viewport.WorkSize.Y));

            ImGui.SetNextWindowPos(currentRect.min);
            ImGui.SetNextWindowSize(new Vector2(currentRect.max.X - currentRect.min.X, 30f));
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5.0f, 5.0f));
            ImGui.Begin("TestMenuBarWindow", windowFlags);
            ImGui.Text("Top Reserved Menu Bar Module");
            ImGui.End();
            ImGui.PopStyleVar();
        }

        protected override void OnClose()
        {
        }
    }

    public class TestUIModule : ImTKModule
    {
        private static readonly LogContext s_log = new LogContext("TestUIModule");
        private VisualElement m_root;

        private TestUIModule() { }

        protected override void OnInitializeSelf()
        {
        }

        protected override void OnInitializeDependencies()
        {
            m_root = new VisualElement();

            var composite = new TestCompositeContainer("Outer Composite");
            var button1 = new TestButtonElement("Button 1 (Inside Composite)");
            var button2 = new TestButtonElement("Button 2 (Directly on Root)");

            button1.RegisterCallback<ClickEvent>(e => s_log.Info($"Button 1 Clicked! Handled by: {(e.current is TestButtonElement ? "Button1" : "Unknown")}"));
            button1.RegisterCallback<MouseEnterEvent>(e => s_log.Info("Button 1 Mouse Enter!"));
            button1.RegisterCallback<MouseLeaveEvent>(e => s_log.Info("Button 1 Mouse Leave!"));

            // Bubbling test
            composite.RegisterCallback<ClickEvent>(e =>
            {
                s_log.Info($"Composite Container received bubbling click from {(e.source is TestButtonElement btn ? "Button" : "Unknown")}!");
            });

            composite.Add(button1);
            m_root.Add(composite);
            m_root.Add(button2);
        }

        protected override void OnGuiRender()
        {
            ImGui.Begin("VisualElement Test Window");
            m_root.InternalRender();
            ImGui.End();
        }

        protected override void OnClose()
        {
        }
    }
}
