using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ImTK;

namespace dashboard.Dashboard.Core
{
    public static class CacheHandler
    {
        public static string CacheFolderPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "gcvex_dashboard");

        private static string UsedGroupsFilePath => Path.Combine(CacheFolderPath, "used_groups.json");
        private static string ConnectionConfigFilePath => Path.Combine(CacheFolderPath, "connection_config.json");

        public class ConnectionConfig
        {
            public int Port { get; set; } = 7071;
            public int SocketId { get; set; } = -1;
        }

        public static void Initialize()
        {
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }

            // Redirect ImTK configuration path
            ImTK.WindowView.configFolderPath = CacheFolderPath;
        }

        [MainMenu("設定/開啟快取資料夾")]
        public static void OpenCacheFolder()
        {
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = CacheFolderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public static HashSet<string> LoadUsedGroups()
        {
            try
            {
                if (File.Exists(UsedGroupsFilePath))
                {
                    string json = File.ReadAllText(UsedGroupsFilePath);
                    var list = JsonSerializer.Deserialize<List<string>>(json);
                    if (list != null)
                    {
                        return new HashSet<string>(list);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CacheHandler] Failed to load used groups: {ex.Message}");
            }
            return new HashSet<string>();
        }

        public static void SaveUsedGroups(HashSet<string> usedGroups)
        {
            try
            {
                if (!Directory.Exists(CacheFolderPath))
                {
                    Directory.CreateDirectory(CacheFolderPath);
                }

                string json = JsonSerializer.Serialize(new List<string>(usedGroups));
                File.WriteAllText(UsedGroupsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CacheHandler] Failed to save used groups: {ex.Message}");
            }
        }

        public static ConnectionConfig LoadConnectionConfig()
        {
            try
            {
                if (File.Exists(ConnectionConfigFilePath))
                {
                    string json = File.ReadAllText(ConnectionConfigFilePath);
                    var config = JsonSerializer.Deserialize<ConnectionConfig>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CacheHandler] Failed to load connection config: {ex.Message}");
            }
            return new ConnectionConfig();
        }

        public static void SaveConnectionConfig(int port, int socketId)
        {
            try
            {
                if (!Directory.Exists(CacheFolderPath))
                {
                    Directory.CreateDirectory(CacheFolderPath);
                }

                var config = new ConnectionConfig { Port = port, SocketId = socketId };
                string json = JsonSerializer.Serialize(config);
                File.WriteAllText(ConnectionConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CacheHandler] Failed to save connection config: {ex.Message}");
            }
        }
    }
}