using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using ImTK.UI;

namespace ImTK.Sample.Framework
{
    public class ScenarioDetailElement : VisualElement
    {
        private ISampleScenario m_currentScenario;
        private List<ISampleScenario> m_allScenarios;

        public event Action<ISampleScenario> onScenarioSelected;

        public void SetAllScenarios(List<ISampleScenario> scenarios)
        {
            m_allScenarios = scenarios;
        }

        public void SetCurrentScenario(ISampleScenario scenario)
        {
            m_currentScenario = scenario;
        }

        public override void OnRender()
        {
            if (m_currentScenario == null)
            {
                ImGui.TextDisabled("No scenario selected.");
                return;
            }

            ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.7f, 1.0f, 1.0f), m_currentScenario.ScenarioName);
            ImGui.Separator();

            if (!string.IsNullOrEmpty(m_currentScenario.DocumentationPath))
            {
                ImGui.TextDisabled($"Doc: {m_currentScenario.DocumentationPath}");
                ImGui.SameLine();
                if (ImGui.SmallButton("Copy"))
                {
                    ImGui.SetClipboardText(m_currentScenario.DocumentationPath);
                }
            }

            // Render "See Also" links
            if (m_currentScenario.SeeAlso != null && m_currentScenario.SeeAlso.Any())
            {
                ImGui.Text("See Also: ");

                foreach (var type in m_currentScenario.SeeAlso)
                {
                    ImGui.SameLine();
                    var target = m_allScenarios.FirstOrDefault(s => s.GetType() == type);
                    if (target != null)
                    {
                        if (ImGui.SmallButton(target.ScenarioName))
                        {
                            onScenarioSelected?.Invoke(target);
                        }
                    }
                }
            }

            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextWrapped(m_currentScenario.Description);
        }
    }
}
