using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
namespace ImTK.UI
{
    public static partial class RenderEngine
    {
        private static readonly List<StyleProperty> s_composedProps = new List<StyleProperty>();
        private static readonly List<StyleProperty> s_translatedProps = new List<StyleProperty>();

        internal static void ComputeStyleFlat(System.Collections.Generic.List<RenderOp> renderList)
        {
            for (int i = 0; i < renderList.Count; i++)
            {
                var op = renderList[i];
                if (op.Type != RenderOpType.Begin) continue;
                
                var element = op.Element;

                if (element.m_isStyleDirty)
                {
                    s_composedProps.Clear();

                    // 0. Inherited Macro Tokens
                    if (element.parent != null)
                    {
                        var activeProps = element.parent.resolvedStyle.GetActiveProperties();
                        for (int j = 0; j < activeProps.Count; j++)
                        {
                            var prop = activeProps[j];
                            if (prop.category == StyleCategory.HighLevelToken && prop.isInheritable)
                            {
                                SetComposedProperty(prop);
                            }
                        }
                    }

                    // 1. Global Sheet
                    foreach (var className in element.classList)
                    {
                        if (StyleSheet.Global.TryGetBlock(className, out var block))
                        {
                            for (int j = 0; j < block.Properties.Count; j++) SetComposedProperty(block.Properties[j]);
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
                        foreach (var className in element.classList)
                        {
                            if (activeLocalSheet.TryGetBlock(className, out var block))
                            {
                                for (int j = 0; j < block.Properties.Count; j++) SetComposedProperty(block.Properties[j]);
                            }
                        }
                    }

                    // 3. Inline Style
                    if (element.internalStyle is VisualElementStyle inlineStyle)
                    {
                        var unres = inlineStyle.UnresolvedProperties;
                        for (int j = 0; j < unres.Count; j++)
                        {
                            SetComposedProperty(unres[j]);
                        }
                    }

                    uint oldLayoutHash = element.resolvedStyle.GetLayoutHash();
                    ResolvedLayoutState oldLayoutState = element.resolvedLayoutState;
                    float oldDpiScale = element.resolvedStyle.currentDpiScale;

                    element.resolvedStyle.Clear();
                    if (element.parent != null)
                    {
                        element.resolvedStyle.CopyFrom(element.parent.resolvedStyle);
                    }

                    // 若此元素有局部 theme，將 theme 的完整 ImGui 樣式注入 resolvedStyle。
                    // 若沒有局部 theme 且為根節點 (Window)，則注入 GlobalTheme 確保其擁有完整覆寫。
                    // 時機：CopyFrom 繼承父層基底之後、composed properties 之前，
                    // 使 StyleSheet / inline style 仍可覆蓋 theme 值（優先序：inline > stylesheet > theme）。
                    // 實際的 Push/Pop 隔離由既有的 Diff → requiredStyle → RenderNode 機制自動處理。
                    var themeToInject = element.m_theme;
                    if (themeToInject == null && element.parent == null)
                    {
                        themeToInject = ImTKTheme.GlobalTheme;
                    }

                    if (themeToInject != null)
                    {
                        themeToInject.InjectToStyleHandler(element.resolvedStyle);
                    }

                    s_translatedProps.Clear();
                    foreach (var composedProp in s_composedProps)
                    {
                        if (element.internalStyle is VisualElementStyle styleComp)
                        {
                            styleComp.ComputeHighlevelToken(composedProp, s_translatedProps);
                        }
                        else
                        {
                            s_translatedProps.Add(composedProp);
                        }
                    }

                    element.ResolveLayoutState(s_translatedProps);

                    var theme = element.theme ?? ImTKTheme.GlobalTheme;

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

                        element.resolvedStyle.TrySetProperty(finalProp);
                    }

                    bool hasPaddingOverride = false;
                    foreach (var prop in s_translatedProps)
                    {
                        if (prop.key == VisualElement.StyleKey.PaddingLeft.Hash || prop.key == VisualElement.StyleKey.PaddingRight.Hash || 
                            prop.key == VisualElement.StyleKey.PaddingTop.Hash || prop.key == VisualElement.StyleKey.PaddingBottom.Hash) 
                        {
                            hasPaddingOverride = true;
                            break;
                        }
                    }

                    if (hasPaddingOverride)
                    {
                        var pad = element.resolvedLayoutState.padding;
                        var paddingVec = new Vector2((pad.left + pad.right) / 2f, (pad.top + pad.bottom) / 2f);

                        element.resolvedStyle.TrySetProperty(new StyleProperty {
                            category = StyleCategory.ImGuiStyle,
                            dataType = StyleDataType.Vector2,
                            key = (int)ImGuiStyleVar.WindowPadding,
                            vector2Value = paddingVec
                        });
                        
                        element.resolvedStyle.TrySetProperty(new StyleProperty {
                            category = StyleCategory.ImGuiStyle,
                            dataType = StyleDataType.Vector2,
                            key = (int)ImGuiStyleVar.FramePadding,
                            vector2Value = paddingVec
                        });
                    }

                    var dpiScale = RenderEngine.Context.CurrentDpiScale;
                    if (dpiScale != 1.0f)
                    {
                        element.resolvedLayoutState.Scale(dpiScale);
                        element.resolvedStyle.Scale(dpiScale);
                    }

                    ImGuiStyleHandler.Diff(element.parent?.resolvedStyle, element.resolvedStyle, element.requiredStyle);
                    
                    uint newLayoutHash = element.resolvedStyle.GetLayoutHash();
                    float newDpiScale = element.resolvedStyle.currentDpiScale;
                    if (oldLayoutHash != newLayoutHash || oldLayoutState != element.resolvedLayoutState || oldDpiScale != newDpiScale)
                    {
                        element.MarkMeasureDirty();
                        element.MarkArrangeDirty();
                    }

                    element.m_isStyleDirty = false;
                }
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
