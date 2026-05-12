using ImTK.Core;

namespace ImTK.Database
{
    /// <summary>
    /// 提供唯讀全域資源的存取。
    /// 對應路徑為 ImTKEnvironment.GlobalAssetPath (應用程式執行檔所在目錄)。
    /// </summary>
    public static class Resource
    {
        private static AssetManager s_manager;

        /// <summary>
        /// 內部初始化。由 DatabaseModule 負責呼叫。
        /// </summary>
        internal static void Initialize()
        {
            if (s_manager == null)
            {
                s_manager = new AssetManager(ImTKEnvironment.GlobalAssetPath, true);
            }
        }

        /// <summary>
        /// 取得唯讀的全域資源。若檔案不存在則拋出 AssetNotFoundException。
        /// </summary>
        /// <typeparam name="T">繼承 ImTKAsset 的資源型別</typeparam>
        /// <param name="relativePath">資源的相對路徑</param>
        /// <returns>回傳快取中的資源實例</returns>
        public static T GetAsset<T>(string relativePath) where T : ImTKAsset, new()
        {
            if (s_manager == null) Initialize();
            return s_manager.GetAsset<T>(relativePath);
        }

        /// <summary>
        /// 清理所有唯讀資源的快取。由 DatabaseModule 負責在系統關閉時呼叫。
        /// </summary>
        internal static void UnloadAll()
        {
            s_manager?.UnloadAll();
            s_manager = null;
        }
    }
}
