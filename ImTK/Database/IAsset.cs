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
        /// 資源的狀態版本號。每次資源內容變更時應遞增，供 UI 元件檢查是否需要重繪或更新綁定。
        /// </summary>
        int Version { get; }

        /// <summary>
        /// 指示該資源是否已被手動卸載並清理底層指標 (如 OpenGL ID)。
        /// 當此值為 true 時，UI 元件不應再取用其內容。
        /// </summary>
        bool IsDisposed { get; }
    }
}
