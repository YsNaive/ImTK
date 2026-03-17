using ImGuiNET;
using System;

namespace ImTK;

public class TextField : FieldElement<string>
{
    public uint maxLength = 256;

    public TextField(string text, string initialValue = "") : base(text, initialValue)
    {
    }

    public override void Render(double deltaTime)
    {
        string v = value ?? string.Empty;
        if (ImGui.InputText(text, ref v, maxLength))
        {
            value = v;
        }
    }
}
