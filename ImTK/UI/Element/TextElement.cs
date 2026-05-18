using System;
using ImGuiNET;
using ImTK.UI.Style;

namespace ImTK.UI
{
    public class TextElement : VisualElement
    {
        public string text { get; set; }

        public TextElement(string text = "")
        {
            this.text = text;
            classList.Add("TextElement");
        }

        protected override void OnRenderSelf()
        {
            if (!string.IsNullOrEmpty(text))
            {
                ImGui.TextUnformatted(text);
            }
        }
    }
}
