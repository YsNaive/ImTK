using System;
using Hexa.NET.ImGui;

namespace ImTK.UI
{
    public class TextElement : VisualElement
    {
        private bool m_enableWordWrap = true;
        public bool enableWordWrap
        {
            get => m_enableWordWrap;
            set
            {
                if (m_enableWordWrap != value)
                {
                    m_enableWordWrap = value;
                    MarkMeasureDirty();
                    MarkArrangeDirty();
                }
            }
        }

        private string m_text = string.Empty;
        public string text
        {
            get => m_text;
            set 
            {
                if (m_text != value)
                {
                    m_text = value ?? string.Empty;
                    MarkMeasureDirty();
                    MarkArrangeDirty();
                }
            }
        }

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            if (string.IsNullOrEmpty(m_text)) return System.Numerics.Vector2.Zero;
            
            if (enableWordWrap && constraint.WidthMode != MeasureMode.Undefined && constraint.AvailableWidth > 0)
            {
                System.Numerics.Vector2 size;
                unsafe {
                    fixed (byte* pText = System.Text.Encoding.UTF8.GetBytes(m_text + "\0")) {
                        size = ImGui.CalcTextSize(pText, (byte*)null, false, constraint.AvailableWidth);
                    }
                }
                return size;
            }
            else
            {
                System.Numerics.Vector2 size;
                unsafe {
                    fixed (byte* pText = System.Text.Encoding.UTF8.GetBytes(m_text + "\0")) {
                        size = ImGui.CalcTextSize(pText);
                    }
                }
                return new System.Numerics.Vector2(size.X, ImGui.GetTextLineHeight());
            }
        }

        public TextElement(string text = "")
        {
            this.text = text;
            classList.Add("text-element");
        }

                protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        public override void OnRender()
        {
            if (!string.IsNullOrEmpty(m_text))
            {
                if (enableWordWrap)
                {
                    ImGui.PushTextWrapPos(Math.Max(1.0f, layoutRect.width));
                    ImGui.TextUnformatted(m_text);
                    ImGui.PopTextWrapPos();
                }
                else
                {
                    ImGui.TextUnformatted(m_text);
                }
            }
        }
    }
}
