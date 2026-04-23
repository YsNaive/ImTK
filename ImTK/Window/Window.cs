using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Reflection;

namespace ImTK;

public abstract class Window : VisualElement
{
    #region static utils

    public static string configFolderPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    public class WindowStateData
    {
        public string typeName { get; set; }
        public string customDataJson { get; set; }
    }

    private static void SaveWindowState()
    {
        try
        {
            if (!Directory.Exists(configFolderPath))
                Directory.CreateDirectory(configFolderPath);

            string path = Path.Combine(configFolderPath, "window_state.json");

            var states = new List<WindowStateData>();
            foreach (var kvp in windowsTable)
            {
                var window = kvp.Value;
                if (window.isOpen)
                {
                    states.Add(new WindowStateData
                    {
                        typeName = kvp.Key.AssemblyQualifiedName,
                        customDataJson = window.SerializeState()
                    });
                }
            }

            string json = JsonSerializer.Serialize(states);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImTK] Failed to save window state: {ex.Message}");
        }
    }

    public readonly static VisualElement openedWindows = new ();
    private readonly static Dictionary<Type, Window> windowsTable = new();
    private readonly static HashSet<string> s_usedWindowNames = new();

    /// <summary>
    /// Open a singleton window of type T
    /// </summary>
    public static T Open<T>() where T : Window, new()
    {
        Window matched = null;
        windowsTable.TryGetValue(typeof(T), out matched);
        if (matched == null)
        {
            matched = new T();
            windowsTable.Add(typeof(T), matched);
        }

        matched.Open();
        SaveWindowState();
        return matched as T;
    }

    private class Module : ImTKModule
    {
        private Module() { }

        public override void OnLoad()
        {
            try
            {
                string path = Path.Combine(configFolderPath, "window_state.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var openWindowTypes = JsonSerializer.Deserialize<List<WindowStateData>>(json);
                    bool needsResave = false;

                    if (openWindowTypes != null)
                    {
                        foreach (var state in openWindowTypes)
                        {
                            Type type = Type.GetType(state.typeName);
                            if (type != null && type.IsSubclassOf(typeof(Window)))
                            {
                                MethodInfo openMethod = typeof(Window).GetMethod("Open", BindingFlags.Public | BindingFlags.Static);
                                if (openMethod != null)
                                {
                                    MethodInfo genericOpen = openMethod.MakeGenericMethod(type);
                                    Window windowInstance = (Window)genericOpen.Invoke(null, null);
                                    if (!string.IsNullOrEmpty(state.customDataJson))
                                    {
                                        windowInstance.DeserializeState(state.customDataJson);
                                    }
                                }
                            }
                            else
                            {
                                needsResave = true;
                            }
                        }
                    }

                    if (needsResave)
                    {
                        SaveWindowState();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImTK] Failed to load window state: {ex.Message}");
            }
        }

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

    /// <summary>
    /// Override this to return custom state to be saved
    /// </summary>
    public virtual string SerializeState() { return null; }

    /// <summary>
    /// Override this to restore custom state
    /// </summary>
    public virtual void DeserializeState(string json) { }


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
    public readonly MenuItem contextMenu = new MenuItem("window-context-menu-root");

    public bool isOpen { get; set; } = false;


    public virtual void Open()
    {
        if (isOpen)
        {
            ImGui.SetWindowFocus(displayName);
            return;
        }

        if (s_usedWindowNames.Contains(displayName))
        {
            throw new InvalidOperationException($"Window name '{displayName}' is already in use. Window names must be unique to prevent ImGui layout conflicts.");
        }

        s_usedWindowNames.Add(displayName);
        isOpen = true;
        openedWindows.Add(this);
        ImGui.SetWindowFocus(displayName);
    }

    public virtual void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        openedWindows.Remove(this);
        s_usedWindowNames.Remove(displayName);

        // If it's a singleton tool window, we resave state.
        // If it's a dynamically instantiated window, we don't save it to windowsTable anyway.
        if (windowsTable.ContainsValue(this))
        {
            SaveWindowState();
        }
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
                    menuBar.RenderMenuTree();
                    ImGui.EndMenuBar();
                }
            }

            if (enableContextMenu && contextMenu.childrenCount > 0)
            {
                ImGui.SetNextWindowSizeConstraints(new Vector2(200, 150), Vector2.PositiveInfinity);
                if (ImGui.BeginPopupContextWindow())
                {
                    contextMenu.RenderMenuTree();
                    ImGui.EndPopup();
                }
            }

            base.RenderVisualTree(deltaTime);
        }

        ImGui.End();
    }
}
