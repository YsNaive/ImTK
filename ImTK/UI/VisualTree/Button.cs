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

        protected override void ApplyTheme(ImTKTheme theme)
        {
            base.ApplyTheme(theme);
            style.ApplyThemeColor(ImGuiCol.Button, theme.PrimaryColor);

            Color primary = theme.PrimaryColor;
            Color hover = primary;
            hover.v = Math.Min(1.0f, primary.v + 0.1f); // Brighten slightly for hover

            Color active = primary;
            active.v = Math.Max(0.0f, primary.v - 0.1f); // Darken slightly for active

            style.ApplyThemeColor(ImGuiCol.ButtonHovered, hover);
            style.ApplyThemeColor(ImGuiCol.ButtonActive, active);
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
