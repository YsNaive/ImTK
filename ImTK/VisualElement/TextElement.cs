using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImTK;

public class TextElement : VisualElement
{
    public string text;
    public TextElement(string text = "")
    {
        this.text = text;
    }

    public override void Render(double deltaTime)
    {
        ImGui.Text(text);
    }
}
