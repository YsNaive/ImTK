using System;
using ImGuiNET;

namespace ImTK;

/// <summary>
/// The non-generic base class for a drawer element.
/// Handles layout, indentation, label rendering, and event bubbling.
/// </summary>
public abstract class RuntimeDrawer : VisualElement, IDrawer
{
    private string m_label;
    private int m_indentLevel = 0;
    private event Action m_onValueChanged;

    /// <summary>
    /// The label text of the drawer.
    /// </summary>
    public string label
    {
        get => m_label;
        set => m_label = value;
    }

    /// <summary>
    /// The indent level of the drawer.
    /// </summary>
    public int indentLevel
    {
        get => m_indentLevel;
        set => m_indentLevel = value;
    }

    protected RuntimeDrawer(string label = "")
    {
        m_label = label;
    }

    public abstract object GetValue();
    public abstract void SetValue(object value);
    public abstract void SetValueWithoutNotify(object newValue);

    public void RegisterValueChanged(Action callback)
    {
        m_onValueChanged += callback;
    }

    public void UnregisterValueChanged(Action callback)
    {
        m_onValueChanged -= callback;
    }

    /// <summary>
    /// Gets the cascaded drawer label width from styles, or the default value.
    /// </summary>
    protected float GetDrawerLabelWidth()
    {
        VisualElement current = this;
        while (current != null)
        {
            if (current.style != null && current.style.drawerLabelWidth.HasValue)
            {
                return current.style.drawerLabelWidth.Value;
            }
            current = current.parent;
        }
        return ImGui.CalcTextSize("A").X * 14f;
    }

    /// <summary>
    /// Gets the cascaded drawer indent width from styles, or the default value.
    /// </summary>
    protected float GetDrawerIndentWidth()
    {
        VisualElement current = this;
        while (current != null)
        {
            if (current.style != null && current.style.drawerIndentWidth.HasValue)
            {
                return current.style.drawerIndentWidth.Value;
            }
            current = current.parent;
        }
        return ImGui.CalcTextSize("A").X * 2f;
    }

    /// <summary>
    /// Triggers the value changed event and bubbles it up the UI tree.
    /// </summary>
    protected void NotifyValueChanged()
    {
        m_onValueChanged?.Invoke();

        // Bubble up to the nearest IDrawer
        VisualElement p = this.parent;
        while (p != null)
        {
            if (p is RuntimeDrawer drawer)
            {
                drawer.NotifyValueChanged();
                break;
            }
            p = p.parent;
        }
    }

    public override void RenderVisualTree(double deltaTime)
    {
        if (!enable) return;

        float indentWidth = GetDrawerIndentWidth() * m_indentLevel;
        float labelWidth = GetDrawerLabelWidth();
        bool hasLabel = !string.IsNullOrEmpty(m_label);

        // Calculate cursor starting position including absolute indent
        float startX = ImGui.GetCursorPosX() + indentWidth;
        ImGui.SetCursorPosX(startX);

        // Draw label with fixed width block
        if (hasLabel)
        {
            // Instead of using ImGui.Text directly, we ensure the label aligns to a fixed width
            // by using an invisible item or setting cursor pos after text.
            ImGui.Text(m_label);

            // Advance cursor to the start position of the input field
            ImGui.SameLine();
            ImGui.SetCursorPosX(startX + labelWidth);
        }
        else
        {
            // If no label, we might still want to shift the controls to align properly,
            // but typical behavior without label just takes up the indent space.
        }

        // Let subclasses render their specific ImGui controls
        OnRenderDrawer();

        // Render children inside contentContainer
        hierarchy.BeginIteration();
        foreach (var child in hierarchy.Children())
        {
            child.RenderVisualTree(deltaTime);
        }
        hierarchy.EndIteration();
    }

    /// <summary>
    /// Subclasses should override this method to render their specific ImGui controls.
    /// This is called after the label and indentation are handled.
    /// </summary>
    protected virtual void OnRenderDrawer()
    {
        // Default implementation does nothing, letting children in contentContainer render.
    }
}
