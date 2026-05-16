using System.Runtime.InteropServices;
using System.Numerics;
using ImTK.Core;

namespace ImTK.UI
{
    public enum StyleVarType
    {
        Color,
        Float,
        Vector2
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct StyleProperty
    {
        [FieldOffset(0)] public int Key; // ImGuiCol or ImGuiStyleVar
        [FieldOffset(4)] public StyleVarType Type;
        [FieldOffset(8)] public StyleKeyword Keyword;

        [FieldOffset(12)] public uint ColorValue; // Store Color as uint (ABGR)
        [FieldOffset(12)] public float FloatValue;
        [FieldOffset(12)] public Vector2 Vector2Value;

        [FieldOffset(20)] public int TokenHash; // Store Hash of HashedString

        public bool IsToken => TokenHash != 0;
        public bool IsNull => Keyword == StyleKeyword.Null;
    }
}
