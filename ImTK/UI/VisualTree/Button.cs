using System;
using ImGuiNET;
using ImTK.UI.Style;

namespace ImTK.UI
{
    public class Button : VisualElement
    {
        public class Mapping : StyleMapping
        {
            public Mapping()
            {
                colorTargets[(int)ImTKStyleKey.BackgroundColor] = (int)ImGuiCol.Button;
                colorTargets[(int)ImTKStyleKey.HoverColor] = (int)ImGuiCol.ButtonHovered;
                colorTargets[(int)ImTKStyleKey.ActiveColor] = (int)ImGuiCol.ButtonActive;
                colorTargets[(int)ImTKStyleKey.TextColor] = (int)ImGuiCol.Text;
            }
        }

        public string text { get; set; }

        public event Action<ClickEvent> onClicked
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public Button(string text = "", Action<ClickEvent> onClicked = null)
        {
            this.text = text;
            if (onClicked != null)
            {
                this.onClicked += onClicked;
            }
            classList.Add("Button");
        }

        protected override void OnRenderSelf()
        {
            if (ImGui.Button(text))
            {
                var evt = EventPool<ClickEvent>.Get();
                SendEvent(evt);
            }
        }
    }
}
