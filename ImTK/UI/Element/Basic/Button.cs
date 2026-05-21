using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class Button : VisualElement<Button.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString Width = new HashedString("Width");
            public static readonly HashedString Height = new HashedString("Height");
            public static readonly HashedString HoverColor = new HashedString("HoverColor");
            public static readonly HashedString ActiveColor = new HashedString("ActiveColor");
        }

        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            public StyleValue<Color>? hoverColor
            {
                get => GetOverrideColor(StyleKey.HoverColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.HoverColor, value.Value);
                    else Clear(StyleKey.HoverColor);
                }
            }

            public StyleValue<Color>? activeColor
            {
                get => GetOverrideColor(StyleKey.ActiveColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.ActiveColor, value.Value);
                    else Clear(StyleKey.ActiveColor);
                }
            }

            public StyleValue<float>? width
            {
                get => GetOverrideFloat(StyleKey.Width);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.Width, value.Value);
                    else Clear(StyleKey.Width);
                }
            }

            public StyleValue<float>? height
            {
                get => GetOverrideFloat(StyleKey.Height);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.Height, value.Value);
                    else Clear(StyleKey.Height);
                }
            }

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

                Color? hoverColor = resolvedStyle.GetColor(StyleKey.HoverColor);
                if (hoverColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoverColor.Value.u32);
                    m_pushedColors++;
                }

                Color? activeColor = resolvedStyle.GetColor(StyleKey.ActiveColor);
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
            float width = resolvedStyle.GetFloat(StyleKey.Width) ?? 0f;
            float height = resolvedStyle.GetFloat(StyleKey.Height) ?? 0f;

            if (ImGui.Button(text, new System.Numerics.Vector2(width, height)))
            {
                var evt = EventPool<ClickEvent>.Get();
                SendEvent(evt);
            }
        }
    }
}
