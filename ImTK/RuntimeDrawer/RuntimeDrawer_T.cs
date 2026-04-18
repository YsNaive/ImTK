using System;
using System.Collections.Generic;

namespace ImTK;

/// <summary>
/// The generic base class for a drawer element.
/// Manages the strongly-typed value and change detection.
/// </summary>
public abstract class RuntimeDrawer<T> : RuntimeDrawer
{
    protected T m_value;

    /// <summary>
    /// The strongly-typed value of the drawer.
    /// Setting this will check for equality and trigger change events if different.
    /// </summary>
    public T value
    {
        get => m_value;
        set => SetValue(value);
    }

    protected RuntimeDrawer(string label = "", T initialValue = default) : base(label)
    {
        m_value = initialValue;
    }

    /// <summary>
    /// Sets the value and triggers change events if the value is different.
    /// </summary>
    public void SetValue(T newValue)
    {
        if (!EqualityComparer<T>.Default.Equals(m_value, newValue))
        {
            m_value = newValue;
            NotifyValueChanged();
        }
    }

    /// <summary>
    /// Sets the value without triggering any change events.
    /// </summary>
    public void SetValueWithoutNotify(T newValue)
    {
        m_value = newValue;
    }

    // IDrawer non-generic bridge implementations
    public override object GetValue() => m_value;

    public override void SetValue(object value)
    {
        if (value is T typedValue)
        {
            SetValue(typedValue);
        }
        else if (value == null && !typeof(T).IsValueType)
        {
             SetValue(default(T));
        }
    }

    public override void SetValueWithoutNotify(object newValue)
    {
        if (newValue is T typedValue)
        {
            SetValueWithoutNotify(typedValue);
        }
        else if (newValue == null && !typeof(T).IsValueType)
        {
             SetValueWithoutNotify(default(T));
        }
    }
}
