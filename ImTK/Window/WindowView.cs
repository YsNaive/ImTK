using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ImTK;

public abstract class WindowView : VisualElement
{
    #region static utils

    public readonly static VisualElement openedWindows = new ();
    private readonly static Dictionary<Type, WindowView> windowsTable = new();
    /// <summary>
    /// Add window T to openedWindows, each T will only got 1 instance with this function
    /// </summary>
    public static T Open<T>() where T : WindowView, new()
    {
        WindowView matched = null;
        windowsTable.TryGetValue (typeof (T), out matched);
        if (matched == null) { 
            matched = new T();
            windowsTable.Add (typeof (T), matched);
        }
        openedWindows.Add (matched);
        return matched as T;
    }

    public static void RenderAll(double deltaTime)
    {
        openedWindows.RenderVisualTree(deltaTime);
    }
    public static void UpdateAll(double deltaTime)
    {
        openedWindows.UpdateVisualTree(deltaTime);
    }

    #endregion

    public abstract string displayName { get; }
    public Vector2 minSize = new Vector2(300, 200);
    public override void RenderVisualTree(double deltaTime)
    {
        ImGui.SetNextWindowSizeConstraints(minSize, Vector2.PositiveInfinity);
        ImGui.Begin(displayName);
        base.RenderVisualTree(deltaTime);
        ImGui.End();
    }
}
