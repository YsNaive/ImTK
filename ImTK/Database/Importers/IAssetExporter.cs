namespace ImTK.Database.Importers
{
    /// <summary>
    /// 資源匯出器的基底介面，供 AssetManager 註冊表使用。
    /// 開發者請實作泛型版本的 <see cref="IAssetExporter{T}"/>。
    /// </summary>
    public interface IAssetExporter { }

    /// <summary>
    /// 負責將記憶體中的物件序列化並寫回實體檔案的資源匯出器。
    /// </summary>
    public interface IAssetExporter<T> : IAssetExporter where T : IAsset
    {
        /// <summary>
        /// 將資源物件寫回磁碟。
        /// </summary>
        /// <param name="asset">欲寫入的資源物件。</param>
        /// <param name="absolutePath">檔案在磁碟上的絕對路徑。</param>
        void Export(T asset, string absolutePath);
    }
}
