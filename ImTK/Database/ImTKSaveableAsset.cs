using System;

namespace ImTK.Database
{
    /// <summary>
    /// 支援寫回磁碟持久化的資源抽象類別。
    /// 開發者自訂的設定檔或專案資料應繼承此類別，並實作 OnLoad 與 OnSave。
    /// </summary>
    public abstract class ImTKSaveableAsset : ImTKAsset, ISaveableAsset
    {
        public bool IsDirty { get; internal set; } = false;

        /// <summary>
        /// 標記此資源的狀態已被修改。
        /// 此操作會遞增 Version，並將 IsDirty 設為 true 準備供 AssetManager 寫入磁碟。
        /// </summary>
        public void MarkDirty()
        {
            Version++;
            IsDirty = true;
        }

        /// <summary>
        /// 當資源被資料庫系統觸發寫回時呼叫。開發者需在此處實作資料序列化並寫入檔案的邏輯。
        /// </summary>
        /// <param name="absolutePath">檔案的絕對路徑</param>
        protected internal abstract void OnSave(string absolutePath);
    }
}
