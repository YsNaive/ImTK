using System;
using System.Numerics;

namespace ImTK
{
    public struct Color : IEquatable<Color>
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a = 1.0f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(Vector4 rgba)
        {
            this.r = rgba.X;
            this.g = rgba.Y;
            this.b = rgba.Z;
            this.a = rgba.W;
        }

        public Vector3 rgb
        {
            get => new Vector3(r, g, b);
            set
            {
                r = value.X;
                g = value.Y;
                b = value.Z;
            }
        }

        public Vector4 rgba
        {
            get => new Vector4(r, g, b, a);
            set
            {
                r = value.X;
                g = value.Y;
                b = value.Z;
                a = value.W;
            }
        }

        public uint ToUInt32()
        {
            uint R = (uint)(Math.Clamp(r, 0.0f, 1.0f) * 255.0f + 0.5f);
            uint G = (uint)(Math.Clamp(g, 0.0f, 1.0f) * 255.0f + 0.5f);
            uint B = (uint)(Math.Clamp(b, 0.0f, 1.0f) * 255.0f + 0.5f);
            uint A = (uint)(Math.Clamp(a, 0.0f, 1.0f) * 255.0f + 0.5f);
            return (A << 24) | (B << 16) | (G << 8) | R;
        }

        public void FromUInt32(uint value)
        {
            r = ((value) & 0xFF) / 255.0f;
            g = ((value >> 8) & 0xFF) / 255.0f;
            b = ((value >> 16) & 0xFF) / 255.0f;
            a = ((value >> 24) & 0xFF) / 255.0f;
        }

        public uint u32
        {
            get => ToUInt32();
            set => FromUInt32(value);
        }

        // HSV support
        public Vector3 hsv
        {
            get
            {
                ColorToHSV(r, g, b, out float h, out float s, out float v);
                return new Vector3(h, s, v);
            }
            set
            {
                HSVToColor(value.X, value.Y, value.Z, out float R, out float G, out float B);
                r = R;
                g = G;
                b = B;
            }
        }

        public float h
        {
            get => hsv.X;
            set
            {
                var current = hsv;
                hsv = new Vector3(value, current.Y, current.Z);
            }
        }

        public float s
        {
            get => hsv.Y;
            set
            {
                var current = hsv;
                hsv = new Vector3(current.X, value, current.Z);
            }
        }

        public float v
        {
            get => hsv.Z;
            set
            {
                var current = hsv;
                hsv = new Vector3(current.X, current.Y, value);
            }
        }

        private static void ColorToHSV(float r, float g, float b, out float h, out float s, out float v)
        {
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            v = max;

            float delta = max - min;
            if (max != 0)
                s = delta / max;
            else
            {
                s = 0;
                h = -1;
                return;
            }

            if (r == max)
                h = (g - b) / delta;
            else if (g == max)
                h = 2 + (b - r) / delta;
            else
                h = 4 + (r - g) / delta;

            h *= 60;
            if (h < 0)
                h += 360;

            h /= 360f; // normalize 0-1
        }

        private static void HSVToColor(float h, float s, float v, out float r, out float g, out float b)
        {
            h *= 360f;
            if (s == 0)
            {
                r = g = b = v;
                return;
            }

            h /= 60;
            int i = (int)Math.Floor(h);
            float f = h - i;
            float p = v * (1 - s);
            float q = v * (1 - s * f);
            float t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0:
                    r = v; g = t; b = p;
                    break;
                case 1:
                    r = q; g = v; b = p;
                    break;
                case 2:
                    r = p; g = v; b = t;
                    break;
                case 3:
                    r = p; g = q; b = v;
                    break;
                case 4:
                    r = t; g = p; b = v;
                    break;
                default:
                    r = v; g = p; b = q;
                    break;
            }
        }

        public static implicit operator Vector4(Color c) => c.rgba;
        public static implicit operator Color(Vector4 v) => new Color(v);
        public static implicit operator uint(Color c) => c.u32;
        public static explicit operator Color(uint u) { var c = new Color(); c.u32 = u; return c; }

        public static Color White => new Color(1, 1, 1, 1);
        public static Color Black => new Color(0, 0, 0, 1);
        public static Color Clear => new Color(0, 0, 0, 0);
        public static Color Red => new Color(1, 0, 0, 1);
        public static Color Green => new Color(0, 1, 0, 1);
        public static Color Blue => new Color(0, 0, 1, 1);
        public static Color Yellow => new Color(1, 1, 0, 1);
        public static Color Cyan => new Color(0, 1, 1, 1);
        public static Color Magenta => new Color(1, 0, 1, 1);
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f, 1);

        public bool Equals(Color other) => r == other.r && g == other.g && b == other.b && a == other.a;
        public override bool Equals(object obj) => obj is Color other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(r, g, b, a);

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !left.Equals(right);
    }
}
