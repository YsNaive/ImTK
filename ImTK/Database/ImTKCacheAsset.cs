using System.Collections.Generic;

namespace ImTK.Database
{
    /// <summary>
    /// 全域的 UI 狀態快取與持久化資料。
    /// 負責儲存所有 VisualElement 帶有 PersistenceKey 的狀態。
    /// </summary>
    public class ImTKCacheAsset : ImTKAsset
    {
        // 為了避免 Boxing / Unboxing 與額外的 GC 開銷，採用強型別分類的 Dictionary

        public Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Ints { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, string> Strings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, bool> Bools { get; set; } = new Dictionary<string, bool>();
    }
}
