using System;
using ImGuiNET;
using ImTK.UI.Style;
using ImTK.Core;

namespace ImTK.UI
{
    public class Button : VisualElement<Button.Style>
    {
        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);

                m_pushedColors = 0;

                Color? bgColor = resolvedStyle.GetColor(VisualElement.StyleKey.BackgroundColor);
                if (bgColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, bgColor.Value.u32);
                    m_pushedColors++;
                }

                Color? hoverColor = resolvedStyle.GetColor(VisualElement.StyleKey.HoverColor);
                if (hoverColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoverColor.Value.u32);
                    m_pushedColors++;
                }

                Color? activeColor = resolvedStyle.GetColor(VisualElement.StyleKey.ActiveColor);
                if (activeColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, activeColor.Value.u32);
                    m_pushedColors++;
                }
            }

            public override void PopFromImGui()
            {
                if (m_pushedColors > 0)
                {
                    ImGui.PopStyleColor(m_pushedColors);
                    m_pushedColors = 0;
                }
                base.PopFromImGui();
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
