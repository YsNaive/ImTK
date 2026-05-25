using System;

namespace ImTK.UI
{
    public enum MeasureMode : byte
    {
        Undefined,
        Exactly,
        AtMost
    }

    public struct LayoutConstraint : IEquatable<LayoutConstraint>
    {
        public float AvailableWidth;
        public float AvailableHeight;
        public MeasureMode WidthMode;
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
