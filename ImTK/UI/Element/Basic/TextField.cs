using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class TextField : VisualElement<TextField.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
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

            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);

                m_pushedColors = 0;

                // TextField Background maps to FrameBg, not WindowBg/ChildBg
                Color? bgColor = resolvedStyle.GetColor(VisualElement.StyleKey.BackgroundColor);
                if (bgColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, bgColor.Value.u32);
                    m_pushedColors++;
                }

                Color? hoverColor = resolvedStyle.GetColor(StyleKey.HoverColor);
                if (hoverColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, hoverColor.Value.u32);
                    m_pushedColors++;
                }

                Color? activeColor = resolvedStyle.GetColor(StyleKey.ActiveColor);
                if (activeColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBgActive, activeColor.Value.u32);
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

        public string label { get; set; }

        public uint maxLength { get; set; }

        private string m_value;
        public string value
        {
            get => m_value;
            set => SetValue(value);
        }

        public event Action<ValueChangedEvent<string>> onValueChanged
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public TextField(string label = "", string defaultValue = "", uint maxLength = 1024)
        {
            this.label = label;
            m_value = defaultValue ?? string.Empty;
            this.maxLength = maxLength;
            classList.Add("TextField");
        }

        public void SetValueWithoutNotify(string newValue)
        {
            m_value = newValue ?? string.Empty;
        }

        private void SetValue(string newValue)
        {
            newValue = newValue ?? string.Empty;
            if (m_value == newValue) return;

            var evt = ValueChangedEvent<string>.GetPooled(m_value, newValue);
            m_value = newValue;
            SendEvent(evt);
        }

        protected override void OnRenderSelf()
        {
            string currentValue = m_value;
            if (ImGui.InputText(label, ref currentValue, maxLength))
            {
                SetValue(currentValue);
            }
        }
    }
}
