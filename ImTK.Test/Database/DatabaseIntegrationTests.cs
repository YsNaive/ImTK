using System;
using System.IO;
using ImTK.Core;
using ImTK.Database;
using ImTK.Test.Database.Mocks;

namespace ImTK.Test.Database
{
    public static class DatabaseIntegrationTests
    {
        public static void RunTests()
        {
            Console.WriteLine("--- Running DatabaseIntegrationTests ---");

            // Setup dummy environment
            ImTKEnvironment.OrganizationName = "ImTK_Test";
            ImTKEnvironment.ApplicationName = "IntegrationTestApp";

            try
            {
                // Force Initialization (usually done by DatabaseModule)
                Resource.Initialize();
                ImTKDatabase.Initialize();

                TestResourceAPI();
                TestDatabaseAPI();
                TestUnloadAll();

                Console.WriteLine("DatabaseIntegrationTests: All passed.");
            }
            finally
            {
                Resource.UnloadAll();
                ImTKDatabase.UnloadAll();

                // Cleanup local test dir
                if (Directory.Exists(ImTKEnvironment.LocalDataPath))
                {
                    Directory.Delete(ImTKEnvironment.LocalDataPath, true);
                }
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception($"Test failed: {message}");
        }

        private static void TestResourceAPI()
        {
            // Global should be read only
            string testFile = Path.Combine(ImTKEnvironment.GlobalAssetPath, "test_resource.txt");
            File.WriteAllText(testFile, "Global Data");

            var asset = Resource.GetAsset<MockReadOnlyAsset>("test_resource.txt");
            Assert(asset.Content == "Global Data", "Resource failed to load global data.");

            File.Delete(testFile);
        }

        private static void TestDatabaseAPI()
        {
            var asset = ImTKDatabase.CreateAsset<MockSaveableAsset>("prefs.json");
            asset.Content = "User Prefs";

            // User can call ImTKDatabase.MarkDirty OR asset.MarkDirty()
            asset.MarkDirty();

            ImTKDatabase.SaveAssets();

            string fullPath = Path.Combine(ImTKEnvironment.LocalDataPath, "prefs.json");
            Assert(File.ReadAllText(fullPath) == "User Prefs", "Database failed to save local data.");
        }

        private static void TestUnloadAll()
        {
            var asset = ImTKDatabase.GetAsset<MockSaveableAsset>("prefs.json");
            Assert(!asset.IsDisposed, "Asset should be active.");

            ImTKDatabase.UnloadAll();

            Assert(asset.IsDisposed, "Asset should be disposed after UnloadAll.");
        }
    }
}
