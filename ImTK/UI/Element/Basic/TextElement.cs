using System;
using ImGuiNET;

namespace ImTK.UI
{
    public class TextElement : VisualElement
    {
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
            // TODO: If constraint.WidthMode is Exactly or AtMost, we might need text wrapping support here.
            // For now, return the basic text size (1-line).
            var size = ImGui.CalcTextSize(m_text);
            return new System.Numerics.Vector2(size.X, ImGui.GetTextLineHeight());
        }

        public TextElement(string text = "")
        {
            this.text = text;
            classList.Add("text-element");
        }

                protected internal override bool CheckHoverState()
        {
            return ImGuiNET.ImGui.IsItemHovered(ImGuiNET.ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        public override void OnRender()
        {
            if (!string.IsNullOrEmpty(m_text))
            {
                ImGui.TextUnformatted(m_text);
            }
        }
    }
}
