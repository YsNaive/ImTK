using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class CheckBox : VisualElement<CheckBox.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString HoverColor = new HashedString("HoverColor");
            public static readonly HashedString ActiveColor = new HashedString("ActiveColor");
            public static readonly HashedString CheckMarkColor = new HashedString("CheckMarkColor");
        }

        public new class Style : VisualElement.Style
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

            public StyleValue<Color>? checkMarkColor
            {
                get => GetPropertyColor(StyleKey.CheckMarkColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.CheckMarkColor, value.Value);
                    else Clear(StyleKey.CheckMarkColor);
                }
            }





            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken)
                {
                    if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
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
                    else if (prop.key == StyleKey.CheckMarkColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.CheckMark;
                        output.Add(prop);
                        return;
                    }
                }
                base.ComputeHighlevelToken(prop, output);
            }
}

        public string label { get; set; }

        private bool m_value;
        public bool value
        {
            get => m_value;
            set => SetValue(value);
        }

        public event Action<ValueChangedEvent<bool>> onValueChanged
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public CheckBox(string label = "", bool defaultValue = false)
        {
            this.label = label;
            m_value = defaultValue;
            classList.Add("check-box");
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            m_value = newValue;
        }

        private void SetValue(bool newValue)
        {
            if (m_value == newValue) return;

            var evt = ValueChangedEvent<bool>.GetPooled(m_value, newValue);
            m_value = newValue;
            SendEvent(evt);
        }

        public override void OnRender()
        {
            bool currentValue = m_value;
            if (ImGui.Checkbox(label, ref currentValue))
            {
                SetValue(currentValue);
            }
        }
    }
}
