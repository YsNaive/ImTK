using System;
using System.IO;
using ImTK.Database;

namespace ImTK.Test.Database
{
    public static class JsonAssetTests
    {
        private static string s_testRoot;

        // 一個簡單的測試資料類別
        public class DummyConfig
        {
            public int Width { get; set; } = 800;
            public int Height { get; set; } = 600;
            public string Title { get; set; } = "Default";
        }

        public static void RunTests()
        {
            Console.WriteLine("--- Running JsonAssetTests ---");

            s_testRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets_Json_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(s_testRoot);

            try
            {
                TestCreateAndSaveJson();
                TestLoadExistingJson();
                TestHandleMalformedJson();

                Console.WriteLine("JsonAssetTests: All passed.");
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

        private static void TestCreateAndSaveJson()
        {
            var manager = new AssetManager(s_testRoot, false);

            // 測試創建新的 JSON
            var asset = manager.CreateAsset<JsonAsset<DummyConfig>>("config.json");
            Assert(asset.Data != null, "Data should not be null after creation.");
            Assert(asset.Data.Width == 800, "Should have default values.");

            // 修改並存檔
            asset.Data.Width = 1920;
            asset.Data.Title = "Custom";
            manager.MarkDirty(asset);
            manager.SaveAssets();

            // 驗證檔案確實寫入且包含正確格式
            string filePath = Path.Combine(s_testRoot, "config.json");
            Assert(File.Exists(filePath), "JSON file should exist on disk.");
            string fileContent = File.ReadAllText(filePath);
            Assert(fileContent.Contains("\"Width\": 1920"), "JSON content should reflect changes.");
        }

        private static void TestLoadExistingJson()
        {
            // 在磁碟上手動準備一份 JSON 檔案
            string jsonString = "{ \"Width\": 1280, \"Height\": 720, \"Title\": \"Loaded\" }";
            File.WriteAllText(Path.Combine(s_testRoot, "load_test.json"), jsonString);

            var manager = new AssetManager(s_testRoot, false);
            var asset = manager.GetAsset<JsonAsset<DummyConfig>>("load_test.json");

            // 驗證資料是否正確反序列化
            Assert(asset.Data.Width == 1280, "Loaded width mismatch.");
            Assert(asset.Data.Title == "Loaded", "Loaded title mismatch.");
        }

        private static void TestHandleMalformedJson()
        {
            // 準備一份壞掉的 JSON 檔案
            string badJson = "{ \"Width\": 1280, oops this is not json }";
            File.WriteAllText(Path.Combine(s_testRoot, "bad.json"), badJson);

            var manager = new AssetManager(s_testRoot, false);

            // OnLoad 應該攔截例外並返回一個擁有預設值的 Data，不應直接 Crash
            var asset = manager.GetAsset<JsonAsset<DummyConfig>>("bad.json");

            Assert(asset.Data != null, "Fallback data should not be null.");
            Assert(asset.Data.Width == 800, "Should fallback to default values on parse error.");
        }
    }
}
