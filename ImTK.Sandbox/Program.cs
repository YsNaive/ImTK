using System;
using ImTK.Silk;
using ImTK.Core;

namespace ImTK.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new ImTK.Log.LogContext("Sandbox");
            log.Info("Starting ImTK Sandbox Application...");

            ImTKEnvironment.CompanyName     = "ImTK";
            ImTKEnvironment.ApplicationName = "ImTK.Sandbox";
            var config = new ImTKSilkConstant
            {
                windowTitle = "ImTK Sandbox",
                windowWidth = 1600,
                windowHeight = 900
            };

            // Starts the ImTK loop
             ImTKSilk.Run(config);

            log.Info("ImTK Sandbox Closed gracefully.");
        }
    }
}
