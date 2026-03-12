using ImGuiNET;
using System;

namespace ImTK;

public class Toggle : FieldElement<bool>
{
    public Toggle(string text, bool initialValue = false) : base(text, initialValue)
    {
    }

    public override void Render(double deltaTime)
    {
        bool v = value;
        if (ImGui.Checkbox(text, ref v))
        {
            value = v;
        }
    }
}
