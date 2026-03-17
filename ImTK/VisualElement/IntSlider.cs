using ImGuiNET;
using System;

namespace ImTK;

public class IntSlider : FieldElement<int>
{
    public int min;
    public int max;

    public IntSlider(string text, int min, int max, int initialValue = 0) : base(text, initialValue)
    {
        this.min = min;
        this.max = max;
    }

    public override void Render(double deltaTime)
    {
        int v = value;
        if (ImGui.SliderInt(text, ref v, min, max))
        {
            value = v;
        }
    }
}
