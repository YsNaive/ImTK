using System;
using System.IO;
using ImTK.Database;
using ImTK.Test.Database.Mocks;

namespace ImTK.Test.Database
{
    public static class AssetManagerTests
    {
        private static string s_testRoot;

        public static void RunTests()
        {
            Console.WriteLine("--- Running AssetManagerTests ---");

            s_testRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(s_testRoot);

            try
            {
                TestReadOnlyMode();
                TestReadWriteMode();
                TestTypeMismatch();
                TestPathTraversal();

                Console.WriteLine("AssetManagerTests: All passed.");
            }
            finally
            {
                if (Directory.Exists(s_testRoot))
                {
                    Directory.Delete(s_testRoot, true);
                }
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception($"Test failed: {message}");
        }

        private static void AssertThrows<T>(Action action, string message = "") where T : Exception
        {
            try
            {
                action();
                throw new Exception($"Expected exception {typeof(T).Name} was not thrown. {message}");
            }
            catch (T)
            {
                // Passed
            }
            catch (Exception ex)
            {
                throw new Exception($"Expected exception {typeof(T).Name} but got {ex.GetType().Name}. {message}");
            }
        }

        private static void TestReadOnlyMode()
        {
            var manager = new AssetManager(s_testRoot, true);

            // Test GetAsset file not found
            AssertThrows<AssetNotFoundException>(() => manager.GetAsset<MockReadOnlyAsset>("notfound.txt"));

            // Create a file manually
            File.WriteAllText(Path.Combine(s_testRoot, "readonly.txt"), "Real Content");

            // Test GetAsset
            var asset = manager.GetAsset<MockReadOnlyAsset>("readonly.txt");
            Assert(asset.Content == "Real Content", "Should load content correctly.");
            Assert(asset.Version == 1, "Initial version should be 1.");

            // Test CreateAsset rejected
            AssertThrows<NotSupportedException>(() => manager.CreateAsset<MockSaveableAsset>("new.txt"));
            AssertThrows<NotSupportedException>(() => manager.GetOrCreateAsset<MockSaveableAsset>("new.txt"));

            // Test MarkDirty / SaveAssets ignored
            var saveable = new MockSaveableAsset { Path = "fake.txt" };
            manager.MarkDirty(saveable);
            manager.SaveAssets(); // Should not crash
        }

        private static void TestReadWriteMode()
        {
            var manager = new AssetManager(s_testRoot, false);

            // Test CreateAsset
            var asset = manager.CreateAsset<MockSaveableAsset>("rw_new.txt");
            Assert(asset.IsDirty == false, "Asset should be clean after initial save.");
            Assert(File.Exists(Path.Combine(s_testRoot, "rw_new.txt")), "File should be created.");
            Assert(File.ReadAllText(Path.Combine(s_testRoot, "rw_new.txt")) == "Initial Value", "File content mismatch.");

            // Test CreateAsset already exists
            AssertThrows<AssetAlreadyExistsException>(() => manager.CreateAsset<MockSaveableAsset>("rw_new.txt"));

            // Test GetOrCreateAsset (Get)
            var asset2 = manager.GetOrCreateAsset<MockSaveableAsset>("rw_new.txt");
            Assert(ReferenceEquals(asset, asset2), "Should return cached instance.");

            // Test GetOrCreateAsset (Create)
            var asset3 = manager.GetOrCreateAsset<MockSaveableAsset>("rw_auto.txt");
            Assert(File.Exists(Path.Combine(s_testRoot, "rw_auto.txt")), "File should be created automatically.");

            // Test MarkDirty & SaveAssets
            asset3.Content = "Updated Value";
            manager.MarkDirty(asset3);
            Assert(asset3.IsDirty, "Should be marked as dirty.");
            Assert(asset3.Version == 2, "Version should increment.");

            manager.SaveAssets();
            Assert(!asset3.IsDirty, "Should be clean after save.");
            Assert(File.ReadAllText(Path.Combine(s_testRoot, "rw_auto.txt")) == "Updated Value", "Updated content mismatch.");
        }

        private static void TestTypeMismatch()
        {
            var manager = new AssetManager(s_testRoot, false);
            manager.CreateAsset<MockSaveableAsset>("conflict.txt");

            AssertThrows<AssetTypeMismatchException>(() => manager.GetAsset<MockReadOnlyAsset>("conflict.txt"));
        }

        private static void TestPathTraversal()
        {
            var manager = new AssetManager(s_testRoot, false);

            AssertThrows<AssetPathInvalidException>(() => manager.GetAsset<MockReadOnlyAsset>("/absolute.txt"));
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                AssertThrows<AssetPathInvalidException>(() => manager.GetAsset<MockReadOnlyAsset>("C:\\absolute.txt"));
            }
            AssertThrows<AssetPathInvalidException>(() => manager.GetAsset<MockReadOnlyAsset>("../../escape.txt"));
        }
    }
}
