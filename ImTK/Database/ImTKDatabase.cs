using ImTK.Core;

namespace ImTK.Database
{
    /// <summary>
    /// 提供可讀寫的本地資料庫存取。
    /// 對應路徑為 ImTKEnvironment.LocalDataPath (應用程式資料資料夾，如 %AppData%)。
    /// </summary>
    public static class ImTKDatabase
    {
        private static AssetManager s_manager;

        /// <summary>
        /// 內部初始化。由 DatabaseModule 負責呼叫。
        /// </summary>
        internal static void Initialize()
        {
            if (s_manager == null)
            {
                s_manager = new AssetManager(ImTKEnvironment.LocalDataPath, false);
            }
        }

        /// <summary>
        /// 取得本地資源。若檔案不存在則拋出 AssetNotFoundException。
        /// </summary>
        public static T GetAsset<T>(string relativePath) where T : ImTKAsset, new()
        {
            if (s_manager == null) Initialize();
            return s_manager.GetAsset<T>(relativePath);
        }

        /// <summary>
        /// 建立並寫入一個全新的本地資源。若檔案或快取已存在則拋出 AssetAlreadyExistsException。
        /// </summary>
        public static T CreateAsset<T>(string relativePath) where T : ImTKSaveableAsset, new()
        {
            if (s_manager == null) Initialize();
            return s_manager.CreateAsset<T>(relativePath);
        }

        /// <summary>
        /// 安全地獲取本地資源。如果檔案存在則載入，否則建立並寫入新的預設資源檔案。
        /// </summary>
        public static T GetOrCreateAsset<T>(string relativePath) where T : ImTKSaveableAsset, new()
        {
            if (s_manager == null) Initialize();
            return s_manager.GetOrCreateAsset<T>(relativePath);
        }

        /// <summary>
        /// 將指定的資源標記為已修改 (Dirty)，並在下次呼叫 SaveAssets() 時統一寫回磁碟。
        /// </summary>
        public static void MarkDirty(ISaveableAsset asset)
        {
            if (s_manager == null) Initialize();
            s_manager.MarkDirty(asset);
        }

        /// <summary>
        /// 將所有標記為 Dirty 的本地資源一次性寫回磁碟。
        /// </summary>
        public static void SaveAssets()
        {
            if (s_manager == null) Initialize();
            s_manager.SaveAssets();
        }

        /// <summary>
        /// 清理所有本地資源的快取，並在清理前確保將尚未寫入的 Dirty 資源存檔。
        /// 由 DatabaseModule 負責在系統關閉時呼叫。
        /// </summary>
        internal static void UnloadAll()
        {
            s_manager?.UnloadAll();
            s_manager = null;
        }
    }
}
