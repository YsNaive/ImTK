using System;

namespace ImTK.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SliderIntAttribute : Attribute
    {
        public int min { get; }
        public int max { get; }

        public SliderIntAttribute(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
