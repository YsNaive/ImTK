namespace ImTK.Test.Framework
{
    /// <summary>
    /// 表示一個不需要啟動 ImTKSilk 視窗主迴圈即可驗證的邏輯測試。
    /// 將於 UI 啟動前自動被掃描並執行。
    /// </summary>
    public interface IHeadlessTest
    {
        void Run();
    }
}
