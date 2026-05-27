using System;

namespace ImTK.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SliderFloatAttribute : Attribute
    {
        public float min { get; }
        public float max { get; }

        public SliderFloatAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
