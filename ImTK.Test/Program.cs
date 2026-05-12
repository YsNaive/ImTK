using System;
using ImTK.Silk;

namespace ImTK.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting ImTK Integration Test...");

            // Run Unit / Logic Tests
            try
            {
                Database.EnvironmentTests.RunTests();
                Database.AssetManagerTests.RunTests();
                Database.DatabaseIntegrationTests.RunTests();
                Database.JsonAssetTests.RunTests();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] Test Failed: {ex.Message}");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\nAll headless tests passed. Launching UI...");

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
