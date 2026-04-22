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

    /// <summary>
    /// If true, the drawer will automatically pull the value from the getter delegate in its Update loop.
    /// </summary>
    public bool autoSync { get; set; } = false;

    /// <summary>
    /// Optional delegate to retrieve the external value.
    /// Used for auto-syncing or data binding.
    /// </summary>
    public Func<T> getter { get; set; }

    /// <summary>
    /// Optional delegate to write the value back to an external source.
    /// Used for data binding.
    /// </summary>
    public Action<T> setter { get; set; }

    protected RuntimeDrawer(string label = "", T initialValue = default) : base(label)
    {
        m_value = initialValue;

        // Automatically write back to setter when UI value changes
        RegisterValueChanged(() =>
        {
            setter?.Invoke(m_value);
        });
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        if (autoSync && getter != null)
        {
            T extValue = getter();
            if (!EqualityComparer<T>.Default.Equals(m_value, extValue))
            {
                SetValueWithoutNotify(extValue);
            }
        }
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
