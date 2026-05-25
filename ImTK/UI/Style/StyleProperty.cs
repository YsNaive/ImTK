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

        public StylePropertyType type
        {
            get
            {
                if (dataType == StyleDataType.Null) return StylePropertyType.Null;
                if (dataType == StyleDataType.HashedString) return StylePropertyType.Token;
                if (dataType == StyleDataType.Color) return StylePropertyType.ColorValue;
                if (dataType == StyleDataType.Float) return StylePropertyType.FloatValue;
                if (dataType == StyleDataType.Vector2) return StylePropertyType.Vector2Value;
                if (dataType == StyleDataType.Int) return StylePropertyType.IntValue;
                if (dataType == StyleDataType.Enum) return StylePropertyType.EnumValue;
                return StylePropertyType.Null;
            }
            set
            {
                switch (value)
                {
                    case StylePropertyType.Null: dataType = StyleDataType.Null; break;
                    case StylePropertyType.Token: dataType = StyleDataType.HashedString; break;
                    case StylePropertyType.ColorValue: dataType = StyleDataType.Color; break;
                    case StylePropertyType.FloatValue: dataType = StyleDataType.Float; break;
                    case StylePropertyType.Vector2Value: dataType = StyleDataType.Vector2; break;
                    case StylePropertyType.IntValue: dataType = StyleDataType.Int; break;
                    case StylePropertyType.EnumValue: dataType = StyleDataType.Enum; break;
                }
            }
        }
    }

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
}
