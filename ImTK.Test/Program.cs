using System;
using ImTK.Core;

namespace ImTK.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting ImTK Logic Test (Headless)...");

            // Since GLFW fails to init in this sandbox (no display),
            // we will just manually drive the Lifecycle to verify reflection and state machine.

            try
            {
                ImTKApplication.Lifecycle.Initialize();
                Console.WriteLine("Initialization passed. Modules found: " + (ImTKApplication.CurrentState == ApplicationState.AwaitingGraphicsSetup));

                ImTKApplication.Lifecycle.GraphicsSetup();

                ImTKApplication.Lifecycle.LogicUpdate(0.016);
                ImTKApplication.Lifecycle.GuiRender();
                ImTKApplication.Lifecycle.GizmoRender();
                ImTKApplication.Lifecycle.LateUpdate();

                Console.WriteLine("Main loop passed. Current State: " + ImTKApplication.CurrentState);

                ImTKApplication.Lifecycle.Close();
                Console.WriteLine("Shutdown passed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
    }
}
