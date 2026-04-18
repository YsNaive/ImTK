using System;

namespace dashboard.Dashboard.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EntityTypeAttribute : Attribute
    {
        public new byte TypeId { get; }
        public bool IsReference { get; }

        public EntityTypeAttribute(byte typeId, bool isReference)
        {
            TypeId = typeId;
            IsReference = isReference;
        }
    }
}