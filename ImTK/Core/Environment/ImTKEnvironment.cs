#nullable enable

using System;
using System.IO;
using System.Reflection;

namespace ImTK.Core
{
    /// <summary>
    /// 管理 ImTK 應用程式的全域環境變數與核心路徑。
    /// 採用延遲載入 (Lazy Evaluation) 與自動推斷機制。
    /// </summary>
    public static class ImTKEnvironment
    {
        private static string? _companyName;
        /// <summary>
        /// 應用程式公司/組織名稱 (用於 LocalDataPath 的資料夾階層)。
        /// 預設透過反射抓取 [AssemblyCompanyAttribute]。若為 null 則在路徑組合時忽略此層級。
        /// </summary>
        public static string? CompanyName
        {
            get
            {
                if (_companyName == null)
                {
                    _companyName = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
                }
                return _companyName;
            }
            set => _companyName = value;
        }

        private static string? _applicationName;
        /// <summary>
        /// 應用程式名稱 (用於 LocalDataPath 的資料夾階層)。
        /// 預設透過反射抓取 [AssemblyProductAttribute]，若無則降級為執行檔名稱。
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                if (_applicationName == null)
                {
                    _applicationName = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyProductAttribute>()?.Product 
                                       ?? AppDomain.CurrentDomain.FriendlyName;
                }
                return _applicationName;
            }
            set => _applicationName = value;
        }

        private static string? _version;
        /// <summary>
        /// 應用程式版本號。
        /// 預設透過反射抓取執行檔版本，保證不為 null。
        /// </summary>
        public static string Version
        {
            get
            {
                if (_version == null)
                {
                    _version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0.0";
                }
                return _version;
            }
            set => _version = value;
        }

        private static bool? _isDevelopment;
        /// <summary>
        /// 標記當前是否為開發環境。
        /// 預設讀取執行檔的 [AssemblyConfigurationAttribute]，若為 "Debug" 則為 true。
        /// </summary>
        public static bool IsDevelopment
        {
            get
            {
                if (!_isDevelopment.HasValue)
                {
                    var config = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
                    _isDevelopment = string.Equals(config, "Debug", StringComparison.OrdinalIgnoreCase);
                }
                return _isDevelopment.Value;
            }
            set => _isDevelopment = value;
        }

        private static string? _globalAssetPath;
        /// <summary>
        /// 取得或設定全域唯讀資源的根目錄路徑。
        /// 預設指向應用程式執行檔所在目錄 (BaseDirectory)。
        /// </summary>
        public static string GlobalAssetPath
        {
            get
            {
                if (_globalAssetPath == null)
                {
                    _globalAssetPath = AppDomain.CurrentDomain.BaseDirectory;
                }
                return _globalAssetPath;
            }
            set => _globalAssetPath = value;
        }

        private static string? _developmentLocalDataPath;
        /// <summary>
        /// 開發模式下，使用者設定檔的覆寫儲存路徑。
        /// 預設為 null，表示不覆寫 (Fallback 至 LocalDataPath)。
        /// </summary>
        public static string? DevelopmentLocalDataPath
        {
            get
            {
                return _developmentLocalDataPath ?? LocalDataPath;
            }
            set => _developmentLocalDataPath = value;
        }

        private static string? _localDataPath;
        /// <summary>
        /// 取得可讀寫本地資料庫 (Local Database) 的根目錄路徑。
        /// 若 IsDevelopment 為 true 且 DevelopmentLocalDataPath 有設定，則回傳該路徑。
        /// 否則預設指向作業系統的 ApplicationData 資料夾 (%AppData%)，並根據 CompanyName 與 ApplicationName 組合。
        /// </summary>
        public static string LocalDataPath
        {
            get
            {
                if (IsDevelopment && _developmentLocalDataPath != null)
                {
                    return _developmentLocalDataPath;
                }

                if (_localDataPath == null)
                {
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    if (string.IsNullOrWhiteSpace(CompanyName))
                    {
                        _localDataPath = Path.Combine(appData, ApplicationName);
                    }
                    else
                    {
                        _localDataPath = Path.Combine(appData, CompanyName, ApplicationName);
                    }
                }
                return _localDataPath;
            }
            set => _localDataPath = value;
        }

        private static int? _hashedStringCapacityWarningThreshold;
        /// <summary>
        /// 全域 HashedString 註冊表的防呆容量上限。
        /// 當註冊的唯一字串數量超過此數值時，會觸發 Error Log 警告可能有 Memory Leak (動態字串濫用)。
        /// 預設為 50000。
        /// </summary>
        public static int HashedStringCapacityWarningThreshold
        {
            get => _hashedStringCapacityWarningThreshold ?? 50000;
            set => _hashedStringCapacityWarningThreshold = value;
        }
    }
}
