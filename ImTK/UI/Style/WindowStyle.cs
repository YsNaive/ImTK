using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public class WindowStyle : VisualElementStyle
    {
        private int m_pushedColors = 0;

        public override void PushToImGui(ResolvedStyle resolvedStyle)
        {
            base.PushToImGui(resolvedStyle);

            m_pushedColors = 0;

            // Notice we do NOT override WindowBg here because it's already done by VisualElementStyle

            Color? titleBg = resolvedStyle.GetColor(ImTKStyleKey.HoverColor); // Originally mapped to TitleBg in old code
            if (titleBg.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.TitleBg, titleBg.Value.u32);
                ImGui.PushStyleColor(ImGuiCol.TitleBgActive, titleBg.Value.u32);
                m_pushedColors += 2;
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
