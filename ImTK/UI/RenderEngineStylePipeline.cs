using System.Collections.Generic;

namespace ImTK.UI
{
    public static partial class RenderEngine
    {
        // 兩個靜態緩衝區，在整個 ComputeStyleRecursive 呼叫樹中重複使用，避免每個 dirty element 的 List 分配。
        // 合約：這兩個欄位只能在 ComputeStyleRecursiveInternal 內、且已消費完畢後才由子節點的遞迴清除。
        // 在 Debug build 中，s_isComputing guard 會在任何意外重入時立即拋出例外。
        private static readonly List<StyleProperty> s_composedProps   = new List<StyleProperty>();
        private static readonly List<StyleProperty> s_translatedProps = new List<StyleProperty>();

#if DEBUG
        private static bool s_isComputing = false;
#endif

        /// <summary>
        /// 遞迴計算 element 及其所有子節點的樣式。
        /// 此方法不可重入：在 ComputeHighlevelToken 或 Theme 解析路徑中不得呼叫 MarkStyleDirty 以觸發重新進入。
        /// </summary>
        public static void ComputeStyleRecursive(VisualElement element)
        {
#if DEBUG
            if (s_isComputing)
                throw new System.InvalidOperationException(
                    "ComputeStyleRecursive is not re-entrant. " +
                    "Do NOT call MarkStyleDirty or trigger style recomputation from within ComputeHighlevelToken or theme token resolution.");
            s_isComputing = true;
            try   { ComputeStyleRecursiveInternal(element); }
            finally { s_isComputing = false; }
#else
            ComputeStyleRecursiveInternal(element);
#endif
        }

        private static void ComputeStyleRecursiveInternal(VisualElement element)
        {
            if (element.m_isStyleDirty)
            {
                s_composedProps.Clear();

                // 1. Global Sheet
                foreach (var block in StyleSheet.Global.Blocks)
                {
                    if (element.classList.Has(block.ClassName))
                    {
                        foreach (var prop in block.Properties) SetComposedProperty(prop);
                    }
                }

                // 2. Ancestor Local Sheet
                var curr = element;
                StyleSheet activeLocalSheet = null;
                while (curr != null)
                {
                    if (curr.localStyleSheet != null)
                    {
                        activeLocalSheet = curr.localStyleSheet;
                        break;
                    }
                    curr = curr.parent;
                }

                if (activeLocalSheet != null)
                {
                    foreach (var block in activeLocalSheet.Blocks)
                    {
                        if (element.classList.Has(block.ClassName))
                        {
                            foreach (var prop in block.Properties) SetComposedProperty(prop);
                        }
                    }
                }

                // 3. Inline Style
                if (element.internalStyle is VisualElementStyle inlineStyle)
                {
                    foreach (var prop in inlineStyle.UnresolvedProperties)
                    {
                        SetComposedProperty(prop);
                    }
                }

                element.resolvedStyle.Clear();
                if (element.parent != null)
                {
                    element.resolvedStyle.CopyFrom(element.parent.resolvedStyle);
                }

                var theme = element.theme ?? ImTKTheme.GlobalTheme;

                foreach (var composedProp in s_composedProps)
                {
                    s_translatedProps.Clear();

                    if (element.internalStyle is VisualElementStyle styleComp)
                    {
                        styleComp.ComputeHighlevelToken(composedProp, s_translatedProps);
                    }
                    else
                    {
                        s_translatedProps.Add(composedProp);
                    }

                    foreach (var prop in s_translatedProps)
                    {
                        var finalProp = prop;

                        if (finalProp.category == StyleCategory.ThemeToken)
                        {
                            if (theme.TryGetColorToken(finalProp.tokenHash, out var color))
                            {
                                finalProp.category = StyleCategory.ImGuiStyle;
                                finalProp.dataType = StyleDataType.Color;
                                finalProp.colorValue = color.u32;
                            }
                            else if (theme.TryGetFloatToken(finalProp.tokenHash, out var floatVal))
                            {
                                finalProp.category = StyleCategory.ImGuiStyle;
                                finalProp.dataType = StyleDataType.Float;
                                finalProp.floatValue = floatVal;
                            }
                            else if (theme.TryGetVector2Token(finalProp.tokenHash, out var vec2Val))
                            {
                                finalProp.category = StyleCategory.ImGuiStyle;
                                finalProp.dataType = StyleDataType.Vector2;
                                finalProp.vector2Value = vec2Val;
                            }
                            else if (theme.TryGetHashToken(finalProp.tokenHash, out var hashVal))
                            {
                                finalProp.category = StyleCategory.ImGuiStyle;
                                finalProp.dataType = StyleDataType.HashedString;
                                finalProp.tokenHash = hashVal;
                            }
                        }

                        if (finalProp.category == StyleCategory.ImGuiStyle)
                        {
                            element.resolvedStyle.TrySetProperty(finalProp);
                        }
                    }
                }

                ImGuiStyleHandler.Diff(element.parent?.resolvedStyle, element.resolvedStyle, element.requiredStyle);
                element.m_isStyleDirty = false;
            }

            int childCount = element.hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                ComputeStyleRecursiveInternal(element.hierarchy.ChildAt(i));
            }
        }

        private static void SetComposedProperty(StyleProperty prop)
        {
            for (int i = 0; i < s_composedProps.Count; i++)
            {
                if (s_composedProps[i].key == prop.key)
                {
                    s_composedProps[i] = prop;
                    return;
                }
            }
            s_composedProps.Add(prop);
        }
    }
}
