using System;

namespace ImTK.Database
{
    /// <summary>
    /// 資源的基礎介面。所有在系統中被快取管理的資源都必須實作此介面。
    /// </summary>
    public interface IAsset : IDisposable
    {
        /// <summary>
        /// 資源在資料庫中對應的標準化相對路徑 (例如 "assets/icon.png")。
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 資源的狀態版本號。每次資源內容變更時應遞增。
        /// </summary>
        int Version { get; }

        /// <summary>
        /// 指示該資源是否已被手動卸載並清理。
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 指示此資源的資料已被修改，需要被加入存檔佇列寫回磁碟。
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 指示此資源是否為唯讀狀態。唯讀資源呼叫 MarkDirty 會拋出例外。
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 標記此資源已被修改。若資源為唯讀，則會拋出 InvalidOperationException。
        /// </summary>
        void MarkDirty();
    }
}
