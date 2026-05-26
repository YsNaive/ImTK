using System;

namespace ImTK.UI
{
    public struct Thickness : IEquatable<Thickness>
    {
        public float left;
        public float top;
        public float right;
        public float bottom;

        public Thickness(float uniformLength)
        {
            left = top = right = bottom = uniformLength;
        }

        public Thickness(float horizontal, float vertical)
        {
            left = right = horizontal;
            top = bottom = vertical;
        }

        public Thickness(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public float horizontal => left + right;
        public float vertical => top + bottom;

        public bool Equals(Thickness other)
        {
            return left == other.left && top == other.top && right == other.right && bottom == other.bottom;
        }

        public override bool Equals(object obj)
        {
            return obj is Thickness other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(left, top, right, bottom);
        }

        public static bool operator ==(Thickness left, Thickness right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Thickness left, Thickness right)
        {
            return !left.Equals(right);
        }

        public static readonly Thickness Zero = new Thickness(0);

        public static implicit operator Thickness(System.Numerics.Vector2 vector)
        {
            return new Thickness(vector.X, vector.Y);
        }
    }
}
