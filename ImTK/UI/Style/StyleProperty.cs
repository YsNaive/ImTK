using System.Runtime.InteropServices;
using System.Numerics;

namespace ImTK.UI.Style
{
    public enum StylePropertyType : byte
    {
        Null = 0,
        Token = 1,
        ColorValue = 2,
        FloatValue = 3,
        Vector2Value = 4,
        IntValue = 5,
        EnumValue = 6
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct StyleProperty
    {
        [FieldOffset(0)] public int key;
        // key is now treated as HashedString.Hash
        [FieldOffset(4)] public StylePropertyType type;

        [FieldOffset(8)] public uint colorValue;
        [FieldOffset(8)] public float floatValue;
        [FieldOffset(8)] public Vector2 vector2Value;
        [FieldOffset(8)] public int intValue;
        [FieldOffset(8)] public int tokenHash;
        [FieldOffset(8)] public int enumValue;

        public bool isNull => type == StylePropertyType.Null;
        public bool isToken => type == StylePropertyType.Token;
    }
}
