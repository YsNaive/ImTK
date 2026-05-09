using System;
using ImTK.Silk;

namespace ImTK.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting ImTK Integration Test...");

            var config = new ImTKSilkConstant
            {
                windowTitle = "ImTK Architecture Test",
                windowWidth = 1024,
                windowHeight = 768
            };

            // Drive the entire ImTK application using the Silk.NET entry point
            ImTKSilk.Run(config);

            Console.WriteLine("Application Closed gracefully.");
        }
    }
}
