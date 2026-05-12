using System;
using System.IO;
using ImTK.Core;
using ImTK.Database;
using ImTK.Test.Database.Mocks;
using ImTK.Test.Framework;

namespace ImTK.Test.Database
{
    public class DatabaseIntegrationTests : IHeadlessTest
    {
        public void Run()
        {
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

        private void TestResourceAPI()
        {
            // Global should be read only
            string testFile = Path.Combine(ImTKEnvironment.GlobalAssetPath, "test_resource.txt");
            File.WriteAllText(testFile, "Global Data");

            var asset = Resource.GetAsset<MockReadOnlyAsset>("test_resource.txt");
            ImTKAssert.AreEqual("Global Data", asset.Content, "Resource failed to load global data.");

            File.Delete(testFile);
        }

        private void TestDatabaseAPI()
        {
            var asset = ImTKDatabase.CreateAsset<MockSaveableAsset>("prefs.json");
            asset.Content = "User Prefs";

            // User can call ImTKDatabase.MarkDirty OR asset.MarkDirty()
            asset.MarkDirty();

            ImTKDatabase.SaveAssets();

            string fullPath = Path.Combine(ImTKEnvironment.LocalDataPath, "prefs.json");
            ImTKAssert.AreEqual("User Prefs", File.ReadAllText(fullPath), "Database failed to save local data.");
        }

        private void TestUnloadAll()
        {
            var asset = ImTKDatabase.GetAsset<MockSaveableAsset>("prefs.json");
            ImTKAssert.IsFalse(asset.IsDisposed, "Asset should be active.");

            ImTKDatabase.UnloadAll();

            ImTKAssert.IsTrue(asset.IsDisposed, "Asset should be disposed after UnloadAll.");
        }
    }
}
