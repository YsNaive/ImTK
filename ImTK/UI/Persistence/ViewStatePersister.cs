using System;
using System.Collections.Generic;
using ImTK.Database;
using ImTK.Log;

namespace ImTK.UI.Persistence
{
    /// <summary>
    /// 全域 UI 狀態排程與持久化管理器。
    /// 負責與 ImTKDatabase 溝通，掃描渲染清單來進行狀態的讀寫。
    /// </summary>
    public static class ViewStatePersister
    {

        
        private const string CacheAssetPath = "imgui/imtk_cache.json";

        /// 掃描 RenderList，為所有未讀取狀態的元件載入狀態。
        /// </summary>
        internal static void LoadNewStates(string rootId, List<RenderOp> renderList)
        {
            if (renderList == null || renderList.Count == 0)
                return;

            ImTKCacheAsset cacheAsset;
            try
            {
                cacheAsset = ImTKDatabase.Load<ImTKCacheAsset>(CacheAssetPath);
            }
            catch (Exception e)
            {
                ImTKLog.Error(e, "Failed to load cache asset for reading.");
                return;
            }

            StateReader reader = new StateReader(cacheAsset, rootId);

            foreach (var op in renderList)
            {
                if (op.Type == RenderOpType.Begin)
                {
                    var element = op.Element;
                    if (!string.IsNullOrEmpty(element.persistenceKey) && !element.m_hasLoadedState)
                    {
                        try
                        {
                            element.OnReadState(reader);
                            element.m_hasLoadedState = true;
                        }
                        catch (Exception e)
                        {
                            ImTKLog.Error(e, $"Exception in OnReadState for element {element.GetType().Name} with key {element.persistenceKey}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 掃描所有傳入的渲染清單，收集狀態並標記 CacheAsset 為 Dirty。
        /// </summary>
        internal static void SaveAllStates(Dictionary<string, List<RenderOp>> rootsToSave)
        {
            ImTKCacheAsset cacheAsset;
            try
            {
                cacheAsset = ImTKDatabase.Load<ImTKCacheAsset>(CacheAssetPath);
            }
            catch (Exception e)
            {
                ImTKLog.Error(e, "Failed to load cache asset for writing.");
                return;
            }

            bool anyDirty = false;
            HashSet<string> keyCollisionCheck = new HashSet<string>();

            foreach (var kvp in rootsToSave)
            {
                string rootId = kvp.Key;
                List<RenderOp> renderList = kvp.Value;

                keyCollisionCheck.Clear();
                StateWriter writer = new StateWriter(cacheAsset, rootId);

                foreach (var op in renderList)
                {
                    if (op.Type == RenderOpType.Begin)
                    {
                        var element = op.Element;
                        if (!string.IsNullOrEmpty(element.persistenceKey))
                        {
                            if (!keyCollisionCheck.Add(element.persistenceKey))
                            {
                                ImTKLog.Error($"PersistenceKey Collision Detected! The key '{element.persistenceKey}' is duplicated within root '{rootId}'. UI state may be overwritten.");
                            }

                            try
                            {
                                element.OnWriteState(writer);
                            }
                            catch (Exception e)
                            {
                                ImTKLog.Error(e, $"Exception in OnWriteState for element {element.GetType().Name} with key {element.persistenceKey}");
                            }
                        }
                    }
                }

                if (writer.IsDirty)
                {
                    anyDirty = true;
                }
            }

            if (anyDirty)
            {
                cacheAsset.MarkDirty();
            }
        }
    }
}
