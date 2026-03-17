using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Maths;
using System;
using ImGuiNET;
using System.IO;
using System.Runtime.InteropServices;

namespace ImTK.Silk;

public static class ImTKSilk
{
    private static IWindow s_window;
    private static GL s_gl;
    private static IInputContext s_input;
    private static ImGuiController s_controller;
    
    private static ImTKSilkConstant s_constant;
    private static IntPtr s_iniFilenamePtr = IntPtr.Zero;

    public static event Action<double> onUpdate;
    public static event Action<double> onRender;

    public static void Initialize(ImTKSilkConstant constant)
    {
        ImTKModule.InitializeAll();

        s_constant = constant ?? new ImTKSilkConstant();

        var options = s_constant.options ?? WindowOptions.Default;
        options.Size = new Vector2D<int>(s_constant.width, s_constant.height);
        options.Title = s_constant.title;

        s_window = Window.Create(options);

        s_window.Load += OnLoad;
        s_window.Update += OnUpdate;
        s_window.Render += OnRender;
        s_window.FramebufferResize += OnFramebufferResize;
        s_window.Closing += OnClose;

        if (!Directory.Exists(s_constant.configFolderPath))
        {
            Directory.CreateDirectory(s_constant.configFolderPath);
        }

        WindowView.configFolderPath = s_constant.configFolderPath;
    }

    public static void Start()
    {
        s_window?.Run();
    }

    private static void OnLoad()
    {
        s_gl = s_window.CreateOpenGL();
        s_input = s_window.CreateInput();

        s_controller = new ImGuiController(
            s_gl,
            s_window,
            s_input,
            () =>
            {
                ImGuiIOPtr io = ImGui.GetIO();
                io.ConfigFlags |= s_constant.configFlags;

                if (!string.IsNullOrEmpty(s_constant.fontPath) && System.IO.File.Exists(s_constant.fontPath))
                {
                    io.Fonts.AddFontFromFileTTF(s_constant.fontPath, s_constant.fontSize, null, io.Fonts.GetGlyphRangesChineseFull());
                }
                else
                {
                    io.FontGlobalScale = s_constant.fontSize / 13f; // 13 is ImGui default font size
                }
            }
        );

        unsafe
        {
            ImGuiIOPtr io = ImGui.GetIO();
            string iniPath = Path.Combine(s_constant.configFolderPath, "imgui.ini");
            // Allocate string in unmanaged memory, ImGui takes ownership or just reads it?
            // Usually we can just allocate and leak this small string for the lifetime of the program,
            // or store the pointer to free it on close. For simplicity we store a static pointer and free on close.
            s_iniFilenamePtr = Marshal.StringToCoTaskMemUTF8(iniPath);
            io.NativePtr->IniFilename = (byte*)s_iniFilenamePtr.ToPointer();
        }

        ImTKModule.LoadAll();
    }

    private static void OnFramebufferResize(Vector2D<int> size)
    {
        s_gl.Viewport(size);

        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(size.X, size.Y);
    }

    private static void OnUpdate(double deltaTime)
    {
        float dt = (float)deltaTime;
        s_controller.Update(dt);
        ImTKModule.UpdateAll(dt);
        onUpdate?.Invoke(dt);
    }

    private static void OnRender(double deltaTime)
    {
        float dt = (float)deltaTime;

        s_gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        ImGuiIOPtr io = ImGui.GetIO();

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags windowFlags =
            ImGuiWindowFlags.NoDocking              |
            ImGuiWindowFlags.NoTitleBar             |
            ImGuiWindowFlags.NoCollapse             |
            ImGuiWindowFlags.NoResize               |
            ImGuiWindowFlags.NoMove                 |
            ImGuiWindowFlags.NoBringToFrontOnFocus  |
            ImGuiWindowFlags.NoNavFocus;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

        ImGui.Begin("main-dock-space", windowFlags);

        ImGui.PopStyleVar(2);

        uint dockspaceId = ImGui.GetID("main-dock-space");

        ImGui.DockSpace(
            dockspaceId,
            System.Numerics.Vector2.Zero,
            ImGuiDockNodeFlags.PassthruCentralNode
        );

        ImTKModule.RenderAll(dt);

        onRender?.Invoke(dt);

        ImGui.End();

        s_controller.Render();

        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }
    }

    private static void OnClose()
    {
        s_window.IsVisible = s_constant.visibleOnClose;
        ImTKModule.CloseAll();

        s_controller?.Dispose();

        if (s_iniFilenamePtr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(s_iniFilenamePtr);
            s_iniFilenamePtr = IntPtr.Zero;
        }
        s_input?.Dispose();
        s_gl?.Dispose();
        s_window?.Dispose();
    }
}