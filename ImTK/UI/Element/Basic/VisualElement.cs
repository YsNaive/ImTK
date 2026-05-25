using System;
using System.Collections.Generic;
using ImGuiNET;

using System.Numerics;
using ImTK.Core;

namespace ImTK.UI
{

    public class VisualElement : IVisualElementHierarchy
    {
        public class StyleKey
        {
            public static readonly HashedString BackgroundColor = new HashedString("BackgroundColor");
            public static readonly HashedString TextColor = new HashedString("TextColor");
            public static readonly HashedString DisabledTextColor = new HashedString("DisabledTextColor");
            public static readonly HashedString SelectionColor = new HashedString("SelectionColor");
            public static readonly HashedString BorderColor = new HashedString("BorderColor");

            public static readonly HashedString Padding = new HashedString("Padding");
            public static readonly HashedString ItemSpacing = new HashedString("ItemSpacing");
            public static readonly HashedString ItemInnerSpacing = new HashedString("ItemInnerSpacing");
            public static readonly HashedString BorderWidth = new HashedString("BorderWidth");
            public static readonly HashedString BorderRadius = new HashedString("BorderRadius");
            public static readonly HashedString Alpha = new HashedString("Alpha");
            public static readonly HashedString DisabledAlpha = new HashedString("DisabledAlpha");

            public static readonly HashedString FontFamily = new HashedString("FontFamily");
            public static readonly HashedString FontSize = new HashedString("FontSize");

            // --- Layout Properties ---
            public static readonly HashedString Width = new HashedString("Width");
            public static readonly HashedString Height = new HashedString("Height");
            public static readonly HashedString MinWidth = new HashedString("MinWidth");
            public static readonly HashedString MaxWidth = new HashedString("MaxWidth");
            public static readonly HashedString MinHeight = new HashedString("MinHeight");
            public static readonly HashedString MaxHeight = new HashedString("MaxHeight");

            public static readonly HashedString Margin = new HashedString("Margin");
            public static readonly HashedString FlexDirection = new HashedString("FlexDirection");
            public static readonly HashedString FlexWrap = new HashedString("FlexWrap");
            public static readonly HashedString JustifyContent = new HashedString("JustifyContent");
            public static readonly HashedString AlignItems = new HashedString("AlignItems");
            public static readonly HashedString FlexGrow = new HashedString("FlexGrow");
            public static readonly HashedString AlignSelf = new HashedString("AlignSelf");
            
            public static readonly HashedString PositionType = new HashedString("PositionType");
            public static readonly HashedString Top = new HashedString("Top");
            public static readonly HashedString Bottom = new HashedString("Bottom");
            public static readonly HashedString Left = new HashedString("Left");
            public static readonly HashedString Right = new HashedString("Right");
            public static readonly HashedString Display = new HashedString("Display");
        }

        public class Style : VisualElementStyle, IVisualElementStyle
        {
            public StyleValue<HashedString> fontFamily { set => SetStringToken(StyleKey.FontFamily, value); }
            public StyleValue<int> fontSize { set => SetInt(StyleKey.FontSize, value); }
            public StyleValue<FontSize> fontSizeEnum { set => SetEnum(StyleKey.FontSize, value); }

            public Style() { }

            protected StyleValue<Color>? GetPropertyColor(HashedString key)
            {
                var p = GetProperty(key.Hash);
                return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Color> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Color> { Value = (Color)p.colorValue });
            }

            protected StyleValue<float>? GetPropertyFloat(HashedString key)
            {
                var p = GetProperty(key.Hash);
                return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<float> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<float> { Value = p.floatValue });
            }

            // --- Low-level Override Setters ---

