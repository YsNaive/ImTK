using Hexa.NET.ImGui;
using ImTK.Core;
using System.Collections.Generic;

namespace ImTK.UI
{
    public class VisualElementStyle
    {
        private readonly List<StyleProperty> m_unresolvedProperties = new List<StyleProperty>();

        public IReadOnlyList<StyleProperty> UnresolvedProperties => m_unresolvedProperties;

        public bool SetProperty(StyleProperty prop)
        {
            for (int i = 0; i < m_unresolvedProperties.Count; i++)
            {
                if (m_unresolvedProperties[i].key == prop.key)
                {
                    ref var existing = ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(m_unresolvedProperties)[i];
                    if (existing.category == prop.category && 
                        existing.dataType == prop.dataType &&
                        existing.flags == prop.flags &&
                        existing.floatValue == prop.floatValue &&
                        existing.vector2Value == prop.vector2Value &&
                        existing.colorValue == prop.colorValue &&
                        existing.tokenHash == prop.tokenHash &&
                        existing.intValue == prop.intValue &&
                        existing.enumValue == prop.enumValue)
                    {
                        return false;
                    }
                    m_unresolvedProperties[i] = prop;
                    return true;
                }
            }
            m_unresolvedProperties.Add(prop);
            return true;
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

            if (prop.isInheritable)
            {
                output.Add(prop);
            }

            bool mapped = true;
            if (prop.key == VisualElement.StyleKey.ColorFamily.Hash)
            {
                if (!prop.isInheritable)
                {
                    prop.isInheritable = true;
                    output.Add(prop);
                }

                string prefix = "--normal";
                if (prop.enumValue == (int)ThemeColorFamily.Success) prefix = "--success";
                else if (prop.enumValue == (int)ThemeColorFamily.Info) prefix = "--info";
                else if (prop.enumValue == (int)ThemeColorFamily.Warning) prefix = "--warning";
                else if (prop.enumValue == (int)ThemeColorFamily.Danger) prefix = "--danger";

                output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.WindowBg, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-surface").Hash });
                output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.ChildBg, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-surface").Hash });
                output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.PopupBg, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-container").Hash });
                output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.Border, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-border").Hash });
                output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.Text, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-text").Hash, isInheritable = true });
                output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.TextDisabled, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-disabled-text").Hash, isInheritable = true });
                return;
            }
            else if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
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
                output.Add(prop);

                var prop2 = prop;
                prop2.key = (int)ImGuiStyleVar.FrameBorderSize;
                output.Add(prop2);

                var prop3 = prop;
                prop3.key = (int)ImGuiStyleVar.PopupBorderSize;
                output.Add(prop3);

                var prop4 = prop;
                prop4.key = (int)ImGuiStyleVar.ChildBorderSize;
                output.Add(prop4);

                return;
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

            else if (prop.key == VisualElement.StyleKey.ItemSpacing.Hash)
            {
                prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                prop.key = (int)ImGuiStyleVar.ItemSpacing;
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
                prop.key = ImGuiStyleHandler.s_fontSizeImGuiKey.Hash;
                prop.isInheritable = true;
            }
            else if (prop.key == VisualElement.StyleKey.Width.Hash ||
                     prop.key == VisualElement.StyleKey.Height.Hash ||
                     prop.key == VisualElement.StyleKey.MinWidth.Hash ||
                     prop.key == VisualElement.StyleKey.MaxWidth.Hash ||
                     prop.key == VisualElement.StyleKey.MinHeight.Hash ||
                     prop.key == VisualElement.StyleKey.MaxHeight.Hash ||
                     prop.key == VisualElement.StyleKey.MarginLeft.Hash ||
                     prop.key == VisualElement.StyleKey.MarginTop.Hash ||
                     prop.key == VisualElement.StyleKey.MarginRight.Hash ||
                     prop.key == VisualElement.StyleKey.MarginBottom.Hash ||
                     prop.key == VisualElement.StyleKey.PaddingLeft.Hash ||
                     prop.key == VisualElement.StyleKey.PaddingTop.Hash ||
                     prop.key == VisualElement.StyleKey.PaddingRight.Hash ||
                     prop.key == VisualElement.StyleKey.PaddingBottom.Hash ||
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
                     prop.key == VisualElement.StyleKey.Right.Hash ||
                     prop.key == VisualElement.StyleKey.Display.Hash ||
                     prop.key == VisualElement.StyleKey.Overflow.Hash)
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
