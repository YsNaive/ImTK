using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace ImTK.UI
{
    public enum StyleCategory : byte
    {
        ImGuiStyle = 0,
        ThemeToken = 1,
        HighLevelToken = 2,
        Layout = 3
    }

    public enum StyleDataType : byte
    {
        Null = 0,
        Float = 1,
        Vector2 = 2,
        Color = 3,
        HashedString = 4,
        Int = 5,
        Enum = 6
    }

    [Flags]
    public enum StyleFlags : byte
    {
        None = 0,
        Inheritable = 1 << 0,
        LayoutAffecting = 1 << 1,
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct StyleProperty
    {
        [FieldOffset(0)] public StyleCategory category;
        [FieldOffset(1)] public StyleDataType dataType;
        [FieldOffset(2)] public StyleFlags flags;

        [FieldOffset(4)] public int key;

        [FieldOffset(8)] public float floatValue;
        [FieldOffset(8)] public Vector2 vector2Value;
        [FieldOffset(8)] public uint colorValue;
        [FieldOffset(8)] public int tokenHash;
        [FieldOffset(8)] public int intValue;
        [FieldOffset(8)] public int enumValue;

        public bool isResolved => category == StyleCategory.ImGuiStyle && dataType != StyleDataType.HashedString;

        public bool isInheritable
        {
            get => (flags & StyleFlags.Inheritable) != 0;
            set
            {
                if (value)
                    flags |= StyleFlags.Inheritable;
                else
                    flags &= ~StyleFlags.Inheritable;
            }
        }

        // Backward compatibility
        public bool isToken => category == StyleCategory.ThemeToken || dataType == StyleDataType.HashedString;
        public bool isNull => dataType == StyleDataType.Null;

    }
}
