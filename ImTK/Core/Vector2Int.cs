using System;
using System.Numerics;

namespace ImTK.Core
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(Vector2Int v) => new Vector2(v.x, v.y);
        public static explicit operator Vector2Int(Vector2 v) => new Vector2Int((int)MathF.Round(v.X), (int)MathF.Round(v.Y));

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.x + b.x, a.y + b.y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.x - b.x, a.y - b.y);
        public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new Vector2Int(a.x * b.x, a.y * b.y);
        public static Vector2Int operator *(Vector2Int a, int d) => new Vector2Int(a.x * d, a.y * d);
        public static Vector2Int operator *(int d, Vector2Int a) => new Vector2Int(a.x * d, a.y * d);
        public static Vector2Int operator /(Vector2Int a, int d) => new Vector2Int(a.x / d, a.y / d);

        public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => lhs.x == rhs.x && lhs.y == rhs.y;
        public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

        public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);
        public bool Equals(Vector2Int other) => x == other.x && y == other.y;
        public override int GetHashCode() => HashCode.Combine(x, y);

        public override string ToString() => $"({x}, {y})";
    }
}
