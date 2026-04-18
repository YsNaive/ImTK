using ImGuiNET;

namespace ImTK;

public class IntDrawer : RuntimeDrawer<int>
{
    public IntDrawer(string label = "", int initialValue = 0) : base(label, initialValue)
    {
    }

    protected override void OnRenderDrawer()
    {
        int v = m_value;
        // Use an empty string for the internal ImGui label, since the label is drawn by the base RuntimeDrawer
        ImGui.PushItemWidth(-1);
        if (ImGui.InputInt($"##{GetHashCode()}", ref v))
        {
            value = v;
        }
        ImGui.PopItemWidth();
    }
}
