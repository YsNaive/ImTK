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

        bool hasLabel = !string.IsNullOrEmpty(m_label);

        // Apply indent
        if (m_indentLevel > 0)
        {
            ImGui.Indent(m_indentLevel * ImGui.GetTreeNodeToLabelSpacing());
        }

        // Draw label
        if (hasLabel)
        {
            ImGui.Text(m_label);
            ImGui.SameLine();
        }

        // Let subclasses render their specific ImGui controls
        OnRenderDrawer();

        // Revert indent
        if (m_indentLevel > 0)
        {
            ImGui.Unindent(m_indentLevel * ImGui.GetTreeNodeToLabelSpacing());
        }

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
