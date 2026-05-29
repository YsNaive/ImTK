using System;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public class TextElement : TextElement<TextElement.Style>
    {
        public new class StyleKey : VisualElement.StyleKey { }
        public new class Style : VisualElement.Style { }

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
            if (!m_textBuffer.IsEmpty)
            {
                if (enableWordWrap)
                {
                    ImGui.PushTextWrapPos(Math.Max(1.0f, layoutRect.width));
                    unsafe { ImGui.TextUnformatted((byte*)m_textBuffer.Data); }
                    ImGui.PopTextWrapPos();
                }
                else
                {
                    unsafe { ImGui.TextUnformatted((byte*)m_textBuffer.Data); }
                }
            }
        }
    }
}
