using System;
using ImTK.Silk;
using ImTK.Core;

namespace ImTK.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new ImTK.Log.LogContext("SampleProgram");
            log.Info("Starting ImTK Sample Application...");
            
            ImTKEnvironment.CompanyName     = "ImTK";
            ImTKEnvironment.ApplicationName = "ImTK.Sample";
            var config = new ImTKSilkConstant
            {
                windowTitle = "ImTK Overview & Sample",
                configFolderPath = ImTKEnvironment.LocalDataPath,
                windowWidth = 1280,
                windowHeight = 800
            };

            // SampleOverviewModule will automatically register and open the OverviewWindow
            ImTKSilk.Run(config);

            log.Info("ImTK Sample Closed gracefully.");
        }
    }
}
