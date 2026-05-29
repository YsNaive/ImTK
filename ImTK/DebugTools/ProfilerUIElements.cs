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
        public ProfilerPlotElement(ProfilerContext context)
        {
            m_context = context;
        }

        public override void OnRender()
        {
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
                    RenderEngine.TextBuffered($"Total Frame Time History ({m_context.RollingWindowSeconds}s)");
                    ImGui.PlotLines("##TimeChart", ref m_context.TotalFrameTimes[0], 3600, startIdx, (string)null, 0f, 33.3f, new Vector2(this.layoutRect.width, this.layoutRect.height - 25f));
                }
                else
                {
                    RenderEngine.TextBuffered($"Total GC Allocation History ({m_context.RollingWindowSeconds}s)");

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

        public ProfilerTreeElement(ProfilerContext context)
        {
            m_context = context;
        }

        public override void OnRender()
        {
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
                ImGui.TableSetupColumn($"Avg ({m_context.RollingWindowSeconds}s)", ImGuiTableColumnFlags.WidthFixed, 80);
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

                foreach (var child in ImTKProfiler.Root.Children.Values)
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
            
            bool isOpen = ImGui.TreeNodeEx($"##{node.Name}", flags);
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            {
                m_context.SelectedNode = node;
            }
            
            ImGui.SameLine();
            
            if (parentAvg > 0.001f)
            {
                float pct = (avgVal / parentAvg) * 100f;
                float strWidth = RenderEngine.CalcTextSizeBuffered($"{pct:F1}%").X;
                float offset = pctMaxWidth - strWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                RenderEngine.TextColoredBuffered(ImTKTheme.GlobalTheme.normalColor.subText, $"{pct:F1}%");
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
            RenderEngine.TextBuffered($"{node.Name}");
            
            ImGui.TableNextColumn();
            RenderEngine.TextBuffered($"{currentVal:F3}");
            
            ImGui.TableNextColumn();
            RenderEngine.TextBuffered($"{avgVal:F3}");
            
            if (isOpen && hasChildren)
            {
                var sortedChildren = node.Children.Values.OrderByDescending(c => m_context.Mode == ProfilerMode.Time ? c.GetAverageMs(framesToAvg) : c.GetAverageGcKb(framesToAvg));
                foreach (var child in sortedChildren)
                {
                    RenderProfilerNode(child, avgVal, framesToAvg, pctMaxWidth);
                }
                ImGui.TreePop();
            }
        }
    }

    public class ProfilerScopeInfoElement : VisualElement
    {
        private ProfilerContext m_context;
        public ProfilerScopeInfoElement(ProfilerContext context)
        {
            m_context = context;
        }

        private void CollectAllCallers(ImTKProfiler.ProfilerNode node, HashSet<string> resultSet)
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
            RenderEngine.TextBuffered($"Scope Information");

            if (m_context.SelectedNode == null)
            {
                RenderEngine.TextDisabledBuffered($"Select a node from the table to view details.");
                return;
            }

            RenderEngine.TextBuffered($"Selected Scope: {m_context.SelectedNode.Name}");
            
            ImGui.BeginChild("CallersList", new Vector2(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);
            
            var allCallers = new HashSet<string>();
            CollectAllCallers(m_context.SelectedNode, allCallers);
            
            if (allCallers.Count == 0)
            {
                RenderEngine.TextDisabledBuffered($"No callers recorded.");
            }
            else
            {
                foreach (var caller in allCallers)
                {
                    ImGui.Bullet();
                    RenderEngine.TextBuffered($"{caller}");
                }
            }
            ImGui.EndChild();
        }
    }
}
