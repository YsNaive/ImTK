using System;

namespace ImTK.Database
{
    /// <summary>
    /// 基礎資源的抽象類別。
    /// 開發者自訂的唯讀資源應繼承此類別，並實作 OnLoad 方法。
    /// </summary>
    public abstract class ImTKAsset : IAsset
    {
        public string Path { get; internal set; } = string.Empty;
        public int Version { get; internal set; } = 1;
        public bool IsDisposed { get; internal set; } = false;

        /// <summary>
        /// 當資源被資料庫初始化時呼叫。開發者需在此處實作檔案的反序列化或解碼邏輯。
        /// </summary>
        /// <param name="absolutePath">檔案的絕對路徑</param>
        protected internal abstract void OnLoad(string absolutePath);

        /// <summary>
        /// 當資源從資料庫快取被移除，或系統關閉時呼叫。
        /// 若資源包含非受控記憶體 (如 C++ 指標、GPU 紋理)，必須覆寫此方法釋放它們。
        /// </summary>
        public virtual void Dispose()
        {
            // 預設提供空的實作，讓純資料物件不必強制覆寫
            IsDisposed = true;
        }

        /// <summary>
        /// 提供給內部管理器的事件觸發點。
        /// </summary>
        internal virtual void InternalDispose()
        {
            if (IsDisposed) return;
            Dispose();
            IsDisposed = true;
        }
    }
}
