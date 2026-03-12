using ImGuiNET;
using System;

namespace ImTK;

public class FloatSlider : FieldElement<float>
{
    public float min;
    public float max;

    public FloatSlider(string text, float min, float max, float initialValue = 0f) : base(text, initialValue)
    {
        this.min = min;
        this.max = max;
    }

    public override void Render(double deltaTime)
    {
        float v = value;
        if (ImGui.SliderFloat(text, ref v, min, max))
        {
            value = v;
        }
    }
}
