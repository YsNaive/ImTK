using System;
using System.Numerics;
using ImGuiNET;

namespace ImTK.UI
{
    public class IconElement : VisualElement
    {
        public enum IconType
        {
            Null,
            None,
            DownArrow,
            RightArrow
        }

        private IconType m_type = IconType.None;
        public IconType type
        {
            get => m_type;
            set
            {
                if (m_type != value)
                {
                    m_type = value;
                    this.style.display = m_type == IconType.Null ? DisplayStyle.None : DisplayStyle.Flex;
                    MarkMeasureDirty();
                }
            }
        }

        public IconElement()
        {
            this.style.display = m_type == IconType.Null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        protected override Vector2 MeasureContent(LayoutConstraint constraint)
        {
            float frameHeight = ImGui.GetFrameHeight();
            return new Vector2(frameHeight, frameHeight);
        }

        public override void OnRender()
        {
            if (m_type == IconType.DownArrow || m_type == IconType.RightArrow)
            {
                var drawList = ImGui.GetWindowDrawList();
                float size = ImGui.GetFontSize() * 0.6f;
                Vector2 center = this.layoutRect.position + this.layoutRect.size * 0.5f;

                uint color = ImGui.GetColorU32(ImGuiCol.Text);

                if (m_type == IconType.DownArrow)
                {
                    Vector2 p1 = center + new Vector2(-size * 0.5f, -size * 0.25f);
                    Vector2 p2 = center + new Vector2(size * 0.5f, -size * 0.25f);
                    Vector2 p3 = center + new Vector2(0, size * 0.5f);
                    drawList.AddTriangleFilled(p1, p2, p3, color);
                }
                else if (m_type == IconType.RightArrow)
                {
                    Vector2 p1 = center + new Vector2(-size * 0.2f, -size * 0.4f);
                    Vector2 p2 = center + new Vector2(-size * 0.2f, size * 0.4f);
                    Vector2 p3 = center + new Vector2(size * 0.35f, 0);
                    drawList.AddTriangleFilled(p1, p2, p3, color);
                }
            }
        }
    }
}
