using ImTK.Core;

namespace ImTK.Database
{
    /// <summary>
    /// 全域唯讀資源庫 (Global Read-Only Database)。
    /// 負責存取不會被修改的資源 (如預設圖示、樣式表)。
    /// 資源載入後強制為 IsReadOnly = true。
    /// </summary>
    public static class Resource
    {
        internal static AssetManager Manager { get; private set; } = null!;

        internal static void Initialize()
        {
            Manager = new AssetManager(ImTKEnvironment.GlobalAssetPath, isReadOnly: true);
        }

        /// <summary>
        /// 從全域資源庫中載入資源。
        /// </summary>
        /// <typeparam name="T">期望的資源型別。</typeparam>
        /// <param name="path">相對於 GlobalAssetPath 的相對路徑。</param>
        /// <returns>載入的資源物件。</returns>
        public static T Load<T>(string path) where T : IAsset
        {
            return Manager.Load<T>(path);
        }

        /// <summary>
        /// 註冊唯讀資源的解析器。
        /// </summary>
        public static void RegisterImporter(System.Type assetType, object importerTypeOrInstance)
        {
            Manager.RegisterImporter(assetType, importerTypeOrInstance);
        }
    }
}
