using ImGuiNET;

namespace ImTK;

public class IntSliderDrawer : RuntimeDrawer<int>
{
    public int min;
    public int max;

    public IntSliderDrawer(string label = "", int min = 0, int max = 100, int initialValue = 0) : base(label, initialValue)
    {
        this.min = min;
        this.max = max;
    }

    protected override void OnRenderDrawer()
    {
        int v = m_value;
        ImGui.PushItemWidth(-1);
        if (ImGui.SliderInt($"##{GetHashCode()}", ref v, min, max))
        {
            value = v;
        }
        ImGui.PopItemWidth();
    }
}
