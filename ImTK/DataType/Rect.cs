using System;
using System.Numerics;

namespace ImTK
{
    public struct Rect : IEquatable<Rect>
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.x = position.X;
            this.y = position.Y;
            this.width = size.X;
            this.height = size.Y;
        }

        public Vector2 position
        {
            get => new Vector2(x, y);
            set { x = value.X; y = value.Y; }
        }

        public Vector2 size
        {
            get => new Vector2(width, height);
            set { width = value.X; height = value.Y; }
        }

        public Vector2 min
        {
            get => new Vector2(x, y);
            set { x = value.X; y = value.Y; }
        }

        public Vector2 max
        {
            get => new Vector2(x + width, y + height);
            set { width = value.X - x; height = value.Y - y; }
        }

        public Vector2 center
        {
            get => new Vector2(x + width / 2f, y + height / 2f);
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= x && point.X < x + width && point.Y >= y && point.Y < y + height;
        }

        public static bool operator ==(Rect lhs, Rect rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
        public static bool operator !=(Rect lhs, Rect rhs) => !(lhs == rhs);

        public override bool Equals(object obj) => obj is Rect other && Equals(other);
        public bool Equals(Rect other) => x == other.x && y == other.y && width == other.width && height == other.height;
        public override int GetHashCode() => HashCode.Combine(x, y, width, height);

        public override string ToString() => $"(x:{x}, y:{y}, width:{width}, height:{height})";
    }
}
