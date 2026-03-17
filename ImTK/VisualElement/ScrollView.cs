using ImGuiNET;
using System;
using System.Numerics;

namespace ImTK;

public class ScrollView : VisualElement
{
    private static int s_scrollIdCount = 0;
    private string m_childId;

    public ScrollView(string id = null)
    {
        if (id == null)
        {
            m_childId = $"ScrollView_{++s_scrollIdCount}";
        }
        else
        {
            m_childId = id;
        }
    }

    public override void RenderVisualTree(double deltaTime)
    {
        style?.Push();
        try
        {
            this.Render(deltaTime);

            if (ImGui.BeginChild(m_childId, Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
            {
                int count = hierarchy.childrenCount;
                if (count > 0)
                {
                    hierarchy.BeginIteration();
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var ve = hierarchy.ChildAt(i);
                            if (ve != null && ve.enable && ve.parent == this)
                            {
                                ve.RenderVisualTree(deltaTime);
                            }
                        }
                    }
                    finally
                    {
                        hierarchy.EndIteration();
                    }
                }
            }
            ImGui.EndChild();
        }
        finally
        {
            style?.Pop();
        }
    }
}
