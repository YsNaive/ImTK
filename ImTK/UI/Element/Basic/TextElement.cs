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
            set => m_text = value ?? string.Empty;
        }

        public TextElement(string text = "")
        {
            this.text = text;
            classList.Add("text-element");
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
