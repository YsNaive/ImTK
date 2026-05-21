using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public abstract class InputFieldBase<TValue, TStyle> : VisualElement<TStyle>
        where TStyle : InputFieldBase<TValue, TStyle>.InputFieldStyle, new()
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString HoverColor = new HashedString("HoverColor");
            public static readonly HashedString ActiveColor = new HashedString("ActiveColor");
        }

        public abstract class InputFieldStyle : VisualElement.Style
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

                // InputField Background maps to FrameBg, not WindowBg/ChildBg
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

        private TValue m_value;
        public TValue value
        {
            get => m_value;
            set => SetValue(value);
        }

        public event Action<ValueChangedEvent<TValue>> onValueChanged
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        protected InputFieldBase(string label = "", TValue defaultValue = default)
        {
            this.label = label;
            m_value = SanitizeValue(defaultValue);
        }

        public void SetValueWithoutNotify(TValue newValue)
        {
            m_value = SanitizeValue(newValue);
        }

        protected void SetValue(TValue newValue)
        {
            newValue = SanitizeValue(newValue);
            if (System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(m_value, newValue))
                return;

            var evt = ValueChangedEvent<TValue>.GetPooled(m_value, newValue);
            m_value = newValue;
            SendEvent(evt);
        }

        protected virtual TValue SanitizeValue(TValue value)
        {
            return value;
        }
    }
}
