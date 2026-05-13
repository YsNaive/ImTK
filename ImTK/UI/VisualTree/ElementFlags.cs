using System;

namespace ImTK.UI
{
    public abstract class ElementFlags<TEnum> where TEnum : struct, Enum
    {
        public TEnum Value { get; set; }

        public ElementFlags()
        {
            Value = default;
        }

        public ElementFlags(TEnum initialValue)
        {
            Value = initialValue;
        }

        protected void SetFlag(TEnum flag, bool state)
        {
            int mask = Convert.ToInt32(flag);
            int current = Convert.ToInt32(Value);
            if (state)
            {
                current |= mask;
            }
            else
            {
                current &= ~mask;
            }
            Value = (TEnum)Enum.ToObject(typeof(TEnum), current);
        }

        protected bool GetFlag(TEnum flag)
        {
            int mask = Convert.ToInt32(flag);
            int current = Convert.ToInt32(Value);
            return (current & mask) == mask;
        }
    }
}
