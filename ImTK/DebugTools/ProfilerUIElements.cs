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
            ImGui.SetCursorScreenPos(this.layoutRect.position);
            int rolling = m_context.RollingWindowSeconds;
            ImGui.SetNextItemWidth(200f);
            if (ImGui.SliderInt("滾動視窗長度 (s)", ref rolling, 1, 60))
            {
                m_context.RollingWindowSeconds = rolling;
            }
        }
    }

    public class ProfilerPlotElement : VisualElement
    {
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

                ImGui.SetCursorScreenPos(this.layoutRect.position);
                ImGui.BeginGroup();
                if (m_context.Mode == ProfilerMode.Time)
                {
                    RenderEngine.TextBuffered(m_cachedTimeTitle);
                    ImGui.PlotLines("##TimeChart", ref m_context.TotalFrameTimes[0], 3600, startIdx, (string)null, 0f, 33.3f, new Vector2(this.layoutRect.width, this.layoutRect.height - 25f));
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
                    ImGui.PlotLines("##GcChart", ref m_context.TotalFrameGcs[0], 3600, startIdx, (string)null, 0f, maxGc * 1.2f, new Vector2(this.layoutRect.width, this.layoutRect.height - 25f));
                }
                ImGui.EndGroup();
            }
        }
    }

    public class ProfilerTreeElement : VisualElement
    {
        private ProfilerContext m_context;
        private string m_cachedAvgColumnTitle;
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
            if (m_cachedAvgRolling != m_context.RollingWindowSeconds) {
                m_cachedAvgRolling = m_context.RollingWindowSeconds;
                m_cachedAvgColumnTitle = $"Avg ({m_cachedAvgRolling}s)";
            }
            int framesToAvg = m_context.RollingWindowSeconds * 60; 
            int count = Math.Min(m_context.FrameDataCount, framesToAvg);
            
            float pctMaxWidth = ImGui.CalcTextSize("000.0%").X;

            ImGui.Separator();
            ImGui.BeginChild("##table", layoutRect.size);

            if (ImGui.BeginTable("SubsystemsTreeTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY))
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn("System", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(m_context.Mode == ProfilerMode.Time ? "Current (ms)" : "Current (KB)", ImGuiTableColumnFlags.WidthFixed, 80);
                ImGui.TableSetupColumn(m_cachedAvgColumnTitle, ImGuiTableColumnFlags.WidthFixed, 80);
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

        private void RenderProfilerNode(ImTKProfiler.ProfilerNode node, float parentAvg, int framesToAvg, float pctMaxWidth)
        {
            if (node == null) return;
            
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            float currentVal = m_context.Mode == ProfilerMode.Time ? node.GetLatestMs() : node.GetLatestGcKb();
            float avgVal = m_context.Mode == ProfilerMode.Time ? node.GetAverageMs(framesToAvg) : node.GetAverageGcKb(framesToAvg);

            bool hasChildren = node.Children.Count > 0;
            
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.AllowOverlap;
            if (!hasChildren)
            {
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
            }
            if (m_context.SelectedNode == node)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }
            
            bool isOpen = ImGui.TreeNodeEx(node.CachedTreeNodeId ?? $"##{node.Name}", flags);
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
                float strWidth = ImGui.CalcTextSize("-").X;
                float offset = pctMaxWidth - strWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                ImGui.TextColored(ImTKTheme.GlobalTheme.normalColor.subText, "-");
            }
            
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5f);
            RenderEngine.TextBuffered(node.Name);
            
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
                
                if (m_context.Mode == ProfilerMode.Time)
                    childArray.AsSpan(0, idx).Sort(new NodeTimeComparer { framesToAvg = framesToAvg });
                else
                    childArray.AsSpan(0, idx).Sort(new NodeGcComparer { framesToAvg = framesToAvg });

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
        private ProfilerContext m_context;
        private System.Collections.Generic.HashSet<(string, int)> m_allCallers = new System.Collections.Generic.HashSet<(string, int)>();

        public ProfilerScopeInfoElement(ProfilerContext context)
        {
            m_context = context;
        }

        private void CollectAllCallers(ImTKProfiler.ProfilerNode node, System.Collections.Generic.HashSet<(string, int)> resultSet)
        {
            if (node == null) return;
            foreach (var caller in node.Callers)
            {
                resultSet.Add(caller.Key);
            }
            foreach (var child in node.Children.Values)
            {
                CollectAllCallers(child, resultSet);
            }
        }

        public override void OnRender()
        {
            ImGui.Separator();
            RenderEngine.TextBuffered("Scope Information");

            if (m_context.SelectedNode == null)
            {
                RenderEngine.TextDisabledBuffered("Select a node from the table to view details.");
                return;
            }

            var selHandler = new ImTKUtf8StringHandler(64, 1);
            selHandler.AppendLiteral("Selected Scope: ");
            selHandler.AppendFormatted(m_context.SelectedNode.Name);
            RenderEngine.TextBuffered(ref selHandler);
            
            ImGui.BeginChild("CallersList", new Vector2(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);
            
            m_allCallers.Clear();
            CollectAllCallers(m_context.SelectedNode, m_allCallers);
            
            if (m_allCallers.Count == 0)
            {
                RenderEngine.TextDisabledBuffered("No callers recorded.");
            }
            else
            {
                foreach (var caller in m_allCallers)
                {
                    string fullPath = caller.Item1;
                    int lastSlash = fullPath.LastIndexOf('\\');
                    if (lastSlash == -1) lastSlash = fullPath.LastIndexOf('/');
                    string fileName = lastSlash >= 0 ? fullPath.Substring(lastSlash + 1) : fullPath;
                    
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
