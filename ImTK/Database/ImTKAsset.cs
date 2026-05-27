using System;

namespace ImTK.Database
{
    /// <summary>
    /// 基礎資源的抽象類別。
    /// 提供標準的狀態與唯讀保護管理。開發者自訂資源應繼承此類別作為純資料容器。
    /// </summary>
    public abstract class ImTKAsset : IAsset
    {
        public string Path { get; internal set; } = string.Empty;
        public int Version { get; internal set; } = 1;
        public bool IsDisposed { get; internal set; } = false;
        public bool IsDirty { get; internal set; } = false;
        public bool IsReadOnly { get; internal set; } = false;

        /// <summary>
        /// 當資源從資料庫快取被移除，或系統關閉時呼叫。
        /// 若資源包含非受控記憶體 (如 C++ 指標、GPU 紋理)，必須覆寫此方法釋放它們。
        /// </summary>
        public virtual void Dispose()
        {
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

        /// <summary>
        /// 標記此資源的狀態已被修改，將排入存檔佇列。
        /// 若資源被標記為唯讀 (IsReadOnly == true)，此操作將會拋出例外。
        /// </summary>
        public void MarkDirty()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException($"無法標記唯讀資源為 Dirty: {Path}");
            }

            Version++;
            IsDirty = true;
        }
    }
}
