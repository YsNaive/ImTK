using System;

namespace ImTK.UI
{
    public interface IFieldDrawer
    {
        object value { get; set; }
        string label { get; set; }
        float? labelWidth { get; set; }
        ImTK.Core.Rect? overrideRenderRect { get; set; }
        void ApplyModifier(Attribute modifier);
    }

    public interface IFieldDrawer<T> : IFieldDrawer
    {
        new T value { get; set; }
    }
}
