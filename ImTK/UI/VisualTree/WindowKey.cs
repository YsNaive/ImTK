using System;

namespace ImTK.UI
{
    internal struct WindowKey : IEquatable<WindowKey>
    {
        public Type Type;
        public string WindowId;

        public WindowKey(Type type, string windowId)
        {
            Type = type;
            WindowId = windowId;
        }

        public bool Equals(WindowKey other)
        {
            return Type == other.Type && WindowId == other.WindowId;
        }

        public override bool Equals(object obj)
        {
            return obj is WindowKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
                hash = hash * 23 + (WindowId != null ? WindowId.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
