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
        matched.isOpen = true;
        openedWindows.Add (matched);
        return matched as T;
    }

    private class Module : ImTKModule
    {
        private Module() { }

        public override void Update(double deltaTime)
        {
            openedWindows.UpdateVisualTree(deltaTime);
        }

        public override void Render(double deltaTime)
        {
            openedWindows.RenderVisualTree(deltaTime);
        }
    }

    #endregion

    public abstract string displayName { get; }
    public Vector2 minSize = new Vector2(300, 200);

    public ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None;

    public bool enableDocking
    {
        get => !windowFlags.HasFlag(ImGuiWindowFlags.NoDocking);
        set
        {
            if (value) windowFlags &= ~ImGuiWindowFlags.NoDocking;
            else windowFlags |= ImGuiWindowFlags.NoDocking;
        }
    }

    public bool isResizable
    {
        get => !windowFlags.HasFlag(ImGuiWindowFlags.NoResize);
        set
        {
            if (value) windowFlags &= ~ImGuiWindowFlags.NoResize;
            else windowFlags |= ImGuiWindowFlags.NoResize;
        }
    }

    public bool isMovable
    {
        get => !windowFlags.HasFlag(ImGuiWindowFlags.NoMove);
        set
        {
            if (value) windowFlags &= ~ImGuiWindowFlags.NoMove;
            else windowFlags |= ImGuiWindowFlags.NoMove;
        }
    }

    public bool isCollapsible
    {
        get => !windowFlags.HasFlag(ImGuiWindowFlags.NoCollapse);
        set
        {
            if (value) windowFlags &= ~ImGuiWindowFlags.NoCollapse;
            else windowFlags |= ImGuiWindowFlags.NoCollapse;
        }
    }

    public bool showTitleBar
    {
        get => !windowFlags.HasFlag(ImGuiWindowFlags.NoTitleBar);
        set
        {
            if (value) windowFlags &= ~ImGuiWindowFlags.NoTitleBar;
            else windowFlags |= ImGuiWindowFlags.NoTitleBar;
        }
    }

    public bool enableMenuBar
    {
        get => windowFlags.HasFlag(ImGuiWindowFlags.MenuBar);
        set
        {
            if (value) windowFlags |= ImGuiWindowFlags.MenuBar;
            else windowFlags &= ~ImGuiWindowFlags.MenuBar;
        }
    }
    public readonly MenuItem menuBar = new MenuItem("MenuBar");

    public bool enableContextMenu { get; set; } = true;
    public readonly VisualElement contextMenu = new VisualElement();

    public bool isOpen { get; set; } = true;

    public virtual void Close()
    {
        isOpen = false;
        openedWindows.Remove(this);
        windowsTable.Remove(this.GetType());
    }

    public override void RenderVisualTree(double deltaTime)
    {
        if (!isOpen) return;

        ImGui.SetNextWindowSizeConstraints(minSize, Vector2.PositiveInfinity);

        bool isWindowOpen = isOpen;
        bool isAppearing = ImGui.Begin(displayName, ref isWindowOpen, windowFlags);

        if (!isWindowOpen)
        {
            ImGui.End();
            Close();
            return;
        }

        if (isAppearing)
        {
            if (enableMenuBar && menuBar.childrenCount > 0)
            {
                if (ImGui.BeginMenuBar())
                {
                    foreach (var child in menuBar.Children())
                    {
                        child.Render();
                    }
                    ImGui.EndMenuBar();
                }
            }

            if (enableContextMenu && contextMenu.childrenCount > 0)
            {
                ImGui.SetNextWindowSizeConstraints(new Vector2(200, 150), Vector2.PositiveInfinity);
                if (ImGui.BeginPopupContextWindow())
                {
                    contextMenu.RenderVisualTree(deltaTime);
                    ImGui.EndPopup();
                }
            }

            base.RenderVisualTree(deltaTime);
        }

        ImGui.End();
    }
}
