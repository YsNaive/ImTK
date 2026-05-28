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
            ImTKEnvironment.CompanyName = "ImTK_Test";
            ImTKEnvironment.ApplicationName = "IntegrationTestApp";

            try
            {
                // Force Initialization (usually done by DatabaseModule)
                Resource.Initialize();
                ImTKDatabase.Initialize();

                Resource.RegisterImporter(typeof(MockReadOnlyAsset), typeof(MockReadOnlyAssetImporter));
                ImTKDatabase.RegisterImporter(typeof(MockSaveableAsset), typeof(MockSaveableAssetImporter));
                ImTKDatabase.RegisterExporter(typeof(MockSaveableAsset), typeof(MockSaveableAssetExporter));

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
            Directory.CreateDirectory(ImTKEnvironment.GlobalAssetPath);
            File.WriteAllText(testFile, "Global Data");

            var asset = Resource.Load<MockReadOnlyAsset>("test_resource.txt");
            ImTKAssert.AreEqual("Global Data", asset.Content, "Resource failed to load global data.");

            File.Delete(testFile);
        }

        private void TestDatabaseAPI()
        {
            var asset = ImTKDatabase.Load<MockSaveableAsset>("prefs.json");
            asset.Content = "User Prefs";

            asset.IsDirty = true;

            ImTKDatabase.SaveAssets();

            string fullPath = Path.Combine(ImTKEnvironment.LocalDataPath, "prefs.json");
            ImTKAssert.AreEqual("User Prefs", File.ReadAllText(fullPath), "Database failed to save local data.");
        }

        private void TestUnloadAll()
        {
            var asset = ImTKDatabase.Load<MockSaveableAsset>("prefs2.json");
            ImTKAssert.IsFalse(asset.WasDisposed, "Asset should be active.");

            ImTKDatabase.UnloadAll();

            ImTKAssert.IsTrue(asset.WasDisposed, "Asset should be disposed after UnloadAll.");
        }
    }
}
