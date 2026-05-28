#nullable enable

using System;
using System.IO;
using System.Text.Json;

namespace ImTK.Database.Importers
{
    /// <summary>
    /// 提供標準 JSON 格式的資源解析與匯出功能。
    /// 當呼叫 Import 時，若檔案不存在將直接拋出 FileNotFoundException (Strict 模式)。
    /// </summary>
    public class JsonAssetHandler<T> : IAssetImporter<T>, IAssetExporter<T> where T : ImTKAsset, new()
    {
        private readonly JsonSerializerOptions _options;

        public JsonAssetHandler(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public T Import(string absolutePath, string normalizedPath)
        {
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException($"JSON 資源檔不存在: {absolutePath}");
            }

            string jsonString = File.ReadAllText(absolutePath);
            var asset = JsonSerializer.Deserialize<T>(jsonString, _options);

            if (asset == null)
            {
                throw new InvalidDataException($"無法將 {absolutePath} 轉換為型別 {typeof(T).Name}");
            }

            asset.Path = normalizedPath;
            asset.IsDirty = false;
            
            return asset;
        }

        public void Export(T asset, string absolutePath)
        {
            string dir = Path.GetDirectoryName(absolutePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string jsonString = JsonSerializer.Serialize(asset, _options);
            File.WriteAllText(absolutePath, jsonString);
        }
    }

    /// <summary>
    /// 提供帶有 Fallback (找不到檔案時自動建立預設值) 的 JSON 資源解析功能。
    /// 通常用於設定檔等必定需要的資源。
    /// </summary>
    public class FallbackJsonAssetHandler<T> : IAssetImporter<T>, IAssetExporter<T> where T : ImTKAsset, new()
    {
        private static readonly ImTK.Log.LogContext s_log = new ImTK.Log.LogContext("FallbackJsonAssetHandler");
        private readonly JsonAssetHandler<T> _underlyingHandler;

        public FallbackJsonAssetHandler(JsonSerializerOptions? options = null)
        {
            _underlyingHandler = new JsonAssetHandler<T>(options);
        }

        public T Import(string absolutePath, string normalizedPath)
        {
            if (!File.Exists(absolutePath))
            {
                return CreateFallback(normalizedPath);
            }

            try
            {
                return _underlyingHandler.Import(absolutePath, normalizedPath);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Failed to load JSON asset from {absolutePath}. File might be corrupted. Falling back to default values.");
                return CreateFallback(normalizedPath);
            }
        }

        private T CreateFallback(string normalizedPath)
        {
            // 如果檔案不存在或損毀，直接建立一個新的實例，並標記為 Dirty 以便之後自動存檔
            var newAsset = new T();
            newAsset.Path = normalizedPath;
            newAsset.Version = 1;
            newAsset.IsDirty = true; // 強制在下次 SaveAssets 時產生檔案
            return newAsset;
        }

        public void Export(T asset, string absolutePath)
        {
            _underlyingHandler.Export(asset, absolutePath);
        }
    }
}
