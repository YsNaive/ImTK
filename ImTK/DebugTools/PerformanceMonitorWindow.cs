using Hexa.NET.ImGui;
using ImTK.Core;
using ImTK.UI;
using ImTK.UI.Persistence;
using System;
using System.Linq;
using System.Numerics;

namespace ImTK.DebugTools
{
    public class PerformanceMonitorWindow : Window
    {
        public const string WindowId = "ImTK.PerformanceMonitorWindow";

        public int RollingWindowSeconds { get; set; } = 15;
        public ProfilerMode Mode { get; set; } = ProfilerMode.Time;

        [Persistent]
        private ProfilerContext m_context;
        private TextElement m_statsText;
        private Button m_btnTime;
        private Button m_btnGc;

        public PerformanceMonitorWindow() : base("偵錯: 效能監測", WindowId)
        {
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = AlignItems.Stretch;
            
            m_context = new ProfilerContext();
            m_context.RollingWindowSeconds = this.RollingWindowSeconds;
            m_context.Mode = this.Mode;
            
            // L1: Buttons
            var btnContainer = new VisualElement();
            btnContainer.style.flexDirection = FlexDirection.Row;
            m_btnTime = new Button { text = "效能 (Time)" };
            m_btnGc = new Button { text = "記憶體 (GC)" };
            btnContainer.Add(m_btnTime);
            btnContainer.Add(m_btnGc);
            
            m_btnTime.onClicked += (evt) => m_context.Mode = ProfilerMode.Time;
            m_btnGc.onClicked += (evt) => m_context.Mode = ProfilerMode.GC;

            // L2: Slider
            var slider = new ProfilerSliderElement(m_context);
            slider.style.height = 30f;

            // L3: Stats Text
            m_statsText = new TextElement();

            // L4: PlotLines
            var plot = new ProfilerPlotElement(m_context);
            plot.style.height = 100f; // 80f for plot + 20f for text

            // L5: Tree Table
            var tree = new ProfilerTreeElement(m_context);
            tree.style.flexGrow = 1;
            
            // L6: Info
            var info = new ProfilerScopeInfoElement(m_context);
            info.style.height = 150f;

            this.hierarchy.Add(btnContainer);
            this.hierarchy.Add(slider);
            this.hierarchy.Add(m_statsText);
            this.hierarchy.Add(plot);
            this.hierarchy.Add(tree);
            this.hierarchy.Add(info);
        }

        public override void OnRender()
        {
            // Sync persistent properties
            if (m_context.RollingWindowSeconds != this.RollingWindowSeconds)
                this.RollingWindowSeconds = m_context.RollingWindowSeconds;
            if (m_context.Mode != this.Mode)
                this.Mode = m_context.Mode;

            using (ImTKProfiler.ScopeRelative("B"))
            {
                // Update UI state
                m_btnTime.style.colorFamily = m_context.Mode == ProfilerMode.Time ? ThemeColorFamily.Info : ThemeColorFamily.Normal;
                m_btnGc.style.colorFamily = m_context.Mode == ProfilerMode.GC ? ThemeColorFamily.Info : ThemeColorFamily.Normal;

                // Record total frame metrics
                float currentFrameTime = (float)Time.UnscaledDeltaTime * 1000f;
                float currentGc = GC.GetAllocatedBytesForCurrentThread() / 1024f;

                float rootTime = ImTKProfiler.Root.GetLatestMs();
                float rootGc   = ImTKProfiler.Root.GetLatestGcKb();
                m_context.TotalFrameTimes[m_context.FrameDataIndex] = rootTime > currentFrameTime ? rootTime : currentFrameTime;
                m_context.TotalFrameGcs[m_context.FrameDataIndex] = rootGc;
            }

            float fps = ImGui.GetIO().Framerate;
            float gcMemoryMb = GC.GetTotalMemory(false) / 1048576f;

            float frameTimeDisplay = m_context.FrameDataCount > 0 ? m_context.TotalFrameTimes[m_context.FrameDataIndex == 0 ? 3599 : m_context.FrameDataIndex - 1] : 0f;
            float frameGcDisplay = m_context.FrameDataCount > 0 ? m_context.TotalFrameGcs[m_context.FrameDataIndex == 0 ? 3599 : m_context.FrameDataIndex - 1] : 0f;

            m_statsText.SetTextBuffered($"FPS: {fps:F1}   |   C# GC Memory: {gcMemoryMb:F2} MB   |   Frame Time: {frameTimeDisplay:F2} ms   |   Frame GC: {frameGcDisplay:F2} KB");

            m_context.FrameDataIndex = (m_context.FrameDataIndex + 1) % 3600;
            if (m_context.FrameDataCount < 3600) m_context.FrameDataCount++;
        }
    }
}
