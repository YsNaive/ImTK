using Hexa.NET.ImGui;
using ImTK.Core;
using ImTK.Log;
using ImTK.UI;
using ImTK.UI.Persistence;
using System;
using System.Linq;
using System.Numerics;

namespace ImTK.DebugTools
{
    public enum ProfilerMode
    {
        Time,
        GC
    }

    public class ProfilerContext
    {
        public ProfilerMode Mode = ProfilerMode.Time;
        [Persistent]
        public int RollingWindowSeconds = 15;
        public ImTKProfiler.ProfilerNode SelectedNode;
        
        public float[] TotalFrameTimes = new float[3600];
        public float[] TotalFrameGcs = new float[3600];
        public int FrameDataIndex = 0;
        public int FrameDataCount = 0;
    }

    public class ProfilerSliderElement : VisualElement
    {
        [Persistent]
        private ProfilerContext m_context;
        public ProfilerSliderElement(ProfilerContext context)
        {
            m_context = context;
        }

        public override void OnRender()
        {
            using (ImTKProfiler.ScopeRelative("SliderElement"))
            {
                ImGui.SetCursorScreenPos(this.layoutRect.position);
                int rolling = m_context.RollingWindowSeconds;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.SliderInt("滾動視窗長度 (s)", ref rolling, 1, 60))
                {
                    m_context.RollingWindowSeconds = rolling;
                }
            }
        }
    }

    public class ProfilerPlotElement : VisualElement
    {
        private static readonly IntPtr s_timeChartId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("##TimeChart");
        private static readonly IntPtr s_gcChartId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("##GcChart");

        private ProfilerContext m_context;
        private string m_cachedTimeTitle;
        private string m_cachedGcTitle;
        private int m_cachedRolling = -1;

        public ProfilerPlotElement(ProfilerContext context)
        {
            m_context = context;
        }

        public override void OnRender()
        {
            using (ImTKProfiler.ScopeRelative("PlotElement"))
            {
                if (m_cachedRolling != m_context.RollingWindowSeconds) {
                    m_cachedRolling = m_context.RollingWindowSeconds;
                    m_cachedTimeTitle = $"Total Frame Time History ({m_cachedRolling}s)";
                    m_cachedGcTitle = $"Total GC Allocation History ({m_cachedRolling}s)";
                }
                int framesToAvg = m_context.RollingWindowSeconds * 60;
                int count = Math.Min(m_context.FrameDataCount, framesToAvg);
                if (count > 0)
                {
                    int startIdx = m_context.FrameDataIndex - count;
                    if (startIdx < 0) startIdx += 3600;

                    float plotWidth = this.layoutRect.width;
                    float plotHeight = this.layoutRect.height - 25f;
                    if (plotWidth <= 0f || plotHeight <= 0f) return;

                    ImGui.SetCursorScreenPos(this.layoutRect.position);
                    ImGui.BeginGroup();
                    unsafe
                    {
                        if (m_context.Mode == ProfilerMode.Time)
                        {
                            RenderEngine.TextBuffered(m_cachedTimeTitle);
                            ImGui.PlotLines((byte*)s_timeChartId, ref m_context.TotalFrameTimes[0], 3600, startIdx, (byte*)null, 0f, 33.3f, new Vector2(plotWidth, plotHeight));
                        }
                        else
                        {
                            RenderEngine.TextBuffered(m_cachedGcTitle);

                            float maxGc = 0.001f;
                            for (int i = 0; i < count; i++)
                            {
                                int idx = (startIdx + i) % 3600;
                                if (m_context.TotalFrameGcs[idx] > maxGc) maxGc = m_context.TotalFrameGcs[idx];
                            }
                            ImGui.PlotLines((byte*)s_gcChartId, ref m_context.TotalFrameGcs[0], 3600, startIdx, (byte*)null, 0f, maxGc * 1.2f, new Vector2(plotWidth, plotHeight));
                        }
                    }
                    ImGui.EndGroup();
                }
            }
        }
    }

    public class ProfilerTreeElement : VisualElement
    {
        private static readonly IntPtr s_tableId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("SubsystemsTreeTable");
        private static readonly IntPtr s_colSystem = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("System");
        private static readonly IntPtr s_colCurrentMs = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("Current (ms)");
        private static readonly IntPtr s_colCurrentKb = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("Current (KB)");
        private static readonly IntPtr s_childTableId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("##table");
        private static readonly IntPtr s_dashString = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("-");
        private static readonly IntPtr s_pctMaxString = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("000.0%");

        private ProfilerContext m_context;
        private string m_cachedAvgColumnTitle;
        private IntPtr m_cachedAvgColumnTitlePtr = IntPtr.Zero;
        private int m_cachedAvgRolling = -1;

        public ProfilerTreeElement(ProfilerContext context)
        {
            m_context = context;
        }

        private struct NodeTimeComparer : System.Collections.Generic.IComparer<ImTKProfiler.ProfilerNode>
        {
            public int framesToAvg;
            public int Compare(ImTKProfiler.ProfilerNode x, ImTKProfiler.ProfilerNode y) => y.GetAverageMs(framesToAvg).CompareTo(x.GetAverageMs(framesToAvg));
        }

        private struct NodeGcComparer : System.Collections.Generic.IComparer<ImTKProfiler.ProfilerNode>
        {
            public int framesToAvg;
            public int Compare(ImTKProfiler.ProfilerNode x, ImTKProfiler.ProfilerNode y) => y.GetAverageGcKb(framesToAvg).CompareTo(x.GetAverageGcKb(framesToAvg));
        }

        public override void OnRender()
        {
            using (ImTKProfiler.ScopeRelative("TreeElement"))
            {
                if (m_cachedAvgRolling != m_context.RollingWindowSeconds) {
                    m_cachedAvgRolling = m_context.RollingWindowSeconds;
                    m_cachedAvgColumnTitle = $"Avg ({m_cachedAvgRolling}s)";
                    if (m_cachedAvgColumnTitlePtr != IntPtr.Zero) System.Runtime.InteropServices.Marshal.FreeCoTaskMem(m_cachedAvgColumnTitlePtr);
                    m_cachedAvgColumnTitlePtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(m_cachedAvgColumnTitle);
                }
                int framesToAvg = m_context.RollingWindowSeconds * 60; 
                int count = Math.Min(m_context.FrameDataCount, framesToAvg);
                
                float pctMaxWidth;
                unsafe { pctMaxWidth = ImGui.CalcTextSize((byte*)s_pctMaxString).X; }

                ImGui.Separator();
                if (layoutRect.size.X <= 0f || layoutRect.size.Y <= 0f) return;
                
                unsafe
                {
                    ImGui.BeginChild((byte*)s_childTableId, layoutRect.size);

                    if (ImGui.BeginTable((byte*)s_tableId, 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableSetupColumn((byte*)s_colSystem, ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn((byte*)(m_context.Mode == ProfilerMode.Time ? s_colCurrentMs : s_colCurrentKb), ImGuiTableColumnFlags.WidthFixed, 80);
                        ImGui.TableSetupColumn((byte*)m_cachedAvgColumnTitlePtr, ImGuiTableColumnFlags.WidthFixed, 80);
                        ImGui.TableHeadersRow();

                    float avgTotalBaseline = 0f;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int idx = m_context.FrameDataIndex - 1 - i;
                            if (idx < 0) idx += 3600;
                            avgTotalBaseline += m_context.Mode == ProfilerMode.Time ? m_context.TotalFrameTimes[idx] : m_context.TotalFrameGcs[idx];
                        }
                        avgTotalBaseline /= count;
                    }

                    foreach (var child in ImTKProfiler.Root.ChildrenArray)
                    {
                        RenderProfilerNode(child, avgTotalBaseline, framesToAvg, pctMaxWidth);
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }
            }
        }

        private void RenderProfilerNode(ImTKProfiler.ProfilerNode node, float parentAvg, int framesToAvg, float pctMaxWidth)
        {
            if (node == null) return;
            
            float currentVal;
            float avgVal;
            bool hasChildren;
            ImGuiTreeNodeFlags flags;
            
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            currentVal = m_context.Mode == ProfilerMode.Time ? node.GetLatestMs() : node.GetLatestGcKb();
            avgVal = m_context.Mode == ProfilerMode.Time ? node.GetAverageMs(framesToAvg) : node.GetAverageGcKb(framesToAvg);

            hasChildren = node.Children.Count > 0;
            
            flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.AllowOverlap;
            if (!hasChildren)
            {
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
            }
            if (m_context.SelectedNode == node)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }
        
            bool isOpen;
            unsafe
            {
                if (node.NamePtr == IntPtr.Zero && !string.IsNullOrEmpty(node.Name))
                {
                    node.NamePtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(node.Name);
                }
                if (node.CachedTreeNodeId == IntPtr.Zero && !string.IsNullOrEmpty(node.Name))
                {
                    node.CachedTreeNodeId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8($"##{node.Name}");
                }

                if (node.CachedTreeNodeId != IntPtr.Zero)
                    isOpen = ImGui.TreeNodeEx((byte*)node.CachedTreeNodeId, flags);
                else
                    isOpen = ImGui.TreeNodeEx("##UNKNOWN", flags);
            }
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            {
                m_context.SelectedNode = node;
            }
                
            ImGui.SameLine();
            
            if (parentAvg > 0.001f)
            {
                float pct = (avgVal / parentAvg) * 100f;
                var pctHandler1 = new ImTKUtf8StringHandler(32, 1);
                pctHandler1.AppendFormatted(pct, "F1");
                pctHandler1.AppendLiteral("%");
                float strWidth = RenderEngine.CalcTextSizeBuffered(ref pctHandler1).X;
                float offset = pctMaxWidth - strWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                var pctHandler2 = new ImTKUtf8StringHandler(32, 1);
                pctHandler2.AppendFormatted(pct, "F1");
                pctHandler2.AppendLiteral("%");
                RenderEngine.TextColoredBuffered(ImTKTheme.GlobalTheme.normalColor.subText, ref pctHandler2);
            }
            else
            {
                float strWidth = 0f;
                unsafe { strWidth = ImGui.CalcTextSize((byte*)s_dashString).X; }
                float offset = pctMaxWidth - strWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                ImGui.PushStyleColor(ImGuiCol.Text, ImTKTheme.GlobalTheme.normalColor.subText.u32);
                unsafe { ImGui.TextUnformatted((byte*)s_dashString); }
                ImGui.PopStyleColor();
            }
        
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5f);
            unsafe { ImGui.TextUnformatted((byte*)node.NamePtr); }
            
            ImGui.TableNextColumn();
            var curHandler = new ImTKUtf8StringHandler(32, 1);
            curHandler.AppendFormatted(currentVal, "F3");
            RenderEngine.TextBuffered(ref curHandler);
            
            ImGui.TableNextColumn();
            var avgHandler = new ImTKUtf8StringHandler(32, 1);
            avgHandler.AppendFormatted(avgVal, "F3");
            RenderEngine.TextBuffered(ref avgHandler);
            
            if (isOpen && hasChildren)
            {
                int childCount = node.ChildrenArray.Length;
                var childArray = System.Buffers.ArrayPool<ImTKProfiler.ProfilerNode>.Shared.Rent(childCount);
                int idx = 0;
                foreach (var child in node.ChildrenArray)
                {
                    if (idx < childArray.Length) childArray[idx++] = child;
                }
                
                // Allocation-free insertion sort to avoid any IComparer boxing
                for (int i = 1; i < idx; i++)
                {
                    var key = childArray[i];
                    int j = i - 1;
                    if (m_context.Mode == ProfilerMode.Time)
                    {
                        while (j >= 0 && childArray[j].GetAverageMs(framesToAvg) < key.GetAverageMs(framesToAvg))
                        {
                            childArray[j + 1] = childArray[j];
                            j--;
                        }
                    }
                    else
                    {
                        while (j >= 0 && childArray[j].GetAverageGcKb(framesToAvg) < key.GetAverageGcKb(framesToAvg))
                        {
                            childArray[j + 1] = childArray[j];
                            j--;
                        }
                    }
                    childArray[j + 1] = key;
                }

                for (int i = 0; i < idx; i++)
                {
                    RenderProfilerNode(childArray[i], avgVal, framesToAvg, pctMaxWidth);
                }
                
                System.Buffers.ArrayPool<ImTKProfiler.ProfilerNode>.Shared.Return(childArray, clearArray: true);
                ImGui.TreePop();
            }
        }
    }

    public class ProfilerScopeInfoElement : VisualElement
    {
        private static readonly IntPtr s_callersListId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("CallersList");

        private ProfilerContext m_context;
        private System.Collections.Generic.HashSet<(string, int)> m_allCallers = new System.Collections.Generic.HashSet<(string, int)>();

        public ProfilerScopeInfoElement(ProfilerContext context)
        {
            m_context = context;
        }

        private void CollectAllCallers(ImTKProfiler.ProfilerNode node, System.Collections.Generic.HashSet<(string, int)> resultSet)
        {
            if (node == null) return;
            foreach (var caller in node.CallersArray)
            {
                resultSet.Add(caller);
            }
            foreach (var child in node.ChildrenArray)
            {
                CollectAllCallers(child, resultSet);
            }
        }

        private static readonly IntPtr s_scopeInfoTitle = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("Scope Information");
        private static readonly IntPtr s_selectNodeMsg = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("Select a node from the table to view details.");
        private static readonly IntPtr s_noCallersMsg = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("No callers recorded.");

        public override void OnRender()
        {
            using (ImTKProfiler.ScopeRelative("ScopeInfoElement"))
            {
                ImGui.Separator();
                unsafe { ImGui.TextUnformatted((byte*)s_scopeInfoTitle); }

                if (m_context.SelectedNode == null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                    unsafe { ImGui.TextUnformatted((byte*)s_selectNodeMsg); }
                    ImGui.PopStyleColor();
                    return;
                }

                var selHandler = new ImTKUtf8StringHandler(64, 1);
                selHandler.AppendLiteral("Selected Scope: ");
                selHandler.AppendFormatted(m_context.SelectedNode.Name);
                RenderEngine.TextBuffered(ref selHandler);
                
                var callersListSize = ImGui.GetContentRegionAvail();
                if (callersListSize.X <= 0f || callersListSize.Y <= 0f) return;
                unsafe
                {
                    ImGui.BeginChild((byte*)s_callersListId, callersListSize, ImGuiChildFlags.Borders, ImGuiWindowFlags.None);
                }
                
                m_allCallers.Clear();
                CollectAllCallers(m_context.SelectedNode, m_allCallers);
                
                if (m_allCallers.Count == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                    unsafe { ImGui.TextUnformatted((byte*)s_noCallersMsg); }
                    ImGui.PopStyleColor();
                }
                else
                {
                    foreach (var caller in m_allCallers)
                    {
                        ReadOnlySpan<char> fullPath = caller.Item1;
                        int lastSlash = fullPath.LastIndexOf('\\');
                        if (lastSlash == -1) lastSlash = fullPath.LastIndexOf('/');
                        ReadOnlySpan<char> fileName = lastSlash >= 0 ? fullPath.Slice(lastSlash + 1) : fullPath;
                        
                        ImGui.Bullet();
                        var callerHandler = new ImTKUtf8StringHandler(128, 2);
                        callerHandler.AppendFormatted(fileName);
                        callerHandler.AppendLiteral(":");
                        callerHandler.AppendFormatted(caller.Item2);
                        RenderEngine.TextBuffered(ref callerHandler);
                    }
                }
                ImGui.EndChild();
            }
        }
    }
}
