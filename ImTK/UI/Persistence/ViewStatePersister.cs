using System;
using System.Collections.Generic;
using ImTK.Database;
using ImTK.Log;

namespace ImTK.UI.Persistence
{
    /// <summary>
    /// 全域 UI 狀態排程與持久化管理器。
    /// 負責與 ImTKDatabase 溝通，掃描 Window.m_renderList 來進行狀態的讀寫。
    /// </summary>
    public static class ViewStatePersister
    {
        private static readonly LogContext s_log = new LogContext("ViewStatePersister");
        
        private const string CacheAssetPath = "imgui/imtk_cache.json";

        /// <summary>
        /// 掃描 Window 的 RenderList，為所有未讀取狀態的元件載入狀態。
        /// 呼叫時機：Window.m_isRenderListDirty 導致重建後。
        /// </summary>
        public static void LoadWindowNewStates(Window window)
        {
            if (window.m_renderList == null || window.m_renderList.Count == 0)
                return;

            ImTKCacheAsset cacheAsset;
            try
            {
                cacheAsset = ImTKDatabase.Load<ImTKCacheAsset>(CacheAssetPath);
            }
            catch (Exception e)
            {
                s_log.Error(e, "Failed to load cache asset for reading.");
                return;
            }

            StateReader reader = new StateReader(cacheAsset, window.windowId);

            foreach (var op in window.m_renderList)
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
                            s_log.Error(e, $"Exception in OnReadState for element {element.GetType().Name} with key {element.persistenceKey}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 掃描所有傳入的 Window，收集狀態並標記 CacheAsset 為 Dirty。
        /// </summary>
        public static void SaveAllWindowStates(IEnumerable<Window> activeWindows)
        {
            ImTKCacheAsset cacheAsset;
            try
            {
                cacheAsset = ImTKDatabase.Load<ImTKCacheAsset>(CacheAssetPath);
            }
            catch (Exception e)
            {
                s_log.Error(e, "Failed to load cache asset for writing.");
                return;
            }

            bool anyDirty = false;
            HashSet<string> keyCollisionCheck = new HashSet<string>();

            foreach (var window in activeWindows)
            {
                if (window.m_renderList == null)
                    continue;

                keyCollisionCheck.Clear();
                StateWriter writer = new StateWriter(cacheAsset, window.windowId);

                foreach (var op in window.m_renderList)
                {
                    if (op.Type == RenderOpType.Begin)
                    {
                        var element = op.Element;
                        if (!string.IsNullOrEmpty(element.persistenceKey))
                        {
                            if (!keyCollisionCheck.Add(element.persistenceKey))
                            {
                                s_log.Error($"PersistenceKey Collision Detected! The key '{element.persistenceKey}' is duplicated within Window '{window.windowId}'. UI state may be overwritten.");
                            }

                            try
                            {
                                element.OnWriteState(writer);
                            }
                            catch (Exception e)
                            {
                                s_log.Error(e, $"Exception in OnWriteState for element {element.GetType().Name} with key {element.persistenceKey}");
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
