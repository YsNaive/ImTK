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

            public StyleValue<float>? width
            {
                get => GetPropertyFloat(StyleKey.Width);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.Width, value.Value);
                    else Clear(StyleKey.Width);
                }
            }

            public StyleValue<float>? height
            {
                get => GetPropertyFloat(StyleKey.Height);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.Height, value.Value);
                    else Clear(StyleKey.Height);
                }
            }





            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken)
                {
                    if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.Button;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.HoverColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.ButtonHovered;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.ActiveColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.ButtonActive;
                        output.Add(prop);
                        return;
                    }
                }
                base.ComputeHighlevelToken(prop, output);
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

        public override void OnRender()
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
