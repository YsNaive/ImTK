using System;

namespace ImTK.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomFieldDrawerAttribute : Attribute
    {
        public Type targetType { get; }
        public Type requiredModifier { get; }
        public bool allowInheritType { get; }

        public CustomFieldDrawerAttribute(Type targetType, Type requiredModifier = null, bool allowInheritType = true)
        {
            this.targetType = targetType;
            this.requiredModifier = requiredModifier;
            this.allowInheritType = allowInheritType;
        }
    }
}
