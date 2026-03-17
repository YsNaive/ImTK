using Silk.NET.Windowing;
using ImGuiNET;
using System;

namespace ImTK.Silk;

public class ImTKSilkConstant
{
    public string title { get; set; } = "ImTK Silk Window";
    public int width { get; set; } = 1280;
    public int height { get; set; } = 720;

    /// <summary>
    /// Base WindowOptions. If null, WindowOptions.Default is used,
    /// then Title, Size (Width/Height) are applied on top of it.
    /// </summary>
    public WindowOptions? options { get; set; }

    /// <summary>
    /// Path to the TTF font file for ImGui. Leave null or empty to use ImGui default font.
    /// </summary>
    public string fontPath { get; set; } = string.Empty;
    public int fontSize { get; set; } = 16;

    public ImGuiConfigFlags configFlags { get; set; } = ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;

    /// <summary>
    /// Directory path where imgui.ini and window_state.json will be saved.
    /// Defaults to the application base directory.
    /// </summary>
    public string configFolderPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// set window.visible
    /// make window visible/invisible right after user close it
    /// </summary>
    public bool visibleOnClose { get; set; } = true;
}
