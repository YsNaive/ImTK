namespace ImTK.Database.Importers
{
    /// <summary>
    /// 資源解析器的基底介面，供 AssetManager 註冊表使用。
    /// 開發者請實作泛型版本的 <see cref="IAssetImporter{T}"/>。
    /// </summary>
    public interface IAssetImporter { }

    /// <summary>
    /// 負責將實體檔案解析為記憶體中物件的資源解析器。
    /// </summary>
    public interface IAssetImporter<T> : IAssetImporter where T : IAsset
    {
        /// <summary>
        /// 從磁碟載入資源並實例化物件。
        /// </summary>
        /// <param name="absolutePath">檔案在磁碟上的絕對路徑 (用來實際讀檔)。</param>
        /// <param name="normalizedPath">檔案在資料庫中的相對路徑 (用來注入到 Asset.Path)。</param>
        /// <returns>解析完成的資源物件。</returns>
        T Import(string absolutePath, string normalizedPath);
    }
}
