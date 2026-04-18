using System;

namespace ImTK;

/// <summary>
/// Defines the non-generic interface for a drawer element.
/// </summary>
public interface IDrawer
{
    object GetValue();
    void SetValue(object value);
    void SetValueWithoutNotify(object newValue);

    void RegisterValueChanged(Action callback);
    void UnregisterValueChanged(Action callback);
}
