using System;

namespace ImTK.Database
{
    /// <summary>
    /// 當系統中找不到指定類型的 AssetImporter 時拋出。
    /// 請確保已在 AssetManager 中正確呼叫 RegisterImporter。
    /// </summary>
    public class AssetImporterNotFoundException : Exception
    {
        public AssetImporterNotFoundException(Type assetType)
            : base($"找不到對應的 Importer。請確認已註冊可以處理型別 {assetType.Name} 的 Importer。") { }
    }

    /// <summary>
    /// 當系統中找不到指定類型的 AssetExporter，但該資源卻被標記為 Dirty 時拋出。
    /// </summary>
    public class AssetExporterNotFoundException : Exception
    {
        public AssetExporterNotFoundException(Type assetType)
            : base($"找不到對應的 Exporter。型別 {assetType.Name} 嘗試寫入磁碟，但沒有註冊能處理它的 Exporter。") { }
    }

    /// <summary>
    /// 當請求的資源絕對路徑不合法或發生目錄穿越攻擊 (Directory Traversal) 時拋出。
    /// </summary>
    public class AssetPathInvalidException : Exception
    {
        public AssetPathInvalidException(string path)
            : base($"資源路徑不合法或存在安全疑慮: {path}") { }
    }

    /// <summary>
    /// 當相同路徑的資源，其快取的型別與請求的型別不符時拋出。
    /// </summary>
    public class AssetTypeMismatchException : Exception
    {
        public AssetTypeMismatchException(string path, Type requested, Type actual)
            : base($"資源路徑 '{path}' 已快取為 {actual.Name}，但請求的型別為 {requested.Name}。") { }
    }
}
