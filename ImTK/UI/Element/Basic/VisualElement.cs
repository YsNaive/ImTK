using System;
using System.Collections.Generic;
using Hexa.NET.ImGui;

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

            public static readonly HashedString BorderWidth = new HashedString("BorderWidth");
            public static readonly HashedString BorderRadius = new HashedString("BorderRadius");
            public static readonly HashedString Alpha = new HashedString("Alpha");
            public static readonly HashedString DisabledAlpha = new HashedString("DisabledAlpha");

            public static readonly HashedString PaddingLeft = new HashedString("PaddingLeft");
            public static readonly HashedString PaddingTop = new HashedString("PaddingTop");
            public static readonly HashedString PaddingRight = new HashedString("PaddingRight");
            public static readonly HashedString PaddingBottom = new HashedString("PaddingBottom");

            public static readonly HashedString ItemSpacing = new HashedString("ItemSpacing");
            public static readonly HashedString ItemInnerSpacing = new HashedString("ItemInnerSpacing");

            public static readonly HashedString FontFamily = new HashedString("FontFamily");
            public static readonly HashedString FontSize = new HashedString("FontSize");

            // --- Layout Properties ---
            public static readonly HashedString Width = new HashedString("Width");
            public static readonly HashedString Height = new HashedString("Height");
            public static readonly HashedString MinWidth = new HashedString("MinWidth");
            public static readonly HashedString MaxWidth = new HashedString("MaxWidth");
            public static readonly HashedString MinHeight = new HashedString("MinHeight");
            public static readonly HashedString MaxHeight = new HashedString("MaxHeight");

            public static readonly HashedString MarginLeft = new HashedString("MarginLeft");
            public static readonly HashedString MarginTop = new HashedString("MarginTop");
            public static readonly HashedString MarginRight = new HashedString("MarginRight");
            public static readonly HashedString MarginBottom = new HashedString("MarginBottom");
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
            public static readonly HashedString ColorFamily = new HashedString("ColorFamily");
        }

        public class Style : VisualElementStyle, IVisualElementStyle
        {
            protected VisualElement m_owner;

            public StyleValue<HashedString> fontFamily { set => SetStringToken(StyleKey.FontFamily, value); }
            public StyleFontSize? fontSize
            {
                get
                {
                    var p = GetProperty(StyleKey.FontSize.Hash);
                    if (p.dataType == StyleDataType.Null) return null;
                    if (p.dataType == StyleDataType.HashedString) return new StyleFontSize { Token = new HashedString("TOKEN_" + p.tokenHash) };
                    if (p.dataType == StyleDataType.Enum) return new StyleFontSize { EnumValue = (FontSize)p.enumValue, IsEnum = true };
                    return new StyleFontSize { IntValue = p.intValue, IsEnum = false };
                }
                set
                {
                    if (value.HasValue)
                    {
                        if (value.Value.IsToken) SetStringToken(StyleKey.FontSize, new StyleValue<HashedString> { Token = value.Value.Token });
                        else if (value.Value.IsEnum) SetEnum(StyleKey.FontSize, new StyleValue<FontSize> { Value = value.Value.EnumValue });
                        else SetInt(StyleKey.FontSize, new StyleValue<int> { Value = value.Value.IntValue });
                    }
                    else
                    {
                        Clear(StyleKey.FontSize);
                    }
                }
            }

            public Style() { }
            public void Init(VisualElement owner) { m_owner = owner; }

            protected StyleColor? GetPropertyColor(HashedString key)
            {
                var p = GetProperty(key.Hash);
                if (p.dataType == StyleDataType.Null) return null;
                if (p.dataType == StyleDataType.HashedString) return new StyleColor { Token = new HashedString("TOKEN_" + p.tokenHash) };
                return new StyleColor { Value = (Color)p.colorValue };
            }

            protected void SetPropertyColor(HashedString key, StyleColor? value)
            {
                if (value.HasValue)
                {
                    if (value.Value.IsToken) SetStringToken(key, new StyleValue<HashedString> { Token = value.Value.Token });
                    else SetColor(key, new StyleValue<Color> { Value = value.Value.Value });
                }
                else Clear(key);
            }

            protected StyleSpacing? GetPropertySpacing(HashedString key)
            {
                var p = GetProperty(key.Hash);
                if (p.dataType == StyleDataType.Null) return null;
                if (p.dataType == StyleDataType.HashedString) return new StyleSpacing { Token = new HashedString("TOKEN_" + p.tokenHash) };
                return new StyleSpacing { Value = p.vector2Value };
            }

            protected void SetPropertySpacing(HashedString key, StyleSpacing? value)
            {
                if (value.HasValue)
                {
                    if (value.Value.IsToken) SetStringToken(key, new StyleValue<HashedString> { Token = value.Value.Token });
                    else SetVector2(key, new StyleValue<Vector2> { Value = value.Value.Value });
                }
                else Clear(key);
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
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }

            public void SetInt(HashedString key, StyleValue<int> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Int };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.intValue = value.Value;
                SetProperty(prop);
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }

            public void SetEnum<TEnum>(HashedString key, StyleValue<TEnum> value) where TEnum : struct, System.Enum
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Enum };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.enumValue = System.Convert.ToInt32(value.Value);
                SetProperty(prop);
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }

            public void SetStringToken(HashedString key, StyleValue<HashedString> value)
            {
                if (value.IsNull) { Clear(key); return; }
                int hash = value.IsToken ? value.Token.Hash : value.Value.Hash;
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = StyleDataType.HashedString, tokenHash = hash };
                SetProperty(prop);
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }

            public void SetFloat(HashedString key, StyleValue<float> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Float };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.floatValue = value.Value;
                SetProperty(prop);
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }

            public void SetVector2(HashedString key, StyleValue<Vector2> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Vector2 };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.vector2Value = value.Value;
                SetProperty(prop);
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }



            public void Clear(HashedString key)
            {
                SetProperty(new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = StyleDataType.Null });
                m_owner?.MarkStyleDirty();
                m_owner?.MarkMeasureDirty();
                m_owner?.MarkArrangeDirty();
            }

            // --- High-level Property Syntax Sugar ---

            public StyleValue<ThemeColorFamily>? colorFamily
            {
                get
                {
                    var p = GetProperty(StyleKey.ColorFamily.Hash);
                    return p.dataType == StyleDataType.Null ? null : new StyleValue<ThemeColorFamily> { Value = (ThemeColorFamily)p.enumValue };
                }
                set
                {
                    if (value.HasValue) SetEnum(StyleKey.ColorFamily, value.Value);
                    else Clear(StyleKey.ColorFamily);
                }
            }

            public StyleColor? backgroundColor
            {
                get => GetPropertyColor(StyleKey.BackgroundColor);
                set => SetPropertyColor(StyleKey.BackgroundColor, value);
            }

            public StyleColor? textColor
            {
                get => GetPropertyColor(StyleKey.TextColor);
                set => SetPropertyColor(StyleKey.TextColor, value);
            }

            public StyleColor? disabledTextColor
            {
                get => GetPropertyColor(StyleKey.DisabledTextColor);
                set => SetPropertyColor(StyleKey.DisabledTextColor, value);
            }

            public StyleColor? borderColor
            {
                get => GetPropertyColor(StyleKey.BorderColor);
                set => SetPropertyColor(StyleKey.BorderColor, value);
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

            public StyleThickness? padding
            {
                get
                {
                    var l = GetProperty(StyleKey.PaddingLeft.Hash);
                    var t = GetProperty(StyleKey.PaddingTop.Hash);
                    var r = GetProperty(StyleKey.PaddingRight.Hash);
                    var b = GetProperty(StyleKey.PaddingBottom.Hash);
                    if (l.isNull && t.isNull && r.isNull && b.isNull) return null;
                    if (l.isToken) return new StyleThickness { Token = new HashedString("TOKEN_" + l.tokenHash) };
                    return new StyleThickness { Value = new Thickness(l.floatValue, t.floatValue, r.floatValue, b.floatValue) };
                }
                set
                {
                    if (value.HasValue)
                    {
                        if (value.Value.IsToken)
                        {
                            var tokenVal = new StyleValue<float> { Token = value.Value.Token };
                            SetFloat(StyleKey.PaddingLeft, tokenVal);
                            SetFloat(StyleKey.PaddingTop, tokenVal);
                            SetFloat(StyleKey.PaddingRight, tokenVal);
                            SetFloat(StyleKey.PaddingBottom, tokenVal);
                        }
                        else
                        {
                            var v = value.Value.Value;
                            SetFloat(StyleKey.PaddingLeft, v.left);
                            SetFloat(StyleKey.PaddingTop, v.top);
                            SetFloat(StyleKey.PaddingRight, v.right);
                            SetFloat(StyleKey.PaddingBottom, v.bottom);
                        }
                    }
                    else
                    {
                        Clear(StyleKey.PaddingLeft);
                        Clear(StyleKey.PaddingTop);
                        Clear(StyleKey.PaddingRight);
                        Clear(StyleKey.PaddingBottom);
                    }
                }
            }

            public StyleSpacing? itemSpacing
            {
                get => GetPropertySpacing(StyleKey.ItemSpacing);
                set => SetPropertySpacing(StyleKey.ItemSpacing, value);
            }

            public StyleSpacing? itemInnerSpacing
            {
                get => GetPropertySpacing(StyleKey.ItemInnerSpacing);
                set => SetPropertySpacing(StyleKey.ItemInnerSpacing, value);
            }

            public StyleColor? selectionColor
            {
                get => GetPropertyColor(StyleKey.SelectionColor);
                set => SetPropertyColor(StyleKey.SelectionColor, value);
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
            public StyleThickness? margin
            {
                get
                {
                    var l = GetProperty(StyleKey.MarginLeft.Hash);
                    var t = GetProperty(StyleKey.MarginTop.Hash);
                    var r = GetProperty(StyleKey.MarginRight.Hash);
                    var b = GetProperty(StyleKey.MarginBottom.Hash);
                    if (l.isNull && t.isNull && r.isNull && b.isNull) return null;
                    if (l.isToken) return new StyleThickness { Token = new HashedString("TOKEN_" + l.tokenHash) };
                    return new StyleThickness { Value = new Thickness(l.floatValue, t.floatValue, r.floatValue, b.floatValue) };
                }
                set
                {
                    if (value.HasValue)
                    {
                        if (value.Value.IsToken)
                        {
                            var tokenVal = new StyleValue<float> { Token = value.Value.Token };
                            SetFloat(StyleKey.MarginLeft, tokenVal);
                            SetFloat(StyleKey.MarginTop, tokenVal);
                            SetFloat(StyleKey.MarginRight, tokenVal);
                            SetFloat(StyleKey.MarginBottom, tokenVal);
                        }
                        else
                        {
                            var v = value.Value.Value;
                            SetFloat(StyleKey.MarginLeft, v.left);
                            SetFloat(StyleKey.MarginTop, v.top);
                            SetFloat(StyleKey.MarginRight, v.right);
                            SetFloat(StyleKey.MarginBottom, v.bottom);
                        }
                    }
                    else
                    {
                        Clear(StyleKey.MarginLeft);
                        Clear(StyleKey.MarginTop);
                        Clear(StyleKey.MarginRight);
                        Clear(StyleKey.MarginBottom);
                    }
                }
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
        public VisualElement focusRoot { get; internal set; }

        protected bool m_useNativeLayout = false;
        public bool useNativeLayout
        {
            get => m_useNativeLayout;
            set => m_useNativeLayout = value;
        }

        public PickingMode pickingMode { get; set; } = PickingMode.Position;
        internal bool m_wasHovered = false;
        internal bool m_useAutoId = true;
        
        public virtual string persistenceKey { get; set; } = null;
        internal bool m_hasLoadedState = false;

        protected internal virtual void OnWriteState(Persistence.StateWriter writer) 
        {
            Persistence.PersistentTypeCache.WriteState(this, writer);
        }
        
        protected internal virtual void OnReadState(Persistence.StateReader reader) 
        {
            Persistence.PersistentTypeCache.ReadState(this, reader);
        }
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
        /// <summary>
        /// 排版引擎 (Layout Engine) 計算後賦予此元件的最終絕對座標與尺寸。
        /// 在渲染階段 (Render) 會依據此矩形進行剪裁與繪製。
        /// </summary>
        public Rect layoutRect { get; internal set; }
        internal LayoutConstraint m_lastConstraint;

        /// <summary>
        /// 標記此元件的尺寸測量 (Measure) 已失效，並向上通知父節點需要重新排版。
        /// </summary>
        internal void MarkMeasureDirty()
        {
            if (m_isMeasureDirty) return;
            m_isMeasureDirty = true;
            if (this is ILayoutRoot) return;
            hierarchy.parent?.MarkMeasureDirty();
        }

        /// <summary>
        /// 標記此元件的絕對位置佈局 (Arrange) 已失效，並向上通知父節點需要重新排版。
        /// </summary>
        internal void MarkArrangeDirty()
        {
            if (m_isArrangeDirty) return;
            m_isArrangeDirty = true;
            if (this is ILayoutRoot) return;
            hierarchy.parent?.MarkArrangeDirty();
        }

        /// <summary>
        /// 執行排版引擎的第一階段：測量 (Measure Pass)。
        /// 根據父節點傳入的約束 (constraint) 計算出此元件所需的理想尺寸 (desiredSize)。
        /// </summary>
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

        /// <summary>
        /// 供子類別覆寫的實際測量邏輯。
        /// 預設實作為遍歷子節點並依照 Flexbox 規則計算內容尺寸。
        /// </summary>
        protected virtual Vector2 MeasureContent(LayoutConstraint constraint)
        {
            if (m_useNativeLayout)
            {
                var stateNative = resolvedLayoutState;
                return new Vector2(stateNative.width ?? 0, stateNative.height ?? 0);
            }

            var state = resolvedLayoutState;
            bool isRow = state.flexDirection == FlexDirection.Row;
            bool isWrap = state.flexWrap == FlexWrap.Wrap;

            System.Numerics.Vector2 itemSpacing = this.theme.itemSpacing;
            if (resolvedStyle.TryGetVector2((int)ImGuiStyleVar.ItemSpacing, out var overrideSpacing))
                itemSpacing = overrideSpacing;
            else
                itemSpacing *= Hexa.NET.ImGui.ImGui.GetWindowViewport().DpiScale;
            float gapMain = isRow ? itemSpacing.X : itemSpacing.Y;
            float gapCross = isRow ? itemSpacing.Y : itemSpacing.X;

            float availableMain = isRow ? constraint.AvailableWidth : constraint.AvailableHeight;
            float availableCross = isRow ? constraint.AvailableHeight : constraint.AvailableWidth;
            MeasureMode mainMode = isRow ? constraint.WidthMode : constraint.HeightMode;
            MeasureMode crossMode = isRow ? constraint.HeightMode : constraint.WidthMode;

            float borderX = 0;
            if (resolvedStyle.TryGetFloat((int)ImGuiStyleVar.WindowBorderSize, out float bw)) borderX = bw;
            float borderY = borderX;
            
            float paddingX = state.padding.horizontal + borderX * 2;
            float paddingY = state.padding.vertical + borderY * 2;
            float paddingMain = isRow ? paddingX : paddingY;
            float paddingCross = isRow ? paddingY : paddingX;

            if (mainMode != MeasureMode.Undefined)
                availableMain = Math.Max(0, availableMain - paddingMain);
            if (crossMode != MeasureMode.Undefined)
                availableCross = Math.Max(0, availableCross - paddingCross);

            float totalMain = 0;
            float maxCross = 0;
            
            float currentLineMain = 0;
            float currentLineMaxCross = 0;
            int itemsInCurrentLine = 0;
            
            int childCount = hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = hierarchy.ChildAt(i);
                var childState = child.resolvedLayoutState;

                if (childState.display == DisplayStyle.None || childState.positionType == PositionType.Absolute)
                    continue;

                float childAvailWidth = 0;
                float childAvailHeight = 0;
                MeasureMode childWidthMode = MeasureMode.Undefined;
                MeasureMode childHeightMode = MeasureMode.Undefined;

                float childFreeMain = availableMain;
                if (!isWrap && mainMode != MeasureMode.Undefined)
                    childFreeMain = availableMain - currentLineMain - (itemsInCurrentLine > 0 ? gapMain : 0);

                if (isRow)
                {
                    childAvailWidth = childFreeMain;
                    childWidthMode = mainMode;
                    childAvailHeight = availableCross;
                    childHeightMode = crossMode;
                }
                else
                {
                    childAvailHeight = childFreeMain;
                    childHeightMode = mainMode;
                    childAvailWidth = availableCross;
                    childWidthMode = crossMode;
                }

                child.Measure(new LayoutConstraint(childAvailWidth, childAvailHeight, childWidthMode, childHeightMode));
                
                float childMarginMain = isRow ? childState.margin.horizontal : childState.margin.vertical;
                float childMarginCross = isRow ? childState.margin.vertical : childState.margin.horizontal;

                float childOuterMain = (isRow ? child.m_desiredSize.X : child.m_desiredSize.Y) + childMarginMain;
                float childOuterCross = (isRow ? child.m_desiredSize.Y : child.m_desiredSize.X) + childMarginCross;
                
                if (isWrap && itemsInCurrentLine > 0 && mainMode != MeasureMode.Undefined && currentLineMain + gapMain + childOuterMain > availableMain)
                {
                    totalMain = Math.Max(totalMain, currentLineMain);
                    maxCross += currentLineMaxCross + gapCross;
                    
                    currentLineMain = childOuterMain;
                    currentLineMaxCross = childOuterCross;
                    itemsInCurrentLine = 1;
                }
                else
                {
                    if (itemsInCurrentLine > 0) currentLineMain += gapMain;
                    currentLineMain += childOuterMain;
                    currentLineMaxCross = Math.Max(currentLineMaxCross, childOuterCross);
                    itemsInCurrentLine++;
                }
            }

            totalMain = Math.Max(totalMain, currentLineMain);
            maxCross += currentLineMaxCross;

            float desiredMain = totalMain + paddingMain;
            float desiredCross = maxCross + paddingCross;

            return isRow ? new Vector2(desiredMain, desiredCross) : new Vector2(desiredCross, desiredMain);
        }

        /// <summary>
        /// 執行排版引擎的第二階段：佈局 (Arrange Pass)。
        /// 根據父節點計算出的絕對矩形 (finalAbsoluteRect) 正式設定此元件的佈局範圍 (layoutRect)，並向下佈局子節點。
        /// </summary>
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

        private class FlexLine
        {
            public List<VisualElement> items = new List<VisualElement>();
            public float mainSize = 0;
            public float crossSize = 0;
            public float totalFlexGrow = 0;
        }

        /// <summary>
        /// 供子類別覆寫的實際佈局邏輯。
        /// 預設實作為根據 Flexbox 規則排列並設定所有子節點的最終絕對位置。
        /// </summary>
        protected virtual void ArrangeContent(Rect finalAbsoluteRect)
        {
            if (m_useNativeLayout) return;

            var state = resolvedLayoutState;
            bool isRow = state.flexDirection == FlexDirection.Row;
            bool isWrap = state.flexWrap == FlexWrap.Wrap;
            
            float borderX = 0;
            if (resolvedStyle.TryGetFloat((int)ImGuiStyleVar.WindowBorderSize, out float bw)) borderX = bw;
            float borderY = borderX;
            
            float padLeft = state.padding.left + borderX;
            float padTop = state.padding.top + borderY;
            float padRight = state.padding.right + borderX;
            float padBottom = state.padding.bottom + borderY;

            Rect contentRect = new Rect(
                finalAbsoluteRect.x + padLeft,
                finalAbsoluteRect.y + padTop,
                Math.Max(0, finalAbsoluteRect.width - padLeft - padRight),
                Math.Max(0, finalAbsoluteRect.height - padTop - padBottom)
            );

            System.Numerics.Vector2 itemSpacing = this.theme.itemSpacing;
            if (resolvedStyle.TryGetVector2((int)ImGuiStyleVar.ItemSpacing, out var overrideSpacing))
                itemSpacing = overrideSpacing;
            else
                itemSpacing *= Hexa.NET.ImGui.ImGui.GetWindowViewport().DpiScale;
            float gapMain = isRow ? itemSpacing.X : itemSpacing.Y;
            float gapCross = isRow ? itemSpacing.Y : itemSpacing.X;

            float availableMain = isRow ? contentRect.width : contentRect.height;
            float availableCross = isRow ? contentRect.height : contentRect.width;

            var absoluteChildren = new List<VisualElement>();
            var flexLines = new List<FlexLine>();
            FlexLine currentLine = new FlexLine();
            
            int childCount = hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = hierarchy.ChildAt(i);
                if (child.resolvedLayoutState.display == DisplayStyle.None) continue;
                
                if (child.resolvedLayoutState.positionType == PositionType.Absolute)
                {
                    absoluteChildren.Add(child);
                    continue;
                }

                var childState = child.resolvedLayoutState;
                float childMarginMain = isRow ? childState.margin.horizontal : childState.margin.vertical;
                float childMarginCross = isRow ? childState.margin.vertical : childState.margin.horizontal;
                float childOuterMain = (isRow ? child.m_desiredSize.X : child.m_desiredSize.Y) + childMarginMain;
                float childOuterCross = (isRow ? child.m_desiredSize.Y : child.m_desiredSize.X) + childMarginCross;

                if (isWrap && currentLine.items.Count > 0 && currentLine.mainSize + gapMain + childOuterMain > availableMain)
                {
                    flexLines.Add(currentLine);
                    currentLine = new FlexLine();
                }

                if (currentLine.items.Count > 0) currentLine.mainSize += gapMain;
                currentLine.mainSize += childOuterMain;
                currentLine.crossSize = Math.Max(currentLine.crossSize, childOuterCross);
                currentLine.totalFlexGrow += childState.flexGrow;
                currentLine.items.Add(child);
            }
            if (currentLine.items.Count > 0) flexLines.Add(currentLine);

            if (!isWrap && flexLines.Count == 1)
            {
                flexLines[0].crossSize = availableCross;
            }

            // Layout each line
            float currentCrossPos = 0;
            foreach (var line in flexLines)
            {
                float freeMainSpace = availableMain - line.mainSize;
                float spacing = gapMain;
                float currentMainPos = 0;

                if (freeMainSpace > 0 && line.totalFlexGrow == 0)
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
                            if (line.items.Count > 1)
                                spacing = (availableMain - (line.mainSize - gapMain * (line.items.Count - 1))) / (line.items.Count - 1);
                            break;
                    }
                }

                // First pass for FlexGrow resolution
                var flexItems = new List<VisualElement>();
                if (freeMainSpace > 0 && line.totalFlexGrow > 0)
                {
                    float totalGrow = line.totalFlexGrow;
                    float remainingFreeSpace = freeMainSpace;
                    bool spaceReassigned;
                    do
                    {
                        spaceReassigned = false;
                        foreach (var item in line.items)
                        {
                            if (flexItems.Contains(item)) continue;
                            if (item.resolvedLayoutState.flexGrow == 0) continue;
                            
                            float flexGrow = item.resolvedLayoutState.flexGrow;
                            float baseMain = isRow ? item.m_desiredSize.X : item.m_desiredSize.Y;
                            float targetMain = baseMain + (flexGrow / totalGrow) * remainingFreeSpace;
                            float maxLimit = isRow ? (item.resolvedLayoutState.maxWidth ?? float.MaxValue) : (item.resolvedLayoutState.maxHeight ?? float.MaxValue);
                            
                            if (targetMain > maxLimit)
                            {
                                flexItems.Add(item);
                                remainingFreeSpace -= (maxLimit - baseMain);
                                totalGrow -= flexGrow;
                                spaceReassigned = true;
                            }
                        }
                    } while (spaceReassigned && totalGrow > 0 && remainingFreeSpace > 0);
                    
                    freeMainSpace = remainingFreeSpace;
                    line.totalFlexGrow = totalGrow;
                }

                foreach (var child in line.items)
                {
                    var childState = child.resolvedLayoutState;
                    float childDesiredMain = isRow ? child.m_desiredSize.X : child.m_desiredSize.Y;
                    float childDesiredCross = isRow ? child.m_desiredSize.Y : child.m_desiredSize.X;
                    
                    float childMarginMain = isRow ? childState.margin.horizontal : childState.margin.vertical;
                    float childMarginCross = isRow ? childState.margin.vertical : childState.margin.horizontal;
                    
                    float childMarginLeftTop = isRow ? childState.margin.left : childState.margin.top;
                    float childMarginRightBottom = isRow ? childState.margin.right : childState.margin.bottom;
                    float childMarginCrossLeftTop = isRow ? childState.margin.top : childState.margin.left;
                    float childMarginCrossRightBottom = isRow ? childState.margin.bottom : childState.margin.right;

                    float childActualMain = childDesiredMain;
                    if (freeMainSpace > 0 && line.totalFlexGrow > 0 && childState.flexGrow > 0)
                    {
                        if (flexItems.Contains(child))
                        {
                            childActualMain = isRow ? (childState.maxWidth ?? childDesiredMain) : (childState.maxHeight ?? childDesiredMain);
                        }
                        else
                        {
                            childActualMain += (childState.flexGrow / line.totalFlexGrow) * freeMainSpace;
                        }
                    }

                    if (isRow)
                    {
                        if (childState.minWidth.HasValue) childActualMain = Math.Max(childActualMain, childState.minWidth.Value);
                        if (childState.maxWidth.HasValue) childActualMain = Math.Min(childActualMain, childState.maxWidth.Value);
                    }
                    else
                    {
                        if (childState.minHeight.HasValue) childActualMain = Math.Max(childActualMain, childState.minHeight.Value);
                        if (childState.maxHeight.HasValue) childActualMain = Math.Min(childActualMain, childState.maxHeight.Value);
                    }

                    float childActualCross = childDesiredCross;
                    float itemCrossPos = currentCrossPos;
                    AlignItems align = childState.alignSelf;
                    
                    float freeCrossSpace = line.crossSize - (childActualCross + childMarginCross);
                    
                    if (align == AlignItems.Stretch && ((isRow && !childState.height.HasValue) || (!isRow && !childState.width.HasValue)))
                    {
                        childActualCross = line.crossSize - childMarginCross;
                    }
                    else if (align == AlignItems.Center)
                    {
                        itemCrossPos += freeCrossSpace / 2f;
                    }
                    else if (align == AlignItems.FlexEnd)
                    {
                        itemCrossPos += freeCrossSpace;
                    }

                    float x, y, w, h;
                    if (isRow)
                    {
                        x = contentRect.x + currentMainPos + childMarginLeftTop;
                        y = contentRect.y + itemCrossPos + childMarginCrossLeftTop;
                        w = childActualMain;
                        h = childActualCross;
                    }
                    else
                    {
                        x = contentRect.x + itemCrossPos + childMarginCrossLeftTop;
                        y = contentRect.y + currentMainPos + childMarginLeftTop;
                        w = childActualCross;
                        h = childActualMain;
                    }
                    
                    currentMainPos += childActualMain + childMarginMain + spacing;
                    child.Arrange(new Rect(x, y, w, h));
                }
                
                currentCrossPos += line.crossSize + gapCross;
            }
            
            // Layout Absolute Children
            foreach (var absChild in absoluteChildren)
            {
                var absState = absChild.resolvedLayoutState;
                float x = contentRect.x + (absState.left ?? 0) + absState.margin.left;
                float y = contentRect.y + (absState.top ?? 0) + absState.margin.top;
                float w = absState.width ?? absChild.m_desiredSize.X;
                float h = absState.height ?? absChild.m_desiredSize.Y;
                
                if (absState.right.HasValue && !absState.left.HasValue)
                    x = contentRect.x + contentRect.width - absState.right.Value - absState.margin.right - w;
                else if (absState.right.HasValue && absState.left.HasValue && !absState.width.HasValue)
                    w = Math.Max(0, contentRect.width - absState.left.Value - absState.right.Value - absState.margin.horizontal);

                if (absState.bottom.HasValue && !absState.top.HasValue)
                    y = contentRect.y + contentRect.height - absState.bottom.Value - absState.margin.bottom - h;
                else if (absState.bottom.HasValue && absState.top.HasValue && !absState.height.HasValue)
                    h = Math.Max(0, contentRect.height - absState.top.Value - absState.bottom.Value - absState.margin.vertical);
                    
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
                    bool isNull = prop.dataType == StyleDataType.Null;
                    if (prop.key == StyleKey.Width.Hash) state.width = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.Height.Hash) state.height = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.MinWidth.Hash) state.minWidth = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.MaxWidth.Hash) state.maxWidth = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.MinHeight.Hash) state.minHeight = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.MaxHeight.Hash) state.maxHeight = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.MarginLeft.Hash) state.margin.left = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.MarginTop.Hash) state.margin.top = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.MarginRight.Hash) state.margin.right = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.MarginBottom.Hash) state.margin.bottom = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.PaddingLeft.Hash) state.padding.left = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.PaddingTop.Hash) state.padding.top = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.PaddingRight.Hash) state.padding.right = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.PaddingBottom.Hash) state.padding.bottom = isNull ? 0 : prop.floatValue;
                    else if (prop.key == StyleKey.FlexDirection.Hash) state.flexDirection = isNull ? FlexDirection.Column : (FlexDirection)prop.enumValue;
                    else if (prop.key == StyleKey.FlexWrap.Hash) state.flexWrap = isNull ? FlexWrap.NoWrap : (FlexWrap)prop.enumValue;
                    else if (prop.key == StyleKey.JustifyContent.Hash) state.justifyContent = isNull ? JustifyContent.FlexStart : (JustifyContent)prop.enumValue;
                    else if (prop.key == StyleKey.AlignItems.Hash) state.alignItems = isNull ? AlignItems.Stretch : (AlignItems)prop.enumValue;
                    else if (prop.key == StyleKey.FlexGrow.Hash) state.flexGrow = isNull ? 0f : prop.floatValue;
                    else if (prop.key == StyleKey.AlignSelf.Hash) state.alignSelf = isNull ? state.alignItems : (AlignItems)prop.enumValue;
                    else if (prop.key == StyleKey.PositionType.Hash) state.positionType = isNull ? PositionType.Relative : (PositionType)prop.enumValue;
                    else if (prop.key == StyleKey.Top.Hash) state.top = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.Bottom.Hash) state.bottom = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.Left.Hash) state.left = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.Right.Hash) state.right = isNull ? null : prop.floatValue;
                    else if (prop.key == StyleKey.Display.Hash) state.display = isNull ? DisplayStyle.Flex : (DisplayStyle)prop.enumValue;
                }
            }

            resolvedLayoutState = state;
        }


        public VisualElement()
        {
            m_elementId = ++s_elementCounter;
            internalStyle = new Style();
            internalStyle.Init(this);
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

        internal ImTKTheme m_theme;

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
            if (this.hierarchy.parent != null && !m_useNativeLayout)
            {
                ImGui.SetCursorScreenPos(this.layoutRect.position);
            }
            return true;
        }

        public virtual void Update()
        {
            int childCount = hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                hierarchy.ChildAt(i).Update();
            }
        }

        public Window GetWindow()
        {
            VisualElement current = this;
            while (current != null)
            {
                if (current is Window w) return w;
                current = current.hierarchy.parent;
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

        /// <summary>
        /// Schedules an action to be executed safely at the end of the current frame.
        /// Useful for modifying UI structures or state during restricted phases like GuiRender.
        /// </summary>
        public void ScheduleDeferred(Action action)
        {
            ImTKApplication.ScheduleDeferred(action);
        }

        internal bool HasAnyCallback()
        {
            return m_callbacks != null && m_callbacks.Count > 0;
        }
    }

    public class VisualElement<TStyle> : VisualElement where TStyle : IVisualElementStyle, new()
    {
        public new TStyle style => (TStyle)internalStyle;

        public VisualElement()
        {
            internalStyle = new TStyle();
            internalStyle.Init(this);
            m_isStyleDirty = true;
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
