using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImTK;

public class Button : TextElement
{
    public event Action clicked;
    public Button(string text, Action onClick) : base(text)
    {
        this.clicked += onClick;
    }

    public override void Render(double deltaTime)
    {
        if (ImGui.Button(text))
        {
            this.clicked.Invoke();
        }
    }
}
