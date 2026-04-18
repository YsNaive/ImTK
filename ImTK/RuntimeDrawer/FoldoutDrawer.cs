using ImGuiNET;

namespace ImTK;

/// <summary>
/// A drawer that can be expanded or collapsed to show its child elements.
/// Increases the indent level for its children.
/// </summary>
public class FoldoutDrawer : RuntimeDrawer
{
    private bool m_isOpen = false;

    /// <summary>
    /// Gets or sets whether the foldout is open (expanded).
    /// </summary>
    public bool isOpen
    {
        get => m_isOpen;
        set => m_isOpen = value;
    }

    public FoldoutDrawer(string label = "", bool defaultOpen = false) : base(label)
    {
        m_isOpen = defaultOpen;
    }

    public override object GetValue() => m_isOpen;

    public override void SetValue(object value)
    {
        if (value is bool b)
        {
            if (m_isOpen != b)
            {
                m_isOpen = b;
                NotifyValueChanged();
            }
        }
    }

    public override void SetValueWithoutNotify(object newValue)
    {
        if (newValue is bool b)
        {
            m_isOpen = b;
        }
    }

    public override void RenderVisualTree(double deltaTime)
    {
        if (!enable) return;

        // Apply indent
        if (indentLevel > 0)
        {
            ImGui.Indent(indentLevel * ImGui.GetTreeNodeToLabelSpacing());
        }

        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth;
        if (m_isOpen)
        {
            flags |= ImGuiTreeNodeFlags.DefaultOpen;
        }

        // Draw the collapsing header / tree node
        bool isNodeOpen = ImGui.TreeNodeEx($"##{GetHashCode()}", flags, label);

        if (isNodeOpen != m_isOpen)
        {
            m_isOpen = isNodeOpen;
            NotifyValueChanged();
        }

        // Render children inside contentContainer if open
        if (isNodeOpen)
        {
            hierarchy.BeginIteration();
            foreach (var child in hierarchy.Children())
            {
                if (child is RuntimeDrawer childDrawer)
                {
                    // Increase indent for child drawers
                    childDrawer.indentLevel = this.indentLevel + 1;
                }
                child.RenderVisualTree(deltaTime);
            }
            hierarchy.EndIteration();

            ImGui.TreePop();
        }

        // Revert indent
        if (indentLevel > 0)
        {
            ImGui.Unindent(indentLevel * ImGui.GetTreeNodeToLabelSpacing());
        }
    }
}
