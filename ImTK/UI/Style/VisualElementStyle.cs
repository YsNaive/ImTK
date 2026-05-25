using System.Collections.Generic;
using ImGuiNET;

namespace ImTK.UI
{
    public class VisualElementStyle
    {
        private readonly List<StyleProperty> m_unresolvedProperties = new List<StyleProperty>();

        public IEnumerable<StyleProperty> UnresolvedProperties => m_unresolvedProperties;

        public void SetProperty(StyleProperty prop)
        {
            for (int i = 0; i < m_unresolvedProperties.Count; i++)
            {
                if (m_unresolvedProperties[i].key == prop.key)
                {
                    m_unresolvedProperties[i] = prop;
                    return;
                }
            }
            m_unresolvedProperties.Add(prop);
        }

        public StyleProperty GetProperty(int key)
        {
            foreach (var prop in m_unresolvedProperties)
            {
                if (prop.key == key) return prop;
            }
            return new StyleProperty { dataType = StyleDataType.Null };
        }

        public void Clear()
        {
            m_unresolvedProperties.Clear();
        }

        public virtual void ComputeHighlevelToken(StyleProperty prop, IList<StyleProperty> output)
        {
            if (prop.category != StyleCategory.HighLevelToken)
            {
                output.Add(prop);
                return;
            }

            bool mapped = true;
            if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiCol.WindowBg;
            }
            else if (prop.key == VisualElement.StyleKey.TextColor.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiCol.Text;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.DisabledTextColor.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiCol.TextDisabled;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.BorderColor.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiCol.Border;
            }
            else if (prop.key == VisualElement.StyleKey.BorderWidth.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.WindowBorderSize;
            }
            else if (prop.key == VisualElement.StyleKey.BorderRadius.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.WindowRounding;
                output.Add(prop);

                var prop2 = prop;
                prop2.key = (int)ImGuiStyleVar.FrameRounding;
                output.Add(prop2);

                var prop3 = prop;
                prop3.key = (int)ImGuiStyleVar.PopupRounding;
                output.Add(prop3);

                var prop4 = prop;
                prop4.key = (int)ImGuiStyleVar.ChildRounding;
                output.Add(prop4);

                return; // return early since we output multiple
            }
            else if (prop.key == VisualElement.StyleKey.Padding.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.WindowPadding;
                output.Add(prop);
                
                var prop2 = prop;
                prop2.key = (int)ImGuiStyleVar.FramePadding;
                output.Add(prop2);
                
                return;
            }
            else if (prop.key == VisualElement.StyleKey.ItemSpacing.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.ItemSpacing;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.ItemInnerSpacing.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.ItemInnerSpacing;
            }
            else if (prop.key == VisualElement.StyleKey.Alpha.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.Alpha;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.DisabledAlpha.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = ImTKTheme.Tokens.DisabledAlpha.Hash;
            }
            else if (prop.key == VisualElement.StyleKey.SelectionColor.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiCol.TextSelectedBg;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.FontFamily.Hash)
            {
                prop.category = StyleCategory.ImGuiStyle;
                prop.key = ImGuiStyleHandler.s_fontFamilyImGuiKey.Hash;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.FontSize.Hash)
            {
                prop.category = StyleCategory.ImGuiStyle;
                prop.dataType = StyleDataType.Float;
                prop.floatValue = prop.intValue;
                prop.key = ImGuiStyleHandler.s_fontSizeImGuiKey.Hash;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.Width.Hash ||
                     prop.key == VisualElement.StyleKey.Height.Hash ||
                     prop.key == VisualElement.StyleKey.MinWidth.Hash ||
                     prop.key == VisualElement.StyleKey.MaxWidth.Hash ||
                     prop.key == VisualElement.StyleKey.MinHeight.Hash ||
                     prop.key == VisualElement.StyleKey.MaxHeight.Hash ||
                     prop.key == VisualElement.StyleKey.Margin.Hash ||
                     prop.key == VisualElement.StyleKey.FlexDirection.Hash ||
                     prop.key == VisualElement.StyleKey.FlexWrap.Hash ||
                     prop.key == VisualElement.StyleKey.JustifyContent.Hash ||
                     prop.key == VisualElement.StyleKey.AlignItems.Hash ||
                     prop.key == VisualElement.StyleKey.FlexGrow.Hash ||
                     prop.key == VisualElement.StyleKey.AlignSelf.Hash ||
                     prop.key == VisualElement.StyleKey.PositionType.Hash ||
                     prop.key == VisualElement.StyleKey.Top.Hash ||
                     prop.key == VisualElement.StyleKey.Bottom.Hash ||
                     prop.key == VisualElement.StyleKey.Left.Hash ||
                     prop.key == VisualElement.StyleKey.Right.Hash)
            {
                prop.category = StyleCategory.Layout;
            }
            else
            {
                mapped = false;
            }

            if (mapped) output.Add(prop);
        }
    }
}
