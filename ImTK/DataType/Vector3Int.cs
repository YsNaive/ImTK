using System;
using System.Numerics;

namespace ImTK
{
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        public int x;
        public int y;
        public int z;

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(Vector3Int v) => new Vector3(v.x, v.y, v.z);
        public static explicit operator Vector3Int(Vector3 v) => new Vector3Int((int)MathF.Round(v.X), (int)MathF.Round(v.Y), (int)MathF.Round(v.Z));

        public static Vector3Int operator +(Vector3Int a, Vector3Int b) => new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3Int operator -(Vector3Int a, Vector3Int b) => new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3Int operator *(Vector3Int a, Vector3Int b) => new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3Int operator *(Vector3Int a, int d) => new Vector3Int(a.x * d, a.y * d, a.z * d);
        public static Vector3Int operator *(int d, Vector3Int a) => new Vector3Int(a.x * d, a.y * d, a.z * d);
        public static Vector3Int operator /(Vector3Int a, int d) => new Vector3Int(a.x / d, a.y / d, a.z / d);

        public static bool operator ==(Vector3Int lhs, Vector3Int rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        public static bool operator !=(Vector3Int lhs, Vector3Int rhs) => !(lhs == rhs);

        public override bool Equals(object obj) => obj is Vector3Int other && Equals(other);
        public bool Equals(Vector3Int other) => x == other.x && y == other.y && z == other.z;
        public override int GetHashCode() => HashCode.Combine(x, y, z);

        public override string ToString() => $"({x}, {y}, {z})";
    }
}
