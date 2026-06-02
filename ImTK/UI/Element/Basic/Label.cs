using System;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public class Label : TextElement
    {
        public new class StyleKey : TextElement.StyleKey { }
        public new class Style : TextElement.Style { }

        public Label(string text = "") : base(text)
        {
            this.enableWordWrap = false;
            this.style.overflow = Overflow.Hidden;
            classList.Add("label");
            classList.Remove("text-element");
        }
    }
}
