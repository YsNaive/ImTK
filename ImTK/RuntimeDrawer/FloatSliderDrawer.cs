using ImGuiNET;

namespace ImTK;

public class FloatSliderDrawer : RuntimeDrawer<float>
{
    public float min;
    public float max;

    public FloatSliderDrawer(string label = "", float min = 0f, float max = 100f, float initialValue = 0f) : base(label, initialValue)
    {
        this.min = min;
        this.max = max;
    }

    protected override void OnRenderDrawer()
    {
        float v = m_value;
        ImGui.PushItemWidth(-1);
        if (ImGui.SliderFloat($"##{GetHashCode()}", ref v, min, max))
        {
            value = v;
        }
        ImGui.PopItemWidth();
    }
}
