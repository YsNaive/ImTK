using ImGuiNET;
using System;

namespace ImTK;

public class Horizontal : VisualElement
{
    public override void RenderVisualTree(double deltaTime)
    {
        style?.Push();
        try
        {
            this.Render(deltaTime);

            int count = hierarchy.childrenCount;
            if (count > 0)
            {
                hierarchy.BeginIteration();
                try
                {
                    bool first = true;
                    for (int i = 0; i < count; i++)
                    {
                        var ve = hierarchy.ChildAt(i);
                        if (ve != null && ve.enable && ve.parent == this)
                        {
                            if (!first)
                            {
                                ImGui.SameLine();
                            }
                            ve.RenderVisualTree(deltaTime);
                            first = false;
                        }
                    }
                }
                finally
                {
                    hierarchy.EndIteration();
                }
            }
        }
        finally
        {
            style?.Pop();
        }
    }
}
