using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class Button : VisualElement<Button.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
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

        private string m_text = string.Empty;
        public string text 
        { 
            get => m_text; 
            set 
            {
                if (m_text != value)
                {
                    m_text = value ?? string.Empty;
                    MarkMeasureDirty();
                    MarkArrangeDirty();
                }
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
            return ImGuiNET.ImGui.IsItemHovered(ImGuiNET.ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            var textSize = ImGui.CalcTextSize(text);
            var padding = ImGui.GetStyle().FramePadding;
            if (resolvedStyle.TryGetVector2((int)ImGuiStyleVar.FramePadding, out var overridePadding))
                padding = overridePadding;
            
            float frameHeight = ImGui.GetTextLineHeight() + padding.Y * 2;
            return new System.Numerics.Vector2(textSize.X + padding.X * 2, frameHeight);
        }

        public override void OnRender()
        {
            if (ImGui.Button(text, new System.Numerics.Vector2(layoutRect.width, layoutRect.height)))
            {
                var evt = EventPool<ClickEvent>.Get();
                SendEvent(evt);
            }
        }
    }
}
