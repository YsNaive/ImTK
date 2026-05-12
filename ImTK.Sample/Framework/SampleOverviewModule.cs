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
        private OverviewWindow m_overviewWindow;
        private List<ISampleScenario> m_scenarios = new List<ISampleScenario>();

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

            m_overviewWindow = Window.Open<OverviewWindow>();
            m_overviewWindow.SetScenarios(m_scenarios);
        }
    }

    public class OverviewWindow : Window
    {
        private List<ISampleScenario> m_scenarios;

        public OverviewWindow() : base("ImTK Sample Overview")
        {
        }

        internal void SetScenarios(List<ISampleScenario> scenarios)
        {
            m_scenarios = scenarios;
        }

        protected override void OnRenderSelf()
        {
            if (m_scenarios == null) return;

            ImGui.Text("Welcome to the ImTK Sample Overview!");
            ImGui.TextWrapped("Select a scenario below to explore the capabilities of the ImTK framework.");
            ImGui.Separator();

            foreach (var scenario in m_scenarios)
            {
                ImGui.PushID(scenario.ScenarioName);

                ImGui.TextDisabled(scenario.ScenarioName);
                ImGui.TextWrapped(scenario.Description);

                if (ImGui.Button("Open Demo"))
                {
                    scenario.Open();
                }

                if (!string.IsNullOrEmpty(scenario.DocumentationPath))
                {
                    ImGui.SameLine();
                    if (ImGui.Button("View Source Doc"))
                    {
                        // Fallback logic to open doc, or just copy path
                        ImGui.SetClipboardText(scenario.DocumentationPath);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"Path copied to clipboard:\n{scenario.DocumentationPath}");
                    }
                }

                ImGui.Separator();
                ImGui.PopID();
            }
        }
    }
}
