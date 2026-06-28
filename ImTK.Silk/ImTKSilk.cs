using System;
using System.IO;
using Silk.NET.OpenGL;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using ImTK.Core;
using ImTK.Event;
using ImTK.Log;

namespace ImTK.Silk
{
    /// <summary>
    /// The entry point and main loop driver for ImTK using Hexa.NET.GLFW and Silk.NET.OpenGL.
    /// </summary>
    public static class ImTKSilk
    {
        private static GL s_gl;
        private static ImTKSilkConstant s_config;
        private static string s_iniFilePath;
        private static unsafe Hexa.NET.GLFW.GLFWwindowPtr s_window;

        /// <summary>
        /// Initializes the GLFW window, ImGui context, and starts the ImTKApplication lifecycle loop.
        /// This method blocks until the window is closed.
        /// </summary>
        public static unsafe void Run(ImTKSilkConstant config = null)
        {
            s_config = config ?? new ImTKSilkConstant();

            if (Hexa.NET.GLFW.GLFW.Init() == 0)
            {
                ImTKLog.Error("Failed to initialize GLFW.");
                return;
            }

            Hexa.NET.GLFW.GLFW.WindowHint(139266 /* GLFW_CONTEXT_VERSION_MAJOR */, 3);
            Hexa.NET.GLFW.GLFW.WindowHint(139267 /* GLFW_CONTEXT_VERSION_MINOR */, 3);
            Hexa.NET.GLFW.GLFW.WindowHint(139272 /* GLFW_OPENGL_PROFILE */, 204801 /* GLFW_OPENGL_CORE_PROFILE */);
            Hexa.NET.GLFW.GLFW.WindowHint(139270 /* GLFW_OPENGL_FORWARD_COMPAT */, 1);

            s_window = Hexa.NET.GLFW.GLFW.CreateWindow(s_config.windowWidth, s_config.windowHeight, s_config.windowTitle, null, null);
            if (s_window.Handle == null)
            {
                ImTKLog.Error("Failed to create GLFW window.");
                Hexa.NET.GLFW.GLFW.Terminate();
                return;
            }

            Hexa.NET.GLFW.GLFW.MakeContextCurrent(s_window);
            Hexa.NET.GLFW.GLFW.SwapInterval(s_config.vsync ? 1 : 0);

            // Initialize Silk.NET OpenGL using Hexa.NET's GLFW GetProcAddress
            s_gl = GL.GetApi(name => (nint)Hexa.NET.GLFW.GLFW.GetProcAddress(name));

            ImTK.Event.ImTKEventBus.GlobalSubscribe<ImTK.Event.OnFontChangedEvent>(OnFontChanged);

            ImGui.CreateContext();
            var io = ImGui.GetIO();

            string folderPath = Path.IsPathRooted(s_config.configFolderPath)
                ? s_config.configFolderPath
                : Path.Combine(ImTKEnvironment.LocalDataPath, s_config.configFolderPath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            s_iniFilePath = Path.Combine(folderPath, "imgui.ini");
            io.IniFilename = (byte*)System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(s_iniFilePath);

            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            if (s_config.enableViewports)
            {
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                io.ConfigViewportsNoDecoration = false;
            }
            io.ConfigWindowsMoveFromTitleBarOnly = s_config.windowMoveFromTitleBarOnly;
            ImGuiImplGLFW.SetCurrentContext(ImGui.GetCurrentContext());
            ImGuiImplOpenGL3.SetCurrentContext(ImGui.GetCurrentContext());

            ImGuiImplGLFW.InitForOpenGL((Hexa.NET.ImGui.Backends.GLFW.GLFWwindow*)s_window.Handle, true);
            ImGuiImplOpenGL3.Init("#version 330");

            float xScale, yScale;
            unsafe { Hexa.NET.GLFW.GLFW.GetWindowContentScale(s_window, &xScale, &yScale); }
            if (xScale > 0) ImTK.UI.RenderEngine.Context.MainViewportDpiScale = xScale;

            ImTKApplication.Lifecycle.Initialize();
            ImTKApplication.Lifecycle.GraphicsSetup();

            ImTK.Database.ImTKDatabase.RegisterImporter(typeof(ImTK.Database.Texture2D), new ImTK.Silk.Importers.TextureImporter(s_gl));

            double lastTime = Hexa.NET.GLFW.GLFW.GetTime();

            while (Hexa.NET.GLFW.GLFW.WindowShouldClose(s_window) == 0)
            {
                using (ImTKProfiler.Scope("Unknown"))
                {
                    using (ImTKProfiler.Scope("System/GLFW Event"))
                    {
                        Hexa.NET.GLFW.GLFW.PollEvents();
                    }

                    double currentTime = Hexa.NET.GLFW.GLFW.GetTime();
                    double deltaTime = currentTime - lastTime;
                    lastTime = currentTime;

                    // Update
                    ImTKApplication.Lifecycle.LogicUpdate(deltaTime);

                    using (ImTKProfiler.Scope("System/Frame Process"))
                    {
                        // Resize handling
                        int width, height;
                        Hexa.NET.GLFW.GLFW.GetFramebufferSize(s_window, &width, &height);
                        s_gl.Viewport(0, 0, (uint)width, (uint)height);

                        // Render
                        s_gl.Clear((uint)ClearBufferMask.ColorBufferBit);

                        ImGuiImplOpenGL3.NewFrame();
                        ImGuiImplGLFW.NewFrame();
                        ImGui.NewFrame();
                    }

                    ImTKApplication.Lifecycle.GuiRender();
                    ImTKApplication.Lifecycle.GizmoRender();

                    using (ImTKProfiler.Scope("System/Frame Process"))
                    {
                        ImGui.Render();
                        ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

                        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
                        {
                            var backup_current_context = Hexa.NET.GLFW.GLFW.GetCurrentContext();
                            ImGui.UpdatePlatformWindows();
                            ImGui.RenderPlatformWindowsDefault();
                            Hexa.NET.GLFW.GLFW.MakeContextCurrent(backup_current_context);
                        }
                    }

                    ImTKApplication.Lifecycle.LateUpdate();

                    using (ImTKProfiler.Scope("System/Frame Process"))
                    {
                        Hexa.NET.GLFW.GLFW.SwapBuffers(s_window);
                    }
                }
            }

            ImTKApplication.Lifecycle.Close();

            ImGuiImplOpenGL3.Shutdown();
            ImGuiImplGLFW.Shutdown();
            ImGui.DestroyContext();

            Hexa.NET.GLFW.GLFW.DestroyWindow(s_window);
            Hexa.NET.GLFW.GLFW.Terminate();
        }

        private static void OnFontChanged(ImTK.Event.OnFontChangedEvent evt)
        {
            ImGuiImplOpenGL3.DestroyDeviceObjects();
            ImGuiImplOpenGL3.CreateDeviceObjects();
        }
    }
}
