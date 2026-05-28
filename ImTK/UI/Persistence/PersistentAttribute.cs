using System;

namespace ImTK.UI.Persistence
{
    /// <summary>
    /// 標記此變數應被自動序列化與持久化至 UI 狀態快取中。
    /// 前提是該元件必須設定過非空的 persistenceKey。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class PersistentAttribute : Attribute
    {
        /// <summary>
        /// 自訂在快取中的 Local Key。若為 null，系統將預設使用變數的名稱。
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 若標記在自定義類別/結構上，是否遞迴展開內部欄位？預設為 true。
        /// </summary>
        public bool Flatten { get; set; } = true;

        /// <summary>
        /// 在下拆過程中，是否強制將該型別內的所有 public 成員都視為 Persistent？
        /// 若為 false (預設)，則內部成員仍必須標記 [Persistent] 才會被處理。
        /// </summary>
        public bool IncludeAllMembers { get; set; } = false;

        public PersistentAttribute(string key = null)
        {
            Key = key;
        }
    }
}
