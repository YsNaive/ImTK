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


            public StyleValue<Color>? hoverColor
            {
                get => GetPropertyColor(StyleKey.HoverColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.HoverColor, value.Value);
                    else Clear(StyleKey.HoverColor);
                }
            }

            public StyleValue<Color>? activeColor
            {
                get => GetPropertyColor(StyleKey.ActiveColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.ActiveColor, value.Value);
                    else Clear(StyleKey.ActiveColor);
                }
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
