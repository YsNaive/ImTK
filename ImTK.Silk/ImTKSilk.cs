using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Maths;
using System;
using ImGuiNET;

namespace ImTK.Silk;

public static class ImTKSilk
{
    private static IWindow s_window;
    private static GL s_gl;
    private static IInputContext s_input;
    private static ImGuiController s_controller;
    
    private static Action<float> s_onUpdate;
    private static Action<float> s_onRender;

    public static void Initialize(string title, int width, int height, Action<float> onUpdate, Action<float> onRender)
    {
        s_onUpdate = onUpdate;
        s_onRender = onRender;

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;

        s_window = Window.Create(options);

        s_window.Load += OnLoad;
        s_window.Update += OnUpdate;
        s_window.Render += OnRender;
        s_window.FramebufferResize += OnFramebufferResize;
        s_window.Closing += OnClose;
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
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

                int fontSize = 16;
                string 
                    fontPath = @"C:\Windows\Fonts\jf-openhuninn-2.1.ttf";
                if (!System.IO.File.Exists(fontPath))
                    fontPath = @"C:\Windows\Fonts\msjh.ttc";
                if (!System.IO.File.Exists(fontPath))
                    fontPath = "";

                if (!string.IsNullOrEmpty(fontPath))
                {
                    io.Fonts.AddFontFromFileTTF(fontPath, fontSize, null, io.Fonts.GetGlyphRangesChineseFull());
                }
                else
                {
                    io.FontGlobalScale = fontSize / 13f; // 13 是 ImGui 預設字體大小
                }
            }
        );
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
        s_onUpdate?.Invoke(dt);
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

        s_onRender?.Invoke(dt);

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
        s_controller?.Dispose();
        s_input?.Dispose();
        s_gl?.Dispose();
        s_window?.Dispose();
    }
}