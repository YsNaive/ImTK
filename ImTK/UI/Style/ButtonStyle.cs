using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public class ButtonStyle : VisualElementStyle
    {
        private int m_pushedColors = 0;

        public override void PushToImGui(ResolvedStyle resolvedStyle)
        {
            base.PushToImGui(resolvedStyle);

            m_pushedColors = 0;

            Color? bgColor = resolvedStyle.GetColor(ImTKStyleKey.BackgroundColor);
            if (bgColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, bgColor.Value.u32);
                m_pushedColors++;
            }

            Color? hoverColor = resolvedStyle.GetColor(ImTKStyleKey.HoverColor);
            if (hoverColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoverColor.Value.u32);
                m_pushedColors++;
            }

            Color? activeColor = resolvedStyle.GetColor(ImTKStyleKey.ActiveColor);
            if (activeColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, activeColor.Value.u32);
                m_pushedColors++;
            }
        }

        public override void PopFromImGui()
        {
            if (m_pushedColors > 0)
            {
                ImGui.PopStyleColor(m_pushedColors);
                m_pushedColors = 0;
            }
            base.PopFromImGui();
        }
    }
}
