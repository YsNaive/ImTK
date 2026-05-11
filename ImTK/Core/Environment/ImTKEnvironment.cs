using System;
using System.IO;

namespace ImTK.Core
{
    /// <summary>
    /// 管理 ImTK 應用程式的全域環境變數與核心路徑。
    /// </summary>
    public static class ImTKEnvironment
    {
        /// <summary>
        /// 應用程式組織名稱 (用於 LocalDataPath 的資料夾階層)。
        /// 預設為 "ImTK"。若設定為空字串，則在路徑組合時會忽略此層級。
        /// </summary>
        public static string OrganizationName { get; set; } = "ImTK";

        /// <summary>
        /// 應用程式名稱 (用於 LocalDataPath 的資料夾階層)。
        /// 預設為 "ImTKProject"。
        /// </summary>
        public static string ApplicationName { get; set; } = "ImTKProject";

        /// <summary>
        /// 標記當前是否為開發環境。
        /// </summary>
        public static bool IsDevelopment { get; set; } = false;

        /// <summary>
        /// 取得全域資源 (Global Resource) 的根目錄路徑。
        /// 預設指向應用程式執行檔所在目錄 (BaseDirectory)。此路徑應視為唯讀。
        /// </summary>
        public static string GlobalAssetPath => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 取得本地資料庫 (Local Database) 的根目錄路徑。
        /// 預設指向作業系統的 ApplicationData 資料夾 (%AppData%)，並根據 OrganizationName 與 ApplicationName 組合。
        /// </summary>
        public static string LocalDataPath
        {
            get
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                if (string.IsNullOrWhiteSpace(OrganizationName))
                {
                    return Path.Combine(appData, ApplicationName);
                }

                return Path.Combine(appData, OrganizationName, ApplicationName);
            }
        }
    }
}
