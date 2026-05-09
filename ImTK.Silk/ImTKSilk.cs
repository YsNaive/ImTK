using System;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.Silk
{
    /// <summary>
    /// The entry point and main loop driver for ImTK using Silk.NET and OpenGL.
    /// </summary>
    public static class ImTKSilk
    {
        private static IWindow s_window;
        private static GL s_gl;
        private static ImGuiController s_imguiController;
        private static ImTKSilkConstant s_config;

        /// <summary>
        /// Initializes the Silk.NET window, ImGui context, and starts the ImTKApplication lifecycle loop.
        /// This method blocks until the window is closed.
        /// </summary>
        public static void Run(ImTKSilkConstant config = null)
        {
            s_config = config ?? new ImTKSilkConstant();

            var options = WindowOptions.Default;
            options.Size = new global::Silk.NET.Maths.Vector2D<int>(s_config.windowWidth, s_config.windowHeight);
            options.Title = s_config.windowTitle;
            options.VSync = s_config.vsync;

            s_window = Window.Create(options);

            s_window.Load += OnLoad;
            s_window.Update += OnUpdate;
            s_window.Render += OnRender;
            s_window.Closing += OnClosing;

            // Start the internal ImTK module scanning (Phase 1 & 2)
            ImTKApplication.Lifecycle.Initialize();

            // Run the Silk.NET window blocking loop
            s_window.Run();
        }

        private static void OnLoad()
        {
            s_gl = s_window.CreateOpenGL();

            // Initialize ImGui Controller
            // Important: We must use the 'onConfigureIO' callback parameter to configure ViewportsEnable.
            // ImGuiController initializes fonts and might implicitly call NewFrame or build states early.
            // If we set ConfigFlags AFTER the constructor finishes, ImGui will throw an assertion failure:
            // "Please set ViewportsEnable before the first call to NewFrame()!"
            s_imguiController = new ImGuiController(
                s_gl,
                s_window,
                s_window.CreateInput(),
                () =>
                {
                    var io = ImGui.GetIO();
                    if (s_config.enableViewports)
                    {
                        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                    }
                }
            );

            // Phase 3: Setup graphics resources for ImTK modules
            ImTKApplication.Lifecycle.GraphicsSetup();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Only drive LogicUpdate in the update callback
            ImTKApplication.Lifecycle.LogicUpdate(deltaTime);
        }

        private static void OnRender(double deltaTime)
        {
            // Clear screen
            s_gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            // Start ImGui frame
            s_imguiController.Update((float)deltaTime);

            // Construct UI elements
            ImTKApplication.Lifecycle.GuiRender();

            // Render debug gizmos
            ImTKApplication.Lifecycle.GizmoRender();

            // Submit ImGui commands to OpenGL
            s_imguiController.Render();

            // Process end-of-frame updates and pending queues
            ImTKApplication.Lifecycle.LateUpdate();
        }

        private static void OnClosing()
        {
            // Teardown the ImTK application and modules
            ImTKApplication.Lifecycle.Close();

            // Dispose ImGui controller and window
            s_imguiController?.Dispose();
            s_window?.Dispose();
        }
    }
}
