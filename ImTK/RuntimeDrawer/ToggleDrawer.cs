using ImGuiNET;

namespace ImTK;

public class ToggleDrawer : RuntimeDrawer<bool>
{
    public ToggleDrawer(string label = "", bool initialValue = false) : base(label, initialValue)
    {
    }

    protected override void OnRenderDrawer()
    {
        bool v = m_value;
        if (ImGui.Checkbox($"##{GetHashCode()}", ref v))
        {
            value = v;
        }
    }
}
