using ImGuiNET;
using System;

namespace ImTK;

public class IntField : FieldElement<int>
{
    public IntField(string text, int initialValue = 0) : base(text, initialValue)
    {
    }

    public override void Render(double deltaTime)
    {
        int v = value;
        if (ImGui.InputInt(text, ref v))
        {
            value = v;
        }
    }
}
