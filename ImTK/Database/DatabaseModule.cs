using ImTK.Core;
using ImTK.Database.Importers;

namespace ImTK.Database
{
    /// <summary>
    /// Database 子系統的生命週期綁定模組。
    /// 將由 ImTKApplication 透過反射自動實例化。
    /// </summary>
    public class DatabaseModule : ImTKModule
    {
        // 必須是非公開無參數建構子 (遵循 AGENTS.md 規範 1.6)
        private DatabaseModule()
        {
        }

        protected internal override void OnInitializeSelf()
        {
            // 1. 初始化 Manager
            Resource.Initialize();
            ImTKDatabase.Initialize();

            // 2. 註冊內建的解析器 (開放式泛型)
            var cacheHandler = new FallbackJsonAssetHandler<ImTKCacheAsset>();
            ImTKDatabase.RegisterImporter(typeof(ImTKCacheAsset), cacheHandler);
            ImTKDatabase.RegisterExporter(typeof(ImTKCacheAsset), cacheHandler);
        }

        protected internal override void OnClose()
        {
            // 系統關閉前強制存檔
            ImTKDatabase.SaveAssets();

            // 卸載所有快取並釋放資源
            ImTKDatabase.Manager.UnloadAll();
            Resource.Manager.UnloadAll();
        }
    }
}
