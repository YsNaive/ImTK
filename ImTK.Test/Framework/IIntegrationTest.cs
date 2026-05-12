namespace ImTK.Test.Framework
{
    /// <summary>
    /// 表示一個需要依附於 ImTKSilk 生命週期內運作的整合測試。
    /// 將由 TestRunnerModule 統一管理執行。
    /// </summary>
    public interface IIntegrationTest
    {
        /// <summary>
        /// 測試的名稱。
        /// </summary>
        string TestName { get; }

        /// <summary>
        /// 標記此測試是否因為過度耗時或會影響全域狀態，而預設不自動執行。
        /// 設為 true 時，需透過 UI 面板手動觸發。
        /// </summary>
        bool IsManualOnly { get; }

        /// <summary>
        /// 執行測試。
        /// </summary>
        void Run();
    }
}
