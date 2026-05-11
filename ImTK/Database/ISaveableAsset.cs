namespace ImTK.Database
{
    /// <summary>
    /// 可被寫回磁碟持久化的資源介面。
    /// </summary>
    public interface ISaveableAsset : IAsset
    {
        /// <summary>
        /// 標記此資源已被修改，需要被加入存檔佇列寫入磁碟。
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// 指示此資源是否已被標記為需存檔 (Dirty 狀態)。
        /// </summary>
        bool IsDirty { get; }
    }
}
