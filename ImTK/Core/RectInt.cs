using System;

namespace ImTK.Core
{
    public struct RectInt : IEquatable<RectInt>
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public RectInt(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectInt(Vector2Int position, Vector2Int size)
        {
            this.x = position.x;
            this.y = position.y;
            this.width = size.x;
            this.height = size.y;
        }

        public Vector2Int position
        {
            get => new Vector2Int(x, y);
            set { x = value.x; y = value.y; }
        }

        public Vector2Int size
        {
            get => new Vector2Int(width, height);
            set { width = value.x; height = value.y; }
        }

        public Vector2Int min
        {
            get => new Vector2Int(x, y);
            set { x = value.x; y = value.y; }
        }

        public Vector2Int max
        {
            get => new Vector2Int(x + width, y + height);
            set { width = value.x - x; height = value.y - y; }
        }

        public Vector2Int center
        {
            get => new Vector2Int(x + width / 2, y + height / 2);
        }

        public bool Contains(Vector2Int point)
        {
            return point.x >= x && point.x < x + width && point.y >= y && point.y < y + height;
        }

        public static explicit operator Rect(RectInt r) => new Rect(r.x, r.y, r.width, r.height);
        public static explicit operator RectInt(Rect r) => new RectInt((int)MathF.Round(r.x), (int)MathF.Round(r.y), (int)MathF.Round(r.width), (int)MathF.Round(r.height));

        public static bool operator ==(RectInt lhs, RectInt rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
        public static bool operator !=(RectInt lhs, RectInt rhs) => !(lhs == rhs);

        public override bool Equals(object obj) => obj is RectInt other && Equals(other);
        public bool Equals(RectInt other) => x == other.x && y == other.y && width == other.width && height == other.height;
        public override int GetHashCode() => HashCode.Combine(x, y, width, height);

        public override string ToString() => $"(x:{x}, y:{y}, width:{width}, height:{height})";
    }
}
