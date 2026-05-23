using System.Collections.Generic;

namespace ImTK.UI
{
    public static partial class RenderEngine
    {
        private static readonly List<StyleProperty> s_composedUnresolvedProperties = new List<StyleProperty>();

        public static void ComputeStyleRecursive(VisualElement element)
        {
            if (element.m_isStyleDirty)
            {
                s_composedUnresolvedProperties.Clear();

                if (element.localStyleSheet != null)
                {
                    foreach (var block in element.localStyleSheet.Blocks)
                    {
                        if (element.classList.Has(block.ClassName))
                        {
                            foreach (var prop in block.Properties)
                            {
                                SetComposedProperty(prop);
                            }
                        }
                    }
                }

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
                var translatedProps = new List<StyleProperty>();

                foreach (var composedProp in s_composedUnresolvedProperties)
                {
                    translatedProps.Clear();

                    if (element.internalStyle is VisualElementStyle styleComp)
                    {
                        styleComp.ComputeHighlevelToken(composedProp, translatedProps);
                    }
                    else
                    {
                        translatedProps.Add(composedProp);
                    }

                    foreach (var prop in translatedProps)
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
                ComputeStyleRecursive(element.hierarchy.childAt(i));
            }
        }

        private static void SetComposedProperty(StyleProperty prop)
        {
            for (int i = 0; i < s_composedUnresolvedProperties.Count; i++)
            {
                if (s_composedUnresolvedProperties[i].key == prop.key)
                {
                    s_composedUnresolvedProperties[i] = prop;
                    return;
                }
            }
            s_composedUnresolvedProperties.Add(prop);
        }
    }
}
