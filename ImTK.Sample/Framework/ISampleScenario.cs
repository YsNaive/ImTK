namespace ImTK.Sample.Framework
{
    /// <summary>
    /// 定義一個展示單元。
    /// 所有的展示案例都必須實作此介面，才能在總覽面板中自動註冊並顯示。
    /// </summary>
    public interface ISampleScenario
    {
        /// <summary>
        /// 範例名稱。
        /// </summary>
        string ScenarioName { get; }

        /// <summary>
        /// 範例的簡短描述。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 此範例對應的 Markdown 文件路徑 (相對於 ImTK.Sample 專案根目錄)。
        /// </summary>
        string DocumentationPath { get; }

        /// <summary>
        /// 範例所屬的分類 (Category)，用於在總覽面板中群組顯示。
        /// </summary>
        string Category { get; }

        /// <summary>
        /// 排序權重，數字越小越排在前面。
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 關聯的範例型別，用於在面板中顯示 "See Also" 快速跳轉。
        /// </summary>
        System.Collections.Generic.IEnumerable<System.Type> SeeAlso { get; }

        /// <summary>
        /// 開啟或觸發此範例的動作。
        /// </summary>
        void Open();
    }
}
