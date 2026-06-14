using ImTK.Database.Importers;
using ImTK.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace ImTK.Database
{
    /// <summary>
    /// 管理資源的生命週期、快取、註冊表與 IO 發派。
    /// 可以實例化多個 Manager 綁定不同根目錄與唯讀屬性。
    /// </summary>
    internal class AssetManager
    {


        private readonly string _baseDirectory;
        private readonly bool _isReadOnly;

        private readonly ConcurrentDictionary<string, IAsset> _cache = new();
        
        // 註冊表
        private readonly Dictionary<Type, object> _importerRegistry = new();
        private readonly Dictionary<Type, object> _exporterRegistry = new();

        public AssetManager(string baseDirectory, bool isReadOnly)
        {
            _baseDirectory = Path.GetFullPath(baseDirectory);
            _isReadOnly = isReadOnly;
        }

        #region Registry API

        public void RegisterImporter(Type assetType, object importerTypeOrInstance)
        {
            _importerRegistry[assetType] = importerTypeOrInstance;
        }

        public void RegisterExporter(Type assetType, object exporterTypeOrInstance)
        {
            _exporterRegistry[assetType] = exporterTypeOrInstance;
        }

        private IAssetImporter<T> GetImporter<T>() where T : IAsset
        {
            Type targetType = typeof(T);
            
            // 1. 精確匹配
            if (_importerRegistry.TryGetValue(targetType, out var exactMatch))
            {
                return ResolveHandler<IAssetImporter<T>>(exactMatch, targetType);
            }

            // 2. 開放式泛型匹配 (例如 typeof(JsonAsset<>))
            if (targetType.IsGenericType)
            {
                Type genericDefinition = targetType.GetGenericTypeDefinition();
                if (_importerRegistry.TryGetValue(genericDefinition, out var genericMatch))
                {
                    return ResolveHandler<IAssetImporter<T>>(genericMatch, targetType);
                }
            }

            throw new AssetImporterNotFoundException(targetType);
        }

        private IAssetExporter<T> GetExporter<T>() where T : IAsset
        {
            Type targetType = typeof(T);

            if (_exporterRegistry.TryGetValue(targetType, out var exactMatch))
            {
                return ResolveHandler<IAssetExporter<T>>(exactMatch, targetType);
            }

            if (targetType.IsGenericType)
            {
                Type genericDefinition = targetType.GetGenericTypeDefinition();
                if (_exporterRegistry.TryGetValue(genericDefinition, out var genericMatch))
                {
                    return ResolveHandler<IAssetExporter<T>>(genericMatch, targetType);
                }
            }

            throw new AssetExporterNotFoundException(targetType);
        }

        private TInterface ResolveHandler<TInterface>(object registeredValue, Type targetType)
        {
            if (registeredValue is Type handlerType)
            {
                // 如果註冊的是開放式泛型 Type (例如 typeof(JsonAssetHandler<>))
                if (handlerType.IsGenericTypeDefinition)
                {
                    var specificType = handlerType.MakeGenericType(targetType.GetGenericArguments());
                    return (TInterface)Activator.CreateInstance(specificType)!;
                }
                
                // 普通 Type 實例化
                return (TInterface)Activator.CreateInstance(handlerType)!;
            }
            
            // 如果註冊的直接是實例
            return (TInterface)registeredValue;
        }

        #endregion

        #region Wrapper Cache

        private interface IAssetExporterWrapper { void Export(IAsset asset, string path); }
        private class AssetExporterWrapper<T> : IAssetExporterWrapper where T : IAsset
        {
            private readonly IAssetExporter<T> _exporter;
            public AssetExporterWrapper(IAssetExporter<T> exporter) { _exporter = exporter; }
            public void Export(IAsset asset, string path) => _exporter.Export((T)asset, path);
        }

        private readonly ConcurrentDictionary<Type, IAssetExporterWrapper> _exporterWrappers = new();

        private IAssetExporterWrapper GetExporterWrapper(Type assetType)
        {
            if (_exporterWrappers.TryGetValue(assetType, out var wrapper))
                return wrapper;

            var getExporterMethod = typeof(AssetManager)
                .GetMethod(nameof(GetExporter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(assetType);
            
            var exporter = getExporterMethod.Invoke(this, null);
            var wrapperType = typeof(AssetExporterWrapper<>).MakeGenericType(assetType);
            wrapper = (IAssetExporterWrapper)Activator.CreateInstance(wrapperType, exporter)!;
            
            _exporterWrappers.TryAdd(assetType, wrapper);
            return wrapper;
        }

        #endregion

        #region Core IO

        public T Load<T>(string relativePath) where T : IAsset
        {
            string normalizedPath = relativePath.Replace('\\', '/');

            // 快取命中
            if (_cache.TryGetValue(normalizedPath, out var cachedAsset))
            {
                if (cachedAsset is T typedAsset)
                {
                    return typedAsset;
                }
                throw new AssetTypeMismatchException(normalizedPath, typeof(T), cachedAsset.GetType());
            }

            // 驗證路徑安全
            string absolutePath = Path.GetFullPath(Path.Combine(_baseDirectory, normalizedPath));
            if (!absolutePath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new AssetPathInvalidException(normalizedPath);
            }

            // 尋找 Importer 並執行
            IAsset newAsset;
            try
            {
                var importer = GetImporter<T>();
                newAsset = importer.Import(absolutePath, normalizedPath);
            }
            catch (Exception ex)
            {
                ImTKLog.Error(ex, $"Exception occurred while loading asset from {normalizedPath}");
                throw;
            }

            // 注入狀態
            if (newAsset is ImTKAsset implAsset)
            {
                implAsset.IsReadOnly = _isReadOnly;
            }

            _cache.TryAdd(normalizedPath, newAsset);
            return (T)newAsset;
        }

        public void SaveAssets()
        {
            if (_isReadOnly) return; // 唯讀管理員不存檔

            foreach (var kvp in _cache)
            {
                var asset = kvp.Value;
                if (!asset.IsDirty || asset.IsReadOnly) continue;

                ExportAsset(asset, kvp.Key);
            }
        }

        private void ExportAsset(IAsset asset, string normalizedPath)
        {
            Type assetType = asset.GetType();
            string absolutePath = Path.Combine(_baseDirectory, normalizedPath);

            try
            {
                var wrapper = GetExporterWrapper(assetType);
                wrapper.Export(asset, absolutePath);

                if (asset is ImTKAsset implAsset)
                {
                    implAsset.IsDirty = false;
                }
            }
            catch (Exception ex)
            {
                ImTKLog.Error(ex, $"Failed to export asset to {normalizedPath}");
            }
        }

        public void UnloadAll()
        {
            foreach (var asset in _cache.Values)
            {
                if (asset is ImTKAsset implAsset)
                {
                    implAsset.InternalDispose();
                }
                else
                {
                    asset.Dispose();
                }
            }
            _cache.Clear();
        }

        public void Unload(string relativePath)
        {
            string normalizedPath = relativePath.Replace('\\', '/');
            if (_cache.TryRemove(normalizedPath, out var asset))
            {
                if (asset is ImTKAsset implAsset)
                {
                    implAsset.InternalDispose();
                }
                else
                {
                    asset.Dispose();
                }
            }
        }

        #endregion
    }
}
