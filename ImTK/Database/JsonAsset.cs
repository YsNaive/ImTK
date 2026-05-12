using System;
using System.IO;
using System.Text.Json;
using ImTK.Log;

namespace ImTK.Database
{
    /// <summary>
    /// 提供標準的 JSON 檔案持久化封裝資源。
    /// 開發者可以透過 ImTKDatabase.GetOrCreateAsset&lt;JsonAsset&lt;T&gt;&gt; 來快速載入或建立 JSON 設定檔。
    /// </summary>
    /// <typeparam name="T">要被序列化的純資料物件型別</typeparam>
    public class JsonAsset<T> : ImTKSaveableAsset where T : new()
    {
        private static readonly LogContext s_log = new LogContext("JsonAsset");

        /// <summary>
        /// JSON 檔案反序列化後對應的資料實體。
        /// 在修改此物件的內容後，請呼叫 <see cref="ImTKSaveableAsset.MarkDirty"/> 以確保資料被存檔。
        /// </summary>
        public T Data { get; set; } = new T();

        protected internal override void OnLoad(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(absolutePath);
                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip
                        };

                        T parsedData = JsonSerializer.Deserialize<T>(jsonString, options);
                        if (parsedData != null)
                        {
                            Data = parsedData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Failed to parse JSON file at {absolutePath}. Using default empty object.");
                    // 保持 Data 為 new T(); 不中斷程式執行
                }
            }
        }

        protected internal override void OnSave(string absolutePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // 增加人類可讀性
                };

                string jsonString = JsonSerializer.Serialize(Data, options);
                File.WriteAllText(absolutePath, jsonString);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Failed to save JSON file to {absolutePath}.");
                throw; // Save 階段的錯誤必須拋出，讓上層知道存檔失敗
            }
        }
    }
}
