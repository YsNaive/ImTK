using System;

namespace ImTK;

public abstract class FieldElement<T> : TextElement, IFieldElement
{
    private T m_value;
    private event Action onValueChanged;

    public T value
    {
        get => m_value;
        set
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_value, value))
            {
                m_value = value;
                onValueChanged?.Invoke();
            }
        }
    }

    protected FieldElement(string text, T initialValue = default) : base(text)
    {
        m_value = initialValue;
    }

    public void RegisterValueChanged(Action callback)
    {
        onValueChanged += callback;
    }

    public void UnregisterValueChanged(Action callback)
    {
        onValueChanged -= callback;
    }
}
