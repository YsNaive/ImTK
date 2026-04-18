using ImGuiNET;

namespace ImTK;

public class FloatDrawer : RuntimeDrawer<float>
{
    public FloatDrawer(string label = "", float initialValue = 0f) : base(label, initialValue)
    {
    }

    protected override void OnRenderDrawer()
    {
        float v = m_value;
        ImGui.PushItemWidth(-1);
        if (ImGui.InputFloat($"##{GetHashCode()}", ref v))
        {
            value = v;
        }
        ImGui.PopItemWidth();
    }
}
