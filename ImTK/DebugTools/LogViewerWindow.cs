using Hexa.NET.ImGui;
using ImTK.Core;
using ImTK.Log;
using ImTK.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ImTK.DebugTools
{
    public class LogViewerWindow : Window, ILogSink
    {
        // 跨執行緒的暫存佇列
        private readonly ConcurrentQueue<LogEntry> m_incomingQueue = new();

        private static readonly byte[] s_filterHint = Encoding.UTF8.GetBytes("過濾訊息內容...");
        private static readonly IntPtr s_scrollingRegionId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("ScrollingRegion");

        // 主執行緒的快取
        private readonly List<LogEntry> m_allLogs = new();
        private readonly List<int> m_filteredIndices = new();

        // 過濾狀態
        private bool m_showTrace = false;
        private bool m_showDebug = true;
        private bool m_showInfo = true;
        private bool m_showWarning = true;
        private bool m_showError = true;
        private bool m_showFatal = true;
        
        private string m_filterText = string.Empty;
        
        private readonly Dictionary<string, bool> m_contextFilters = new();
        private bool m_isAnyContextFilterChanged = false;

        private bool m_autoScroll = true;
        private bool m_requestScrollToBottom = false;

        // 游離的元件
        private readonly LogEntryElement m_logEntryElement;
        private float m_lastDpiScale = 1.0f;
        private readonly List<string> m_tempKeys = new();

        // ILogSink 介面
        public bool enabled { get; set; } = true;

        public const string WindowId = "ImTK.LogViewerWindow";

        public LogViewerWindow() : base("偵錯: 日誌", WindowId)
        {
            m_logEntryElement = new LogEntryElement();
        }

        protected override void OnEnable()
        {
            ImTKLog.AddSink(this);
        }

        protected override void OnDisable()
        {
            ImTKLog.RemoveSink(this);
        }

        public void Emit(LogEntry entry)
        {
            m_incomingQueue.Enqueue(entry);
        }

        private bool IsLogPassingFilter(LogEntry entry)
        {
            // 1. Level
            switch (entry.Level)
            {
                case LogLevel.Trace: if (!m_showTrace) return false; break;
                case LogLevel.Debug: if (!m_showDebug) return false; break;
                case LogLevel.Info: if (!m_showInfo) return false; break;
                case LogLevel.Warning: if (!m_showWarning) return false; break;
                case LogLevel.Error: if (!m_showError) return false; break;
                case LogLevel.Fatal: if (!m_showFatal) return false; break;
            }

            // 2. Context
            if (m_contextFilters.TryGetValue(entry.ContextName, out bool contextEnabled))
            {
                if (!contextEnabled) return false;
            }

            // 3. Text
            if (!string.IsNullOrEmpty(m_filterText))
            {
                if (!entry.Message.Contains(m_filterText, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void RebuildFilter()
        {
            m_filteredIndices.Clear();
            for (int i = 0; i < m_allLogs.Count; i++)
            {
                if (IsLogPassingFilter(m_allLogs[i]))
                {
                    m_filteredIndices.Add(i);
                }
            }
            if (m_autoScroll) m_requestScrollToBottom = true;
        }

        public override void OnRender()
        {
            if (m_lastDpiScale != RenderEngine.Context.CurrentDpiScale)
            {
                m_lastDpiScale = RenderEngine.Context.CurrentDpiScale;
                m_logEntryElement.MarkStyleDirty();
            }

            // 1. 同步新進來的 Log
            bool hasNewLogs = false;
            using (ImTKProfiler.ScopeRelative("Sync Logs"))
            {
                while (m_incomingQueue.TryDequeue(out var entry))
                {
                    m_allLogs.Add(entry);
                    if (!m_contextFilters.ContainsKey(entry.ContextName))
                    {
                        m_contextFilters[entry.ContextName] = true;
                        m_isAnyContextFilterChanged = true;
                    }
                    
                    if (IsLogPassingFilter(entry))
                    {
                        m_filteredIndices.Add(m_allLogs.Count - 1);
                        hasNewLogs = true;
                    }
                }

                if (m_isAnyContextFilterChanged)
                {
                    // 若有新的 Context 加入，不需重建，因為預設是 true 且已經通過 filter 加進去過了
                    m_isAnyContextFilterChanged = false;
                }

                if (hasNewLogs && m_autoScroll)
                {
                    m_requestScrollToBottom = true;
                }
            }

            // 2. 控制面板 (Top Bar)
            using (ImTKProfiler.ScopeRelative("Top Bar"))
            {
                bool needsRebuild = false;

                if (ImGui.Button("Clear"))
                {
                    m_allLogs.Clear();
                    m_filteredIndices.Clear();
                    m_contextFilters.Clear();
                }
                
                ImGui.SameLine();
                ImGui.Checkbox("Auto-scroll", ref m_autoScroll);
                
                ImGui.SameLine();
                RenderEngine.TextBuffered("|");
                ImGui.SameLine();

                // Level Filters (Combo)
                ImGui.SetNextItemWidth(120f);
                if (ImGui.BeginCombo("Level", "過濾層級..."))
                {
                    if (ImGui.Button("全選 (Select All)"))
                    {
                        m_showTrace = m_showDebug = m_showInfo = m_showWarning = m_showError = m_showFatal = true;
                        needsRebuild = true;
                    }
                    if (ImGui.Button("取消全選 (Deselect All)"))
                    {
                        m_showTrace = m_showDebug = m_showInfo = m_showWarning = m_showError = m_showFatal = false;
                        needsRebuild = true;
                    }
                    ImGui.Separator();

                    needsRebuild |= ImGui.Checkbox("Trace", ref m_showTrace);
                    needsRebuild |= ImGui.Checkbox("Debug", ref m_showDebug);
                    needsRebuild |= ImGui.Checkbox("Info", ref m_showInfo);
                    needsRebuild |= ImGui.Checkbox("Warning", ref m_showWarning);
                    needsRebuild |= ImGui.Checkbox("Error", ref m_showError);
                    needsRebuild |= ImGui.Checkbox("Fatal", ref m_showFatal);

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                RenderEngine.TextBuffered("|");
                ImGui.SameLine();

                // Context Filters (Combo)
                ImGui.SetNextItemWidth(150f);
                if (ImGui.BeginCombo("Context", "過濾模組..."))
                {
                    if (ImGui.Button("全選 (Select All)"))
                    {
                        var allKeys = new List<string>(m_contextFilters.Keys);
                        foreach (var key in allKeys) m_contextFilters[key] = true;
                        needsRebuild = true;
                    }
                    if (ImGui.Button("取消全選 (Deselect All)"))
                    {
                        var allKeys = new List<string>(m_contextFilters.Keys);
                        foreach (var key in allKeys) m_contextFilters[key] = false;
                        needsRebuild = true;
                    }
                    ImGui.Separator();

                    // 使用暫存陣列或 tolist 避免迴圈內修改 dictionary 的例外
                    m_tempKeys.Clear();
                    foreach (var k in m_contextFilters.Keys) m_tempKeys.Add(k);
                    foreach (var key in m_tempKeys)
                    {
                        bool isChecked = m_contextFilters[key];
                        if (ImGui.Checkbox(key, ref isChecked))
                        {
                            m_contextFilters[key] = isChecked;
                            needsRebuild = true;
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.SameLine();

                // Text Filter
                ImGui.SetNextItemWidth(-float.Epsilon); // fill remaining width
                string tempFilterText = m_filterText;
                if (ImGui.InputTextWithHint("##Filter", s_filterHint, ref tempFilterText, 256))
                {
                    m_filterText = tempFilterText;
                    needsRebuild = true;
                }

                if (needsRebuild)
                {
                    RebuildFilter();
                }
            }

            ImGui.Separator();

            // 3. 虛擬化捲動區域
            using (ImTKProfiler.ScopeRelative("Scrolling Region"))
            {
                var scrollRegionSize = ImGui.GetContentRegionAvail();
                if (scrollRegionSize.X <= 0f || scrollRegionSize.Y <= 0f) return;
                unsafe
                {
                    ImGui.BeginChild((byte*)s_scrollingRegionId, scrollRegionSize, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);
                }

                unsafe
                {
                    ImGuiListClipper clipper = new ImGuiListClipper();
                    clipper.Begin(m_filteredIndices.Count);
                    while (clipper.Step())
                    {
                        for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                        {
                            var log = m_allLogs[m_filteredIndices[i]];
                            
                            // 餵入資料給游離元件
                            m_logEntryElement.SetData(log);
                            
                            RenderEngine.Render(m_logEntryElement);
                        }
                    }
                    clipper.End();
                }

                if (m_requestScrollToBottom)
                {
                    ImGui.SetScrollHereY(1.0f);
                    m_requestScrollToBottom = false;
                }

                ImGui.EndChild();
            }
        }
    }
}
