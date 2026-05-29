using System;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public class Button : TextElement<Button.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString HoverColor = new HashedString("HoverColor");
            public static readonly HashedString ActiveColor = new HashedString("ActiveColor");
        }

        public new class Style : VisualElement.Style
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

                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.Button, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component").Hash });
                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.ButtonHovered, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-hover").Hash });
                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.ButtonActive, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-active").Hash });
                    }
                    else if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
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

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            System.Numerics.Vector2 textSize = base.MeasureContent(constraint);
            
            var padding = ImGui.GetStyle().FramePadding;
            if (resolvedStyle.TryGetVector2((int)ImGuiStyleVar.FramePadding, out var overridePadding))
                padding = overridePadding;
            
            float frameHeight = ImGui.GetTextLineHeight() + padding.Y * 2;
            return new System.Numerics.Vector2(textSize.X + padding.X * 2, frameHeight);
        }

        public override void OnRender()
        {
            bool clicked = false;
            unsafe {
                if (!m_textBuffer.IsEmpty)
                {
                    clicked = ImGui.Button((byte*)m_textBuffer.Data, new System.Numerics.Vector2(layoutRect.width, layoutRect.height));
                }
                else
                {
                    clicked = ImGui.Button("", new System.Numerics.Vector2(layoutRect.width, layoutRect.height));
                }
            }

            if (clicked)
            {
                var evt = EventPool<ClickEvent>.Get();
                SendEvent(evt);
            }
        }
    }
}
