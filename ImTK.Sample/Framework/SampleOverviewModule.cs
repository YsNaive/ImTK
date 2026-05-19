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
        private Dictionary<string, List<ISampleScenario>> m_groupedScenarios = new Dictionary<string, List<ISampleScenario>>();
        private string m_searchQuery = "";

        public OverviewWindow() : base("ImTK Sample Overview")
        {
        }

        internal void SetScenarios(List<ISampleScenario> scenarios)
        {
            m_scenarios = scenarios;
            UpdateGrouping();
        }

        private void UpdateGrouping()
        {
            m_groupedScenarios.Clear();
            if (m_scenarios == null) return;

            var query = m_searchQuery.ToLowerInvariant();

            var filtered = m_scenarios.Where(s =>
                string.IsNullOrEmpty(query) ||
                (s.ScenarioName ?? "").ToLowerInvariant().Contains(query) ||
                (s.Description ?? "").ToLowerInvariant().Contains(query)
            );

            foreach (var scenario in filtered)
            {
                var category = string.IsNullOrEmpty(scenario.Category) ? "Uncategorized" : scenario.Category;
                if (!m_groupedScenarios.ContainsKey(category))
                {
                    m_groupedScenarios[category] = new List<ISampleScenario>();
                }
                m_groupedScenarios[category].Add(scenario);
            }

            foreach (var list in m_groupedScenarios.Values)
            {
                list.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
        }

        protected override void OnRenderSelf()
        {
            ImGui.Text("Welcome to the ImTK Sample Overview!");
            ImGui.TextWrapped("Select a scenario below to explore the capabilities of the ImTK framework.");
            ImGui.Spacing();

            if (ImGui.InputText("Search", ref m_searchQuery, 100))
            {
                UpdateGrouping();
            }

            ImGui.Separator();

            if (m_groupedScenarios == null || m_groupedScenarios.Count == 0)
            {
                ImGui.TextDisabled("No scenarios found.");
                return;
            }

            // Render categories sorted alphabetically
            var categories = m_groupedScenarios.Keys.ToList();
            categories.Sort();

            foreach (var category in categories)
            {
                if (ImGui.CollapsingHeader(category, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var scenarios = m_groupedScenarios[category];
                    foreach (var scenario in scenarios)
                    {
                        ImGui.PushID(scenario.ScenarioName);

                        ImGui.Indent();
                        ImGui.Spacing();

                        ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.7f, 1.0f, 1.0f), scenario.ScenarioName);
                        ImGui.TextWrapped(scenario.Description);

                        ImGui.Spacing();

                        if (ImGui.Button("Open Demo"))
                        {
                            scenario.Open();
                        }

                        if (!string.IsNullOrEmpty(scenario.DocumentationPath))
                        {
                            ImGui.SameLine();
                            if (ImGui.Button("View Source Doc"))
                            {
                                ImGui.SetClipboardText(scenario.DocumentationPath);
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip($"Path copied to clipboard:\n{scenario.DocumentationPath}");
                            }
                        }

                        // Render "See Also" links
                        if (scenario.SeeAlso != null && scenario.SeeAlso.Any())
                        {
                            ImGui.SameLine();
                            ImGui.Text("  See Also: ");

                            foreach (var type in scenario.SeeAlso)
                            {
                                ImGui.SameLine();
                                var target = m_scenarios.FirstOrDefault(s => s.GetType() == type);
                                if (target != null)
                                {
                                    if (ImGui.SmallButton(target.ScenarioName))
                                    {
                                        target.Open();
                                    }
                                }
                            }
                        }

                        ImGui.Spacing();
                        ImGui.Unindent();
                        ImGui.Separator();
                        ImGui.PopID();
                    }
                }
            }
        }
    }
}
