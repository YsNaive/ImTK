using System;
using ImTK.Silk;
using ImTK.Core;
using ImTK.UI;

namespace ImTK.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new ImTK.Log.LogContext("TestProgram");
            log.Info("Starting ImTK Integration Test...");

            bool headlessPassed = Framework.HeadlessRunner.RunAllHeadlessTests();

            if (args.Length > 0 && args[0] == "--headless-only")
            {
                Environment.Exit(headlessPassed ? 0 : 1);
            }

            if (!headlessPassed)
            {
                log.Warning("Some Headless tests failed, but launching UI to show report...");
            }
            else
            {
                log.Info("All headless tests passed. Launching UI...");
            }
            
            ImTKEnvironment.CompanyName     = "ImTK";
            ImTKEnvironment.ApplicationName = "ImTK.Test";
            var config = new ImTKSilkConstant
            {
                windowTitle = "ImTK Integration Test",
                windowWidth = 1024,
                windowHeight = 768
            };

            ImTKFontManager.OverrideDefaultFamily(new FontSource("consola"));

            // Drive the entire ImTK application using the Silk.NET entry point
            ImTKSilk.Run(config);

            log.Info("Application Closed gracefully.");
        }
    }
}
