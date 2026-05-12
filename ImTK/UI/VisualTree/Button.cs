using System;
using ImGuiNET;

namespace ImTK.UI
{
    public class Button : VisualElement
    {
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
