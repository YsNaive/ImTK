using ImGuiNET;
using System;

namespace ImTK;

public class FloatField : FieldElement<float>
{
    public FloatField(string text, float initialValue = 0f) : base(text, initialValue)
    {
    }

    public override void Render(double deltaTime)
    {
        float v = value;
        if (ImGui.InputFloat(text, ref v))
        {
            value = v;
        }
    }
}
