using ImTK.Core;

namespace ImTK.Database
{
    /// <summary>
    /// 資源系統生命週期綁定模組。負責初始化資料庫並在系統關閉時自動清理資源。
    /// </summary>
    internal class DatabaseModule : ImTKModule
    {
        private DatabaseModule() { }

        protected internal override void OnInitializeSelf()
        {
            Resource.Initialize();
            ImTKDatabase.Initialize();
        }

        protected internal override void OnLateUpdate()
        {
            // 可選的自動存檔機制：如果開發者標記了資源但不主動呼叫 SaveAssets()
            // 我們可以選擇在此處，或是在某個閒置時刻自動執行。
            // 為了不干擾開發者的主觀控制，我們預設不在每幀自動儲存，僅交由 OnClose 或手動儲存。
            // 但如果未來需要實作 "自動儲存草稿" 功能，可在此處實作計時器。
        }

        protected internal override void OnClose()
        {
            // 確保在系統關閉前將未儲存的資料寫入磁碟，並釋放記憶體與指標
            ImTKDatabase.SaveAssets(); // 安全保障

            ImTKDatabase.UnloadAll();
            Resource.UnloadAll();
        }
    }
}
