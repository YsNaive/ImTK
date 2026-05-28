using System;
using System.IO;
using ImTK.Database;
using ImTK.Test.Database.Mocks;
using ImTK.Test.Framework;

namespace ImTK.Test.Database
{
    public class AssetManagerTests : IHeadlessTest
    {
        private string s_testRoot;

        public void Run()
        {
            s_testRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(s_testRoot);

            try
            {
                TestReadOnlyMode();
                TestReadWriteMode();
                TestTypeMismatch();
                TestPathTraversal();
            }
            finally
            {
                if (Directory.Exists(s_testRoot))
                {
                    Directory.Delete(s_testRoot, true);
                }
            }
        }

        private void TestReadOnlyMode()
        {
            var manager = new AssetManager(s_testRoot, true);
            manager.RegisterImporter(typeof(MockReadOnlyAsset), typeof(MockReadOnlyAssetImporter));

            // Load non-existent file, importer should provide default content
            var asset = manager.Load<MockReadOnlyAsset>("notfound.txt");
            ImTKAssert.AreEqual("Default Mock Content", asset.Content, "Importer should handle missing files gracefully.");
            ImTKAssert.IsTrue(asset.IsReadOnly, "Asset loaded from read-only manager should be read-only.");

            // Create a file manually
            File.WriteAllText(Path.Combine(s_testRoot, "readonly.txt"), "Real Content");

            // Load existing file
            var asset2 = manager.Load<MockReadOnlyAsset>("readonly.txt");
            ImTKAssert.AreEqual("Real Content", asset2.Content, "Should load content correctly.");

            // Test SaveAssets ignored for read-only
            manager.RegisterExporter(typeof(MockReadOnlyAsset), typeof(MockSaveableAssetExporter)); // fake exporter just to test
            asset.IsDirty = true;
            manager.SaveAssets(); // Should not crash, and should not write because of IsReadOnly
        }

        private void TestReadWriteMode()
        {
            var manager = new AssetManager(s_testRoot, false);
            manager.RegisterImporter(typeof(MockSaveableAsset), typeof(MockSaveableAssetImporter));
            manager.RegisterExporter(typeof(MockSaveableAsset), typeof(MockSaveableAssetExporter));

            // Test CreateAsset via Save
            var asset = manager.Load<MockSaveableAsset>("rw_new.txt");
            ImTKAssert.IsFalse(asset.IsReadOnly, "Should be writable.");
            
            asset.Content = "Initial Value From Test";
            asset.IsDirty = true;
            
            manager.SaveAssets();
            ImTKAssert.IsFalse(asset.IsDirty, "Asset should be clean after save.");
            ImTKAssert.IsTrue(File.Exists(Path.Combine(s_testRoot, "rw_new.txt")), "File should be created.");
            ImTKAssert.AreEqual("Initial Value From Test", File.ReadAllText(Path.Combine(s_testRoot, "rw_new.txt")), "File content mismatch.");

            // Test Get cached
            var asset2 = manager.Load<MockSaveableAsset>("rw_new.txt");
            ImTKAssert.IsTrue(ReferenceEquals(asset, asset2), "Should return cached instance.");
        }

        private void TestTypeMismatch()
        {
            var manager = new AssetManager(s_testRoot, false);
            manager.RegisterImporter(typeof(MockSaveableAsset), typeof(MockSaveableAssetImporter));
            manager.RegisterImporter(typeof(MockReadOnlyAsset), typeof(MockReadOnlyAssetImporter));

            manager.Load<MockSaveableAsset>("conflict.txt");
            ImTKAssert.Throws<AssetTypeMismatchException>(() => manager.Load<MockReadOnlyAsset>("conflict.txt"));
        }

        private void TestPathTraversal()
        {
            var manager = new AssetManager(s_testRoot, false);
            manager.RegisterImporter(typeof(MockReadOnlyAsset), typeof(MockReadOnlyAssetImporter));

            ImTKAssert.Throws<AssetPathInvalidException>(() => manager.Load<MockReadOnlyAsset>("/absolute.txt"));
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ImTKAssert.Throws<AssetPathInvalidException>(() => manager.Load<MockReadOnlyAsset>("C:\\absolute.txt"));
            }
            ImTKAssert.Throws<AssetPathInvalidException>(() => manager.Load<MockReadOnlyAsset>("../../escape.txt"));
        }
    }
}
