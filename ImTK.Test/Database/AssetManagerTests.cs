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

            // Test GetAsset file not found
            ImTKAssert.Throws<AssetNotFoundException>(() => manager.GetAsset<MockReadOnlyAsset>("notfound.txt"));

            // Create a file manually
            File.WriteAllText(Path.Combine(s_testRoot, "readonly.txt"), "Real Content");

            // Test GetAsset
            var asset = manager.GetAsset<MockReadOnlyAsset>("readonly.txt");
            ImTKAssert.AreEqual("Real Content", asset.Content, "Should load content correctly.");
            ImTKAssert.AreEqual(1ul, (ulong)asset.Version, "Initial version should be 1.");

            // Test CreateAsset rejected
            ImTKAssert.Throws<NotSupportedException>(() => manager.CreateAsset<MockSaveableAsset>("new.txt"));
            ImTKAssert.Throws<NotSupportedException>(() => manager.GetOrCreateAsset<MockSaveableAsset>("new.txt"));

            // Test MarkDirty / SaveAssets ignored
            var saveable = new MockSaveableAsset { Path = "fake.txt" };
            manager.MarkDirty(saveable);
            manager.SaveAssets(); // Should not crash
        }

        private void TestReadWriteMode()
        {
            var manager = new AssetManager(s_testRoot, false);

            // Test CreateAsset
            var asset = manager.CreateAsset<MockSaveableAsset>("rw_new.txt");
            ImTKAssert.IsFalse(asset.IsDirty, "Asset should be clean after initial save.");
            ImTKAssert.IsTrue(File.Exists(Path.Combine(s_testRoot, "rw_new.txt")), "File should be created.");
            ImTKAssert.AreEqual("Initial Value", File.ReadAllText(Path.Combine(s_testRoot, "rw_new.txt")), "File content mismatch.");

            // Test CreateAsset already exists
            ImTKAssert.Throws<AssetAlreadyExistsException>(() => manager.CreateAsset<MockSaveableAsset>("rw_new.txt"));

            // Test GetOrCreateAsset (Get)
            var asset2 = manager.GetOrCreateAsset<MockSaveableAsset>("rw_new.txt");
            ImTKAssert.IsTrue(ReferenceEquals(asset, asset2), "Should return cached instance.");

            // Test GetOrCreateAsset (Create)
            var asset3 = manager.GetOrCreateAsset<MockSaveableAsset>("rw_auto.txt");
            ImTKAssert.IsTrue(File.Exists(Path.Combine(s_testRoot, "rw_auto.txt")), "File should be created automatically.");

            // Test MarkDirty & SaveAssets
            asset3.Content = "Updated Value";
            manager.MarkDirty(asset3);
            ImTKAssert.IsTrue(asset3.IsDirty, "Should be marked as dirty.");
            ImTKAssert.AreEqual(2ul, (ulong)asset3.Version, "Version should increment.");

            manager.SaveAssets();
            ImTKAssert.IsFalse(asset3.IsDirty, "Should be clean after save.");
            ImTKAssert.AreEqual("Updated Value", File.ReadAllText(Path.Combine(s_testRoot, "rw_auto.txt")), "Updated content mismatch.");
        }

        private void TestTypeMismatch()
        {
            var manager = new AssetManager(s_testRoot, false);
            manager.CreateAsset<MockSaveableAsset>("conflict.txt");

            ImTKAssert.Throws<AssetTypeMismatchException>(() => manager.GetAsset<MockReadOnlyAsset>("conflict.txt"));
        }

        private void TestPathTraversal()
        {
            var manager = new AssetManager(s_testRoot, false);

            ImTKAssert.Throws<AssetPathInvalidException>(() => manager.GetAsset<MockReadOnlyAsset>("/absolute.txt"));
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ImTKAssert.Throws<AssetPathInvalidException>(() => manager.GetAsset<MockReadOnlyAsset>("C:\\absolute.txt"));
            }
            ImTKAssert.Throws<AssetPathInvalidException>(() => manager.GetAsset<MockReadOnlyAsset>("../../escape.txt"));
        }
    }
}
