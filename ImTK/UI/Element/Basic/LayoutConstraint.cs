using System;

namespace ImTK.UI
{
    public enum FlexDirection { Column, Row }
    public enum FlexWrap { NoWrap, Wrap }
    public enum JustifyContent { FlexStart, Center, FlexEnd, SpaceBetween }
    public enum AlignItems { FlexStart, Center, FlexEnd, Stretch }
    public enum PositionType { Static, Relative, Absolute }
    public enum DisplayStyle { Flex, None }
    public enum Overflow { Visible, Hidden }

    /// <summary>
    /// 定義在排版計算 (Measure) 時，父節點對子節點的尺寸測量模式約束。
    /// </summary>
    public enum MeasureMode : byte
    {
        /// <summary>不受限制。子節點可以自由決定其所需尺寸 (通常用於 ScrollView 內部或 FlexGrow)。</summary>
        Undefined,
        /// <summary>精確值。子節點必須精確符合父節點給定的尺寸 (通常受到 Width/Height 等明確樣式約束)。</summary>
        Exactly,
        /// <summary>最大限制。子節點的尺寸不能超過父節點給定的尺寸 (通常受到 MaxWidth/MaxHeight 或可用空間的約束)。</summary>
        AtMost
    }

    /// <summary>
    /// 封裝排版計算 (Measure) 階段，父節點傳遞給子節點的可用空間與約束模式。
    /// </summary>
    public struct LayoutConstraint : IEquatable<LayoutConstraint>
    {
        /// <summary>父節點提供的可用寬度</summary>
        public float AvailableWidth;
        /// <summary>父節點提供的可用高度</summary>
        public float AvailableHeight;
        /// <summary>寬度的約束測量模式</summary>
        public MeasureMode WidthMode;
        /// <summary>高度的約束測量模式</summary>
        public MeasureMode HeightMode;

        public LayoutConstraint(float width, float height, MeasureMode widthMode, MeasureMode heightMode)
        {
            AvailableWidth = width;
            AvailableHeight = height;
            WidthMode = widthMode;
            HeightMode = heightMode;
        }

        public bool Equals(LayoutConstraint other)
        {
            return AvailableWidth == other.AvailableWidth &&
                   AvailableHeight == other.AvailableHeight &&
                   WidthMode == other.WidthMode &&
                   HeightMode == other.HeightMode;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutConstraint other && Equals(other);
        }

        public static bool operator ==(LayoutConstraint left, LayoutConstraint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayoutConstraint left, LayoutConstraint right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AvailableWidth, AvailableHeight, WidthMode, HeightMode);
        }
    }
}
