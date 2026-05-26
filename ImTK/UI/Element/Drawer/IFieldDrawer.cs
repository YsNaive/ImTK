using System;

namespace ImTK.UI
{
    public interface IFieldDrawer
    {
        object value { get; set; }
        string label { get; set; }
        float? labelWidth { get; set; }
        void ApplyModifier(Attribute modifier);
        void SetValueWithoutNotify(object newValue);
    }

    public interface IFieldDrawer<T> : IFieldDrawer
    {
        new T value { get; set; }
    }
}
