using Silk.NET.Windowing;
using ImGuiNET;
using System;

namespace ImTK.Silk;

public class ImTKSilkConstant
{
    public string WindowTitle { get; set; } = "ImTK Silk Window";
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;

    /// <summary>
    /// Base WindowOptions. If null, WindowOptions.Default is used,
    /// then Title, Size (Width/Height) are applied on top of it.
    /// </summary>
    public WindowOptions? CustomWindowOptions { get; set; }

    /// <summary>
    /// Path to the TTF font file for ImGui. Leave null or empty to use ImGui default font.
    /// </summary>
    public string FontPath { get; set; } = string.Empty;
    public int FontSize { get; set; } = 16;

    public ImGuiConfigFlags ConfigFlags { get; set; } = ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;

    public Action<float> OnUpdate { get; set; }
    public Action<float> OnRender { get; set; }
}