            public void SetColor(HashedString key, StyleValue<Color> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Color };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.colorValue = value.Value.u32;
                SetProperty(prop);
            }

            public void SetInt(HashedString key, StyleValue<int> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Int };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.intValue = value.Value;
                SetProperty(prop);
            }

            public void SetEnum<TEnum>(HashedString key, StyleValue<TEnum> value) where TEnum : struct, System.Enum
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Enum };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.enumValue = System.Convert.ToInt32(value.Value);
                SetProperty(prop);
            }

            public void SetStringToken(HashedString key, StyleValue<HashedString> value)
            {
                if (value.IsNull) { Clear(key); return; }
                int hash = value.IsToken ? value.Token.Hash : value.Value.Hash;
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = StyleDataType.HashedString, tokenHash = hash };
                SetProperty(prop);
            }

            public void SetFloat(HashedString key, StyleValue<float> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Float };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.floatValue = value.Value;
                SetProperty(prop);
            }

            public void SetVector2(HashedString key, StyleValue<Vector2> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Vector2 };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.vector2Value = value.Value;
                SetProperty(prop);
            }

            public void Clear(HashedString key)
            {
                SetProperty(new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = StyleDataType.Null });
            }

            // --- High-level Property Syntax Sugar ---

            public StyleValue<Color>? backgroundColor
            {
                get => GetPropertyColor(StyleKey.BackgroundColor);
                set { if (value.HasValue) SetColor(StyleKey.BackgroundColor, value.Value); else Clear(StyleKey.BackgroundColor); }
            }

            public StyleValue<Color>? textColor
            {
                get => GetPropertyColor(StyleKey.TextColor);
                set { if (value.HasValue) SetColor(StyleKey.TextColor, value.Value); else Clear(StyleKey.TextColor); }
            }

            public StyleValue<Color>? disabledTextColor
            {
                get => GetPropertyColor(StyleKey.DisabledTextColor);
                set { if (value.HasValue) SetColor(StyleKey.DisabledTextColor, value.Value); else Clear(StyleKey.DisabledTextColor); }
            }

            public StyleValue<Color>? borderColor
            {
                get => GetPropertyColor(StyleKey.BorderColor);
                set { if (value.HasValue) SetColor(StyleKey.BorderColor, value.Value); else Clear(StyleKey.BorderColor); }
            }

            public StyleValue<float>? borderWidth
            {
                get => GetPropertyFloat(StyleKey.BorderWidth);
                set { if (value.HasValue) SetFloat(StyleKey.BorderWidth, value.Value); else Clear(StyleKey.BorderWidth); }
            }

            public StyleValue<float>? borderRadius
            {
                get => GetPropertyFloat(StyleKey.BorderRadius);
                set { if (value.HasValue) SetFloat(StyleKey.BorderRadius, value.Value); else Clear(StyleKey.BorderRadius); }
            }

            public StyleValue<float>? alpha
            {
                get => GetPropertyFloat(StyleKey.Alpha);
                set { if (value.HasValue) SetFloat(StyleKey.Alpha, value.Value); else Clear(StyleKey.Alpha); }
            }

            public StyleValue<float>? disabledAlpha
            {
                get => GetPropertyFloat(StyleKey.DisabledAlpha);
                set { if (value.HasValue) SetFloat(StyleKey.DisabledAlpha, value.Value); else Clear(StyleKey.DisabledAlpha); }
            }

            public StyleValue<Vector2>? padding
            {
                get { var p = GetProperty(StyleKey.Padding.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.Padding, value.Value); else Clear(StyleKey.Padding); }
            }

            public StyleValue<Vector2>? itemSpacing
            {
                get { var p = GetProperty(StyleKey.ItemSpacing.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.ItemSpacing, value.Value); else Clear(StyleKey.ItemSpacing); }
            }

            public StyleValue<Vector2>? itemInnerSpacing
            {
                get { var p = GetProperty(StyleKey.ItemInnerSpacing.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.ItemInnerSpacing, value.Value); else Clear(StyleKey.ItemInnerSpacing); }
            }

            public StyleValue<Color>? selectionColor
            {
                get => GetPropertyColor(StyleKey.SelectionColor);
                set { if (value.HasValue) SetColor(StyleKey.SelectionColor, value.Value); else Clear(StyleKey.SelectionColor); }
            }

            // --- High-level Layout Properties ---
            public StyleValue<float>? width
            {
                get => GetPropertyFloat(StyleKey.Width);
                set { if (value.HasValue) SetFloat(StyleKey.Width, value.Value); else Clear(StyleKey.Width); }
            }
            public StyleValue<float>? height
            {
                get => GetPropertyFloat(StyleKey.Height);
                set { if (value.HasValue) SetFloat(StyleKey.Height, value.Value); else Clear(StyleKey.Height); }
            }
            public StyleValue<float>? minWidth
            {
                get => GetPropertyFloat(StyleKey.MinWidth);
                set { if (value.HasValue) SetFloat(StyleKey.MinWidth, value.Value); else Clear(StyleKey.MinWidth); }
            }
            public StyleValue<float>? maxWidth
            {
                get => GetPropertyFloat(StyleKey.MaxWidth);
                set { if (value.HasValue) SetFloat(StyleKey.MaxWidth, value.Value); else Clear(StyleKey.MaxWidth); }
            }
            public StyleValue<float>? minHeight
            {
                get => GetPropertyFloat(StyleKey.MinHeight);
                set { if (value.HasValue) SetFloat(StyleKey.MinHeight, value.Value); else Clear(StyleKey.MinHeight); }
            }
            public StyleValue<float>? maxHeight
            {
                get => GetPropertyFloat(StyleKey.MaxHeight);
                set { if (value.HasValue) SetFloat(StyleKey.MaxHeight, value.Value); else Clear(StyleKey.MaxHeight); }
            }
            public StyleValue<Vector2>? margin
            {
                get { var p = GetProperty(StyleKey.Margin.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.Margin, value.Value); else Clear(StyleKey.Margin); }
            }
            public StyleValue<FlexDirection>? flexDirection
            {
                get { var p = GetProperty(StyleKey.FlexDirection.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<FlexDirection> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<FlexDirection> { Value = (FlexDirection)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.FlexDirection, value.Value); else Clear(StyleKey.FlexDirection); }
            }
            public StyleValue<FlexWrap>? flexWrap
            {
                get { var p = GetProperty(StyleKey.FlexWrap.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<FlexWrap> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<FlexWrap> { Value = (FlexWrap)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.FlexWrap, value.Value); else Clear(StyleKey.FlexWrap); }
            }
            public StyleValue<JustifyContent>? justifyContent
            {
                get { var p = GetProperty(StyleKey.JustifyContent.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<JustifyContent> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<JustifyContent> { Value = (JustifyContent)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.JustifyContent, value.Value); else Clear(StyleKey.JustifyContent); }
            }
            public StyleValue<AlignItems>? alignItems
            {
                get { var p = GetProperty(StyleKey.AlignItems.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<AlignItems> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<AlignItems> { Value = (AlignItems)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.AlignItems, value.Value); else Clear(StyleKey.AlignItems); }
            }
            public StyleValue<float>? flexGrow
            {
                get => GetPropertyFloat(StyleKey.FlexGrow);
                set { if (value.HasValue) SetFloat(StyleKey.FlexGrow, value.Value); else Clear(StyleKey.FlexGrow); }
            }
            public StyleValue<AlignItems>? alignSelf
            {
                get { var p = GetProperty(StyleKey.AlignSelf.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<AlignItems> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<AlignItems> { Value = (AlignItems)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.AlignSelf, value.Value); else Clear(StyleKey.AlignSelf); }
            }
            public StyleValue<PositionType>? positionType
            {
                get { var p = GetProperty(StyleKey.PositionType.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<PositionType> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<PositionType> { Value = (PositionType)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.PositionType, value.Value); else Clear(StyleKey.PositionType); }
            }
            public StyleValue<float>? top
            {
                get => GetPropertyFloat(StyleKey.Top);
                set { if (value.HasValue) SetFloat(StyleKey.Top, value.Value); else Clear(StyleKey.Top); }
            }
            public StyleValue<float>? bottom
            {
                get => GetPropertyFloat(StyleKey.Bottom);
                set { if (value.HasValue) SetFloat(StyleKey.Bottom, value.Value); else Clear(StyleKey.Bottom); }
            }
            public StyleValue<float>? left
            {
                get => GetPropertyFloat(StyleKey.Left);
                set { if (value.HasValue) SetFloat(StyleKey.Left, value.Value); else Clear(StyleKey.Left); }
            }
            public StyleValue<float>? right
            {
                get => GetPropertyFloat(StyleKey.Right);
                set { if (value.HasValue) SetFloat(StyleKey.Right, value.Value); else Clear(StyleKey.Right); }
            }
            public StyleValue<DisplayStyle>? display
            {
                get { var p = GetProperty(StyleKey.Display.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<DisplayStyle> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<DisplayStyle> { Value = (DisplayStyle)p.enumValue }); }
                set { if (value.HasValue) SetEnum(StyleKey.Display, value.Value); else Clear(StyleKey.Display); }
            }
        }

        private static int s_elementCounter = 0;
        internal readonly int m_elementId;

        public VisualElementHierarchy hierarchy { get; }
        public virtual VisualElement contentContainer => this;

        public VisualElement parent { get; internal set; }

        public PickingMode pickingMode { get; set; } = PickingMode.Position;
        internal bool m_wasHovered = false;
        internal bool m_useAutoId = true;

        private Dictionary<Type, Delegate> m_callbacks;

        public IVisualElementStyle internalStyle { get; set; }

        public Style style => internalStyle as Style;

        public StyleClass classList { get; private set; }

        private StyleSheet m_localStyleSheet;
        public StyleSheet localStyleSheet
        {
            get => m_localStyleSheet;
            set
            {
                if (m_localStyleSheet != value)
                {
                    m_localStyleSheet = value;
                    MarkStyleDirty();
                }
            }
        }

        internal bool m_isStyleDirty = true;

        internal bool m_isMeasureDirty = true;
        internal bool m_isArrangeDirty = true;
        internal Vector2 m_desiredSize;
        public Rect layoutRect { get; internal set; }
        internal LayoutConstraint m_lastConstraint;

        internal void MarkMeasureDirty()
        {
            if (m_isMeasureDirty) return;
            m_isMeasureDirty = true;
            if (this is IWindow) return;
            parent?.MarkMeasureDirty();
        }

        internal void MarkArrangeDirty()
        {
            if (m_isArrangeDirty) return;
            m_isArrangeDirty = true;
            if (this is IWindow) return;
            parent?.MarkArrangeDirty();
        }

        public void Measure(LayoutConstraint constraint)
        {
            if (!m_isMeasureDirty && m_lastConstraint == constraint)
                return;

            m_lastConstraint = constraint;
            resolvedStyle.PushFontOnly();
            Vector2 desired = MeasureContent(constraint);
            
            var state = resolvedLayoutState;
            if (state.width.HasValue) desired.X = state.width.Value;
            if (state.height.HasValue) desired.Y = state.height.Value;
            
            if (state.minWidth.HasValue) desired.X = Math.Max(desired.X, state.minWidth.Value);
            if (state.maxWidth.HasValue) desired.X = Math.Min(desired.X, state.maxWidth.Value);
            if (state.minHeight.HasValue) desired.Y = Math.Max(desired.Y, state.minHeight.Value);
            if (state.maxHeight.HasValue) desired.Y = Math.Min(desired.Y, state.maxHeight.Value);

            m_desiredSize = desired;
            resolvedStyle.PopFontOnly();

            m_isMeasureDirty = false;
        }

        protected virtual Vector2 MeasureContent(LayoutConstraint constraint)
        {
            var state = resolvedLayoutState;
            bool isRow = state.flexDirection == FlexDirection.Row;

            float totalMain = 0;
            float maxCross = 0;
            
            System.Numerics.Vector2 itemSpacing = this.theme.itemSpacing;
            if (resolvedStyle.TryGetVector2((int)ImGuiStyleVar.ItemSpacing, out var overrideSpacing))
                itemSpacing = overrideSpacing;
            float gap = isRow ? itemSpacing.X : itemSpacing.Y;
            int renderableChildCount = 0;

            int childCount = hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = hierarchy.ChildAt(i);
                var childState = child.resolvedLayoutState;

                if (childState.display == DisplayStyle.None || childState.positionType == PositionType.Absolute)
                    continue;

                LayoutConstraint childConstraint = new LayoutConstraint(0, 0, MeasureMode.Undefined, MeasureMode.Undefined);
                child.Measure(childConstraint);
                
                Vector2 childOuterSize = child.m_desiredSize + new Vector2(
                    Math.Max(0, childState.margin.X), Math.Max(0, childState.margin.Y)
                );
                
                if (isRow)
                {
                    totalMain += childOuterSize.X;
                    maxCross = Math.Max(maxCross, childOuterSize.Y);
                }
                else
                {
                    totalMain += childOuterSize.Y;
                    maxCross = Math.Max(maxCross, childOuterSize.X);
                }
                renderableChildCount++;
            }

            if (renderableChildCount > 1)
                totalMain += gap * (renderableChildCount - 1);

            Vector2 desired = isRow ? new Vector2(totalMain, maxCross) : new Vector2(maxCross, totalMain);
            return desired;
        }

        public void Arrange(Rect finalAbsoluteRect)
        {
            if (!m_isArrangeDirty && this.layoutRect == finalAbsoluteRect && !m_isMeasureDirty)
                return;

            this.layoutRect = finalAbsoluteRect;
            ArrangeContent(finalAbsoluteRect);
            m_isArrangeDirty = false;
        }

        public System.Numerics.Vector2 LocalToWorld(System.Numerics.Vector2 localPoint)
        {
            return layoutRect.position + localPoint;
        }

        public System.Numerics.Vector2 WorldToLocal(System.Numerics.Vector2 worldPoint)
        {
            return worldPoint - layoutRect.position;
        }

        public Rect LocalToWorld(Rect localRect)
        {
            return new Rect(layoutRect.position + localRect.position, localRect.size);
        }

        public Rect WorldToLocal(Rect worldRect)
        {
            return new Rect(worldRect.position - layoutRect.position, worldRect.size);
        }

        protected virtual void ArrangeContent(Rect finalAbsoluteRect)
        {
            var state = resolvedLayoutState;
            bool isRow = state.flexDirection == FlexDirection.Row;
            
            var relativeChildren = new System.Collections.Generic.List<VisualElement>();
            var absoluteChildren = new System.Collections.Generic.List<VisualElement>();
            
            int childCount = hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = hierarchy.ChildAt(i);
                if (child.resolvedLayoutState.display == DisplayStyle.None) continue;
                
                if (child.resolvedLayoutState.positionType == PositionType.Absolute)
                    absoluteChildren.Add(child);
                else
                    relativeChildren.Add(child);
            }
            
            // Layout Relative Children (Simplified 1-Pass Flexbox)
            if (relativeChildren.Count > 0)
            {
                float availableMain = isRow ? finalAbsoluteRect.width : finalAbsoluteRect.height;
                float availableCross = isRow ? finalAbsoluteRect.height : finalAbsoluteRect.width;
                
                float totalMainDesired = 0;
                float totalFlexGrow = 0;
                
                foreach (var child in relativeChildren)
                {
                    var childState = child.resolvedLayoutState;
                    Vector2 margin = childState.margin;
                    float childMain = isRow ? (child.m_desiredSize.X + margin.X) : (child.m_desiredSize.Y + margin.Y);
                    totalMainDesired += childMain;
                    totalFlexGrow += childState.flexGrow;
                }
                
                System.Numerics.Vector2 itemSpacing = this.theme.itemSpacing;
                if (resolvedStyle.TryGetVector2((int)ImGuiStyleVar.ItemSpacing, out var overrideSpacing))
                    itemSpacing = overrideSpacing;
                float gap = isRow ? itemSpacing.X : itemSpacing.Y;
                float spacing = gap;
                
                float totalGapSpace = relativeChildren.Count > 1 ? gap * (relativeChildren.Count - 1) : 0;
                float freeMainSpace = availableMain - totalMainDesired - totalGapSpace;
                float currentMainPos = 0;
                
                if (freeMainSpace > 0 && totalFlexGrow == 0)
                {
                    switch (state.justifyContent)
                    {
                        case JustifyContent.Center:
                            currentMainPos = freeMainSpace / 2f;
                            break;
                        case JustifyContent.FlexEnd:
                            currentMainPos = freeMainSpace;
                            break;
                        case JustifyContent.SpaceBetween:
                            if (relativeChildren.Count > 1)
                                spacing = (availableMain - totalMainDesired) / (relativeChildren.Count - 1);
                            break;
                    }
                }
                
                foreach (var child in relativeChildren)
                {
                    var childState = child.resolvedLayoutState;
                    Vector2 margin = childState.margin;
                    
                    float childDesiredMain = isRow ? child.m_desiredSize.X : child.m_desiredSize.Y;
                    float childDesiredCross = isRow ? child.m_desiredSize.Y : child.m_desiredSize.X;
                    
                    float childMarginMain = isRow ? margin.X : margin.Y;
                    float childMarginCross = isRow ? margin.Y : margin.X;
                    
                    float childActualMain = childDesiredMain;
                    if (freeMainSpace > 0 && totalFlexGrow > 0 && childState.flexGrow > 0)
                    {
                        float growAmt = (childState.flexGrow / totalFlexGrow) * freeMainSpace;
                        childActualMain += growAmt;
                    }
                    
                    float childActualCross = childDesiredCross;
                    float currentCrossPos = 0;
                    AlignItems align = childState.alignSelf;
                    
                    float freeCrossSpace = availableCross - (childActualCross + childMarginCross);
                    
                    if (align == AlignItems.Stretch && ((isRow && !childState.height.HasValue) || (!isRow && !childState.width.HasValue)))
                    {
                        childActualCross = availableCross - childMarginCross;
                    }
                    else if (align == AlignItems.Center)
                    {
                        currentCrossPos = freeCrossSpace / 2f;
                    }
                    else if (align == AlignItems.FlexEnd)
                    {
                        currentCrossPos = freeCrossSpace;
                    }
                    
                    if (isRow)
                    {
                        if (childState.minWidth.HasValue) childActualMain = Math.Max(childActualMain, childState.minWidth.Value);
                        if (childState.maxWidth.HasValue) childActualMain = Math.Min(childActualMain, childState.maxWidth.Value);
                        if (childState.minHeight.HasValue) childActualCross = Math.Max(childActualCross, childState.minHeight.Value);
                        if (childState.maxHeight.HasValue) childActualCross = Math.Min(childActualCross, childState.maxHeight.Value);
                    }
                    else
                    {
                        if (childState.minHeight.HasValue) childActualMain = Math.Max(childActualMain, childState.minHeight.Value);
                        if (childState.maxHeight.HasValue) childActualMain = Math.Min(childActualMain, childState.maxHeight.Value);
                        if (childState.minWidth.HasValue) childActualCross = Math.Max(childActualCross, childState.minWidth.Value);
                        if (childState.maxWidth.HasValue) childActualCross = Math.Min(childActualCross, childState.maxWidth.Value);
                    }
                    
                    float x, y, w, h;
                    if (isRow)
                    {
                        x = finalAbsoluteRect.x + currentMainPos + margin.X / 2f;
                        y = finalAbsoluteRect.y + currentCrossPos + margin.Y / 2f;
                        w = childActualMain;
                        h = childActualCross;
                    }
                    else
                    {
                        x = finalAbsoluteRect.x + currentCrossPos + margin.X / 2f;
                        y = finalAbsoluteRect.y + currentMainPos + margin.Y / 2f;
                        w = childActualCross;
                        h = childActualMain;
                    }
                    
                    currentMainPos += childActualMain + childMarginMain + spacing;
                    child.Arrange(new Rect(x, y, w, h));
                }
            }
            
            // Layout Absolute Children
            foreach (var absChild in absoluteChildren)
            {
                var absState = absChild.resolvedLayoutState;
                float x = finalAbsoluteRect.x + (absState.left ?? 0);
                float y = finalAbsoluteRect.y + (absState.top ?? 0);
                float w = absState.width ?? absChild.m_desiredSize.X;
                float h = absState.height ?? absChild.m_desiredSize.Y;
                
                if (absState.right.HasValue && !absState.left.HasValue)
                    x = finalAbsoluteRect.x + finalAbsoluteRect.width - absState.right.Value - w;
                    
                if (absState.bottom.HasValue && !absState.top.HasValue)
                    y = finalAbsoluteRect.y + finalAbsoluteRect.height - absState.bottom.Value - h;
                    
                absChild.Arrange(new Rect(x, y, w, h));
            }
        }

        public ImGuiStyleHandler requiredStyle { get; } = new ImGuiStyleHandler();
        public ImGuiStyleHandler resolvedStyle { get; } = new ImGuiStyleHandler();
        public ResolvedLayoutState resolvedLayoutState { get; internal set; } = ResolvedLayoutState.Default;

        internal void ResolveLayoutState(List<StyleProperty> composedProps)
        {
            var state = ResolvedLayoutState.Default;
            
            // Layout properties are not inheritable by default, so we don't copy from parent
            // except for AlignSelf which defers to parent's AlignItems if not set
            if (parent != null)
            {
                state.alignSelf = parent.resolvedLayoutState.alignItems;
            }

            foreach (var prop in composedProps)
            {
                if (prop.category == StyleCategory.Layout)
                {
                    if (prop.key == StyleKey.Width.Hash) state.width = prop.floatValue;
                    else if (prop.key == StyleKey.Height.Hash) state.height = prop.floatValue;
                    else if (prop.key == StyleKey.MinWidth.Hash) state.minWidth = prop.floatValue;
                    else if (prop.key == StyleKey.MaxWidth.Hash) state.maxWidth = prop.floatValue;
                    else if (prop.key == StyleKey.MinHeight.Hash) state.minHeight = prop.floatValue;
                    else if (prop.key == StyleKey.MaxHeight.Hash) state.maxHeight = prop.floatValue;
                    else if (prop.key == StyleKey.Margin.Hash) state.margin = prop.vector2Value;
                    else if (prop.key == StyleKey.FlexDirection.Hash) state.flexDirection = (FlexDirection)prop.enumValue;
                    else if (prop.key == StyleKey.FlexWrap.Hash) state.flexWrap = (FlexWrap)prop.enumValue;
                    else if (prop.key == StyleKey.JustifyContent.Hash) state.justifyContent = (JustifyContent)prop.enumValue;
                    else if (prop.key == StyleKey.AlignItems.Hash) state.alignItems = (AlignItems)prop.enumValue;
                    else if (prop.key == StyleKey.FlexGrow.Hash) state.flexGrow = prop.floatValue;
                    else if (prop.key == StyleKey.AlignSelf.Hash) state.alignSelf = (AlignItems)prop.enumValue;
                    else if (prop.key == StyleKey.PositionType.Hash) state.positionType = (PositionType)prop.enumValue;
                    else if (prop.key == StyleKey.Top.Hash) state.top = prop.floatValue;
                    else if (prop.key == StyleKey.Bottom.Hash) state.bottom = prop.floatValue;
                    else if (prop.key == StyleKey.Left.Hash) state.left = prop.floatValue;
                    else if (prop.key == StyleKey.Right.Hash) state.right = prop.floatValue;
                    else if (prop.key == StyleKey.Display.Hash) state.display = (DisplayStyle)prop.enumValue;
                }
            }

            resolvedLayoutState = state;
        }


        public VisualElement()
        {
            m_elementId = ++s_elementCounter;
            internalStyle = new Style();
            hierarchy = new VisualElementHierarchy(this);

            classList = new StyleClass();
            classList.OnClassChanged = MarkStyleDirty;
        }

        public void MarkStyleDirty()
        {
            m_isStyleDirty = true;
            int count = hierarchy.childCount;
            for (int i = 0; i < count; i++)
            {
                hierarchy.ChildAt(i).MarkStyleDirty();
            }
        }

        private ImTKTheme m_theme;

        public ImTKTheme theme
        {
            get
            {
                if (m_theme != null) return m_theme;
                if (parent != null) return parent.theme;
                return ImTKTheme.GlobalTheme;
            }
            set
            {
                if (m_theme != value)
                {
                    m_theme = value;
                    MarkStyleDirty();
                }
            }
        }

        public NodeType GetNodeType()
        {
            bool hasLogicalParent = this.parent != null;
            bool hasPhysicalParent = this.hierarchy.parent != null;

            if (!hasLogicalParent && !hasPhysicalParent) return NodeType.None;
            if (hasLogicalParent && hasPhysicalParent) return NodeType.LogicNode;
            if (!hasLogicalParent && hasPhysicalParent) return NodeType.PhysicsNode;
            return NodeType.Invalid;
        }

        public int childCount => contentContainer == this ? hierarchy.childCount : contentContainer.childCount;

        public VisualElement ChildAt(int index) => contentContainer == this ? hierarchy.ChildAt(index) : contentContainer.ChildAt(index);

        public void Add(VisualElement child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (!hierarchy.CheckSafeState()) return;

            NodeType type = child.GetNodeType();
            if (type == NodeType.LogicNode)
            {
                child.parent.Remove(child);
            }
            else if (type == NodeType.PhysicsNode)
            {
                child.hierarchy.parent.hierarchy.Remove(child);
            }

            child.parent = this;

            if (contentContainer == this)
            {
                hierarchy.Add(child);
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Add(child);
            }

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public void AddRange(IEnumerable<VisualElement> children)
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            foreach (var child in children)
            {
                Add(child);
            }
        }

        public void Remove(VisualElement child)
        {
            if (child == null) return;
            if (!hierarchy.CheckSafeState()) return;

            if (contentContainer == this)
            {
                hierarchy.Remove(child);
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Remove(child);
            }

            if (child.parent == this)
            {
                child.parent = null;
            }

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public void Clear()
        {
            if (!hierarchy.CheckSafeState()) return;

            var childrenToClear = new List<VisualElement>(contentContainer == this ? hierarchy.Children() : contentContainer.Children());
            foreach(var child in childrenToClear)
            {
                if (child.parent == this)
                {
                    child.parent = null;
                }
            }

            if (contentContainer == this)
            {
                hierarchy.Clear();
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Clear();
            }

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public IEnumerable<VisualElement> Children()
        {
            return contentContainer == this ? hierarchy.Children() : contentContainer.Children();
        }

        public void RegisterCallback<TEventType>(Action<TEventType> callback) where TEventType : UIEventBase
        {
            if (m_callbacks == null) m_callbacks = new Dictionary<Type, Delegate>();

            Type type = typeof(TEventType);
            if (m_callbacks.TryGetValue(type, out var existing))
            {
                m_callbacks[type] = Delegate.Combine(existing, callback);
            }
            else
            {
                m_callbacks[type] = callback;
            }
        }

        public void UnregisterCallback<TEventType>(Action<TEventType> callback) where TEventType : UIEventBase
        {
            if (m_callbacks == null) return;

            Type type = typeof(TEventType);
            if (m_callbacks.TryGetValue(type, out var existing))
            {
                var newDelegate = Delegate.Remove(existing, callback);
                if (newDelegate == null)
                {
                    m_callbacks.Remove(type);
                }
                else
                {
                    m_callbacks[type] = newDelegate;
                }
            }
        }

        protected void SendEvent(UIEventBase evt)
        {
            evt.source = this;
            EventDispatcher.Enqueue(evt);
        }

        public virtual bool OnBeginRender()
        {
            if (this.parent != null)
            {
                ImGui.SetCursorScreenPos(this.layoutRect.position);
            }
            return true;
        }

        public virtual void Update()
        {

        }

        public Window GetWindow()
        {
            VisualElement current = this;
            while (current != null)
            {
                if (current is Window w) return w;
                current = current.parent;
            }
            return null;
        }

        public virtual void OnRender()
        {
        }

        public virtual void OnEndRender()
        {
        }

        protected internal virtual bool CheckHoverState()
        {
            return false;
        }

        public virtual void HandleEvent(UIEventBase evt)
        {
            if (m_callbacks != null && m_callbacks.TryGetValue(evt.GetType(), out var callback))
            {
                callback.DynamicInvoke(evt);
            }
        }

        internal bool HasAnyCallback()
        {
            return m_callbacks != null && m_callbacks.Count > 0;
        }
    }

    public class VisualElement<TStyle> : VisualElement where TStyle : IVisualElementStyle, new()
    {
        public new TStyle style => (TStyle)internalStyle;

        public VisualElement() : base()
        {
            internalStyle = new TStyle();
        }
    }

    public enum PickingMode
    {
        Position,
        Ignore
    }

    public abstract class ElementFlags<TEnum> where TEnum : struct, Enum
    {
        public TEnum Value { get; set; }

        public ElementFlags()
        {
            Value = default;
        }

        public ElementFlags(TEnum initialValue)
        {
            Value = initialValue;
        }

        protected void SetFlag(TEnum flag, bool state)
        {
            int mask = Convert.ToInt32(flag);
            int current = Convert.ToInt32(Value);
            if (state)
            {
                current |= mask;
            }
            else
            {
                current &= ~mask;
            }
            Value = (TEnum)Enum.ToObject(typeof(TEnum), current);
        }

        protected bool GetFlag(TEnum flag)
        {
            int mask = Convert.ToInt32(flag);
            int current = Convert.ToInt32(Value);
            return (current & mask) == mask;
        }
    }

    public enum NodeType
    {
        None,
        LogicNode,
        PhysicsNode,
        Invalid
    }
}
