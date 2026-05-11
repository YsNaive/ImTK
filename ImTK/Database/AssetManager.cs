using System;
using System.Collections.Generic;
using System.IO;
using ImTK.Log;

namespace ImTK.Database
{
    /// <summary>
    /// 資源快取與實例化邏輯的內部核心管理器。
    /// 負責維護特定 RootPath 下的資源唯一性與生命週期。
    /// </summary>
    internal class AssetManager
    {
        private static readonly LogContext s_log = new LogContext("AssetManager");

        private readonly string m_rootPath;
        private readonly bool m_isReadOnly;
        private readonly Dictionary<string, IAsset> m_cache = new Dictionary<string, IAsset>();

        public AssetManager(string rootPath, bool isReadOnly)
        {
            m_rootPath = Path.GetFullPath(rootPath);
            m_isReadOnly = isReadOnly;

            if (!Directory.Exists(m_rootPath))
            {
                try
                {
                    Directory.CreateDirectory(m_rootPath);
                    s_log.Info($"Created root directory at {m_rootPath}");
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Failed to create root directory at {m_rootPath}");
                }
            }
        }

        private string NormalizePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new AssetPathInvalidException(relativePath, "Path cannot be null or empty.");
            }

            if (Path.IsPathRooted(relativePath))
            {
                throw new AssetPathInvalidException(relativePath, "Absolute paths are not allowed. Please use relative paths.");
            }

            string normalized = relativePath.Replace('\\', '/');
            if (normalized.StartsWith("./"))
            {
                normalized = normalized.Substring(2);
            }
            while (normalized.StartsWith("/"))
            {
                normalized = normalized.Substring(1);
            }

            return normalized;
        }

        private string GetAbsolutePath(string normalizedPath)
        {
            string absolutePath = Path.GetFullPath(Path.Combine(m_rootPath, normalizedPath));

            // 安全性檢查：防禦 Directory Traversal (目錄穿越) 漏洞 (例如傳入 "../../secret.txt")
            if (!absolutePath.StartsWith(m_rootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new AssetPathInvalidException(normalizedPath, "Path traversal is not allowed. Asset must be within the root directory.");
            }

            return absolutePath;
        }

        public T GetAsset<T>(string relativePath) where T : class, IAsset, new()
        {
            string normalizedPath = NormalizePath(relativePath);

            if (m_cache.TryGetValue(normalizedPath, out IAsset cachedAsset))
            {
                if (cachedAsset is T typedAsset)
                {
                    return typedAsset;
                }
                throw new AssetTypeMismatchException(normalizedPath, typeof(T), cachedAsset.GetType());
            }

            string absolutePath = GetAbsolutePath(normalizedPath);
            if (!File.Exists(absolutePath))
            {
                throw new AssetNotFoundException(normalizedPath);
            }

            T newAsset = new T();
            if (newAsset is ImTKAsset impl)
            {
                impl.Path = normalizedPath;
                impl.Version = 1;
                impl.IsDisposed = false;

                try
                {
                    impl.OnLoad(absolutePath);
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Failed to execute OnLoad for asset {normalizedPath}");
                    throw;
                }
            }

            m_cache[normalizedPath] = newAsset;
            s_log.Trace($"Loaded and cached asset: {normalizedPath} as {typeof(T).Name}");
            return newAsset;
        }

        public T CreateAsset<T>(string relativePath) where T : class, ISaveableAsset, new()
        {
            if (m_isReadOnly)
            {
                throw new NotSupportedException($"Cannot create asset '{relativePath}' because this AssetManager is marked as read-only.");
            }

            string normalizedPath = NormalizePath(relativePath);
            string absolutePath = GetAbsolutePath(normalizedPath);

            if (m_cache.ContainsKey(normalizedPath) || File.Exists(absolutePath))
            {
                throw new AssetAlreadyExistsException(normalizedPath);
            }

            T newAsset = new T();
            if (newAsset is ImTKSaveableAsset impl)
            {
                impl.Path = normalizedPath;
                impl.Version = 1;
                impl.IsDisposed = false;
                impl.IsDirty = true;

                // 確保目錄存在
                string dir = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                try
                {
                    impl.OnSave(absolutePath);
                    impl.IsDirty = false;
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Failed to execute OnSave for newly created asset {normalizedPath}");
                    throw;
                }
            }

            m_cache[normalizedPath] = newAsset;
            s_log.Trace($"Created and cached new asset: {normalizedPath} as {typeof(T).Name}");
            return newAsset;
        }

        public T GetOrCreateAsset<T>(string relativePath) where T : class, ISaveableAsset, new()
        {
            if (m_isReadOnly)
            {
                throw new NotSupportedException($"Cannot use GetOrCreateAsset for '{relativePath}' because this AssetManager is marked as read-only.");
            }

            string normalizedPath = NormalizePath(relativePath);
            string absolutePath = GetAbsolutePath(normalizedPath);

            if (m_cache.ContainsKey(normalizedPath) || File.Exists(absolutePath))
            {
                try
                {
                    return GetAsset<T>(relativePath); // 會回傳已實作 GetAsset 中針對 IAsset 的 new() 但是為了滿足 ISaveableAsset 的型別我們在定義介面時需要確保能被轉型，而在 ImTK 中 GetAsset 要求的泛型只是 IAsset 這是合法的
                }
                catch(AssetNotFoundException)
                {
                    // 若在 GetAsset 中發生檔案被刪除等極端競爭狀況，會在此被捕捉
                }
            }

            return CreateAsset<T>(relativePath);
        }

        public void MarkDirty(ISaveableAsset asset)
        {
            if (m_isReadOnly)
            {
                s_log.Warning($"Attempted to mark asset '{asset.Path}' as dirty in a read-only AssetManager.");
                return;
            }

            if (asset is ImTKSaveableAsset impl && !impl.IsDisposed)
            {
                impl.MarkDirty();
            }
        }

        public void SaveAssets()
        {
            if (m_isReadOnly) return;

            int savedCount = 0;
            foreach (var kvp in m_cache)
            {
                if (kvp.Value is ImTKSaveableAsset impl && !impl.IsDisposed && impl.IsDirty)
                {
                    string absolutePath = GetAbsolutePath(impl.Path);

                    try
                    {
                        impl.OnSave(absolutePath);
                        impl.IsDirty = false;
                        savedCount++;
                        s_log.Trace($"Saved asset: {impl.Path}");
                    }
                    catch (Exception ex)
                    {
                        s_log.Error(ex, $"Failed to save asset {impl.Path}");
                    }
                }
            }

            if (savedCount > 0)
            {
                s_log.Debug($"Successfully saved {savedCount} dirty assets.");
            }
        }

        public void Unload(string relativePath)
        {
            string normalizedPath = NormalizePath(relativePath);

            if (m_cache.TryGetValue(normalizedPath, out IAsset asset))
            {
                if (asset is ImTKAsset impl)
                {
                    impl.InternalDispose();
                }
                else
                {
                    asset.Dispose();
                }

                m_cache.Remove(normalizedPath);
                s_log.Trace($"Unloaded asset: {normalizedPath}");
            }
        }

        public void UnloadAll()
        {
            s_log.Debug($"Unloading all {m_cache.Count} cached assets in {m_rootPath}...");

            // 先嘗試將所有還沒存檔的寫回
            if (!m_isReadOnly)
            {
                SaveAssets();
            }

            foreach (var kvp in m_cache)
            {
                if (kvp.Value is ImTKAsset impl)
                {
                    impl.InternalDispose();
                }
                else
                {
                    kvp.Value.Dispose();
                }
            }

            m_cache.Clear();
        }
    }
}
