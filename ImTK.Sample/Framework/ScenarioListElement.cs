using System;
using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;
using ImTK.UI;

namespace ImTK.Sample.Framework
{
    public class ScenarioListElement : VisualElement
    {
        private List<ISampleScenario> m_scenarios;
        private Dictionary<string, List<ISampleScenario>> m_groupedScenarios = new Dictionary<string, List<ISampleScenario>>();
        private string m_searchQuery = "";

        public ISampleScenario currentScenario { get; private set; }
        public event Action<ISampleScenario> onScenarioSelected;

        public void SetScenarios(List<ISampleScenario> scenarios)
        {
            m_scenarios = scenarios;
            UpdateGrouping();
        }

        public void SetCurrentScenario(ISampleScenario scenario)
        {
            currentScenario = scenario;
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

        public override void OnRender()
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
            }
            else
            {
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

                            bool isSelected = currentScenario == scenario;
                            if (ImGui.Selectable(scenario.ScenarioName, isSelected))
                            {
                                onScenarioSelected?.Invoke(scenario);
                            }

                            ImGui.PopID();
                        }
                    }
                }
            }
        }
    }
}
