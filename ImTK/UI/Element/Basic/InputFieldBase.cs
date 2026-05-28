using System;
using Hexa.NET.ImGui;
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


            public StyleColor? hoverColor
            {
                get => GetPropertyColor(StyleKey.HoverColor);
                set => SetPropertyColor(StyleKey.HoverColor, value);
            }

            public StyleColor? activeColor
            {
                get => GetPropertyColor(StyleKey.ActiveColor);
                set => SetPropertyColor(StyleKey.ActiveColor, value);
            }

            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken)
                {
                    if (prop.key == VisualElement.StyleKey.ColorFamily.Hash)
                    {
                        string prefix = "--normal";
                        if (prop.enumValue == (int)ThemeColorFamily.Success) prefix = "--success";
                        else if (prop.enumValue == (int)ThemeColorFamily.Info) prefix = "--info";
                        else if (prop.enumValue == (int)ThemeColorFamily.Warning) prefix = "--warning";
                        else if (prop.enumValue == (int)ThemeColorFamily.Danger) prefix = "--danger";

                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.FrameBg, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component").Hash });
                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.FrameBgHovered, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-hover").Hash });
                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.FrameBgActive, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-active").Hash });
                    }
                    else if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.FrameBg;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.HoverColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.FrameBgHovered;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.ActiveColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.FrameBgActive;
                        output.Add(prop);
                        return;
                    }
                }
                base.ComputeHighlevelToken(prop, output);
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
