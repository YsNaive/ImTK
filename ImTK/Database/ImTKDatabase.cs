using ImTK.Core;

namespace ImTK.Database
{
    /// <summary>
    /// 本地可讀寫資料庫 (Local Read-Write Database)。
    /// 負責存取使用者設定檔、存檔等會變動的資源。
    /// 資源載入後 IsReadOnly = false，可透過 MarkDirty() 與 SaveAssets() 寫回磁碟。
    /// </summary>
    public static class ImTKDatabase
    {
        internal static AssetManager Manager { get; private set; } = null!;

        internal static void Initialize()
        {
            // 使用 ImTKEnvironment 提供的最新 LocalDataPath (支援 DevelopmentLocalDataPath 覆寫)
            Manager = new AssetManager(ImTKEnvironment.LocalDataPath, isReadOnly: false);
        }

        /// <summary>
        /// 從本地資料庫中載入資源。
        /// </summary>
        /// <typeparam name="T">期望的資源型別。</typeparam>
        /// <param name="path">相對於 LocalDataPath 的相對路徑。</param>
        /// <returns>載入的資源物件。</returns>
        public static T Load<T>(string path) where T : IAsset
        {
            return Manager.Load<T>(path);
        }

        /// <summary>
        /// 註冊資源解析器 (Importer)。
        /// </summary>
        public static void RegisterImporter(System.Type assetType, object importerTypeOrInstance)
        {
            Manager.RegisterImporter(assetType, importerTypeOrInstance);
        }

        /// <summary>
        /// 註冊資源匯出器 (Exporter)。
        /// </summary>
        public static void RegisterExporter(System.Type assetType, object exporterTypeOrInstance)
        {
            Manager.RegisterExporter(assetType, exporterTypeOrInstance);
        }

        /// <summary>
        /// 將所有標記為 Dirty 的資源寫回磁碟。
        /// </summary>
        public static void SaveAssets()
        {
            Manager.SaveAssets();
        }

        internal static void UnloadAll()
        {
            Manager.UnloadAll();
        }
    }
}
