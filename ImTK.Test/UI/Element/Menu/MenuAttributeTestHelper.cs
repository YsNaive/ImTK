using System;
using System.Diagnostics;
using System.IO;
using ImTK.Core;
using ImTK.Log;
using ImTK.UI;

namespace ImTK.Test.UI.Element.Menu
{
    /// <summary>
    /// 用於在 Integration Test UI 啟動時，手動驗證 MainMenuAttribute 與版面的輔助類別。
    /// </summary>
    public static class MenuAttributeTestHelper
    {

        [MainMenu("Test/Open Cache Directory", priority = 900)]
        private static void OpenCacheDirectory()
        {
            try
            {
                string path = ImTKEnvironment.LocalDataPath;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    ImTKLog.Info($"Created cache directory at {path}");
                }

                ImTKLog.Info($"Opening cache directory: {path}");

                // 跨平台開啟資料夾的簡單實作
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                ImTKLog.Error(ex, "Failed to open cache directory.");
            }
        }
    }
}
