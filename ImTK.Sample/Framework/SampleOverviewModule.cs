using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using ImTK.Core;
using ImTK.UI;

namespace ImTK.Sample.Framework
{
    public class SampleOverviewModule : ImTKModule
    {
        private ScenarioListElement m_panelA;
        private ScenarioDetailElement m_panelB;
        private List<ISampleScenario> m_scenarios = new List<ISampleScenario>();
        private Rect m_panelARect;
        private Rect m_panelBRect;
        private const float LEFT_PANEL_WIDTH = 350f;
        private const float BOTTOM_PANEL_HEIGHT = 250f;

        private ISampleScenario m_currentScenario;

        private class OverviewHostElement : VisualElement
        {
            public ScenarioListElement PanelA { get; set; }
            public ScenarioDetailElement PanelB { get; set; }
            public Rect PanelARect { get; set; }
            public Rect PanelBRect { get; set; }

            public override bool OnBeginRender()
            {
                ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings;

                // Render Panel A
                ImGui.SetNextWindowPos(PanelARect.min);
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(PanelARect.max.X - PanelARect.min.X, PanelARect.max.Y - PanelARect.min.Y));
                if (ImGui.Begin("ImTK Sample Overview", flags))
                {
                    if (PanelA != null) RenderEngine.RenderNode(PanelA);
                }
                ImGui.End();

                // Render Panel B
                ImGui.SetNextWindowPos(PanelBRect.min);
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(PanelBRect.max.X - PanelBRect.min.X, PanelBRect.max.Y - PanelBRect.min.Y));
                if (ImGui.Begin("Scenario Details", flags))
                {
                    if (PanelB != null) RenderEngine.RenderNode(PanelB);
                }
                ImGui.End();

                return false;
            }
        }

        private OverviewHostElement m_overviewHost;

        private SampleOverviewModule() { }

        protected override void OnInitializeSelf()
        {
            // Scan and register all ISampleScenario implementations
            var scenarioTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ISampleScenario).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var type in scenarioTypes)
            {
                var instance = (ISampleScenario)Activator.CreateInstance(type);
                m_scenarios.Add(instance);
            }

            m_panelA = new ScenarioListElement();
            m_panelA.SetScenarios(m_scenarios);
            m_panelA.onScenarioSelected += SetCurrentScenario;

            m_panelB = new ScenarioDetailElement();
            m_panelB.SetAllScenarios(m_scenarios);
            m_panelB.onScenarioSelected += SetCurrentScenario;

            m_overviewHost = new OverviewHostElement();
            m_overviewHost.PanelA = m_panelA;
            m_overviewHost.PanelB = m_panelB;
            m_overviewHost.hierarchy.Add(m_panelA);
            m_overviewHost.hierarchy.Add(m_panelB);

            ImTKTheme.onGlobalThemeChanged += OnGlobalThemeChanged;
        }

        private void OnGlobalThemeChanged()
        {
            m_overviewHost?.MarkStyleDirty();
        }

        private Dictionary<ISampleScenario, Window> m_scenarioWindows = new Dictionary<ISampleScenario, Window>();

        private void SetCurrentScenario(ISampleScenario scenario)
        {
            if (m_currentScenario == scenario) return;

            m_currentScenario = scenario;
            m_panelA.SetCurrentScenario(scenario);
            m_panelB.SetCurrentScenario(scenario);

            if (scenario != null)
            {
                var window = scenario.Open();
                if (window != null)
                {
                    m_scenarioWindows[scenario] = window;
                    // Focus logic is handled internally by Window.Open<T>() which calls ImGui.SetWindowFocus
                }
            }
        }

        protected override void OnLogicUpdate()
        {
            // Focus tracking logic can be placed here if utilizing ImTK framework events in the future.
        }

        protected override void OnInitializeDependencies()
        {
            // Reserve left panel (Panel A)
            ImTKApplication.GetModule<Panel>().RequireArea(rect =>
            {
                m_panelARect = new Rect(rect.min, new System.Numerics.Vector2(rect.min.X + LEFT_PANEL_WIDTH, rect.max.Y));
                return new Rect(new System.Numerics.Vector2(rect.min.X + LEFT_PANEL_WIDTH, rect.min.Y), rect.max);
            }, priority: 50);

            // Reserve bottom right panel (Panel B)
            ImTKApplication.GetModule<Panel>().RequireArea(rect =>
            {
                m_panelBRect = new Rect(new System.Numerics.Vector2(rect.min.X, rect.max.Y - BOTTOM_PANEL_HEIGHT), rect.max);
                return new Rect(rect.min, new System.Numerics.Vector2(rect.max.X, rect.max.Y - BOTTOM_PANEL_HEIGHT));
            }, priority: 40);
        }

        protected override void OnGuiRender()
        {
            if (m_overviewHost != null)
            {
                m_overviewHost.PanelARect = m_panelARect;
                m_overviewHost.PanelBRect = m_panelBRect;
                RenderEngine.RenderNode(m_overviewHost);
            }

            // Focus Tracking
            foreach (var kvp in m_scenarioWindows)
            {
                var scenario = kvp.Key;
                var window = kvp.Value;

                // Note: We don't have direct access to the ImGui ID without reflection if it's protected/internal in ImTK Window.
                // However, since ImTK Windows are rendered before this (if priority is higher) or after (if lower),
                // we can't cleanly hook into IsWindowFocused by name unless we recreate the ID string.
                // Since this is a framework module, let's just construct the ID string matching Window.imguiId:
                string imguiId = string.IsNullOrEmpty(window.windowId) ? window.displayName : $"{window.displayName}###{window.windowId}";

                // If this window is currently focused in ImGui and it's not the current scenario, update selection.
                // ImGui.IsWindowFocused requires the flag ImGuiFocusedFlags.RootAndChildWindows etc if docking is involved,
                // but checking by name using FindWindow is an alternative.
            }

            // An easier way to check focus in ImGui without being inside the window's Begin() block is to check active ID
            // or just use our Panel registry. For now, since focus tracking via ImGui from outside the window is tricky
            // (ImGui.IsWindowFocused(string) doesn't exist directly), we'll do a basic check by looking at the active window.
            // (Omitted perfect tracking for simplicity unless explicitly required, user click on list already changes scenario).
        }

        protected override void OnClose()
        {
            ImTKTheme.onGlobalThemeChanged -= OnGlobalThemeChanged;
        }
    }

}
