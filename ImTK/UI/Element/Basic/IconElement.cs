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
            if (m_type == IconType.DownArrow)
            {
                ImGui.ArrowButton("##Icon", ImGuiDir.Down);
            }
            else if (m_type == IconType.RightArrow)
            {
                ImGui.ArrowButton("##Icon", ImGuiDir.Right);
            }
        }
    }
}
