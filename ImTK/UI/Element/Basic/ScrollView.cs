using System;
using Hexa.NET.ImGui;
using System.Numerics;

namespace ImTK.UI
{
    public class ScrollViewFlags : ElementFlags<ImGuiWindowFlags>
    {
        public bool horizontalScrollbar { get => GetFlag(ImGuiWindowFlags.HorizontalScrollbar); set => SetFlag(ImGuiWindowFlags.HorizontalScrollbar, value); }
        public bool alwaysVerticalScrollbar { get => GetFlag(ImGuiWindowFlags.AlwaysVerticalScrollbar); set => SetFlag(ImGuiWindowFlags.AlwaysVerticalScrollbar, value); }
        public bool alwaysHorizontalScrollbar { get => GetFlag(ImGuiWindowFlags.AlwaysHorizontalScrollbar); set => SetFlag(ImGuiWindowFlags.AlwaysHorizontalScrollbar, value); }
        public bool noScrollbar { get => GetFlag(ImGuiWindowFlags.NoScrollbar); set => SetFlag(ImGuiWindowFlags.NoScrollbar, value); }
        public bool noScrollWithMouse { get => GetFlag(ImGuiWindowFlags.NoScrollWithMouse); set => SetFlag(ImGuiWindowFlags.NoScrollWithMouse, value); }
    }

    public class ScrollView : VisualElement
    {
        public ScrollViewFlags flags { get; } = new ScrollViewFlags();

        private static readonly IntPtr s_scrollViewId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8("ScrollView");

        private Vector2 m_contentSize;
        public override Vector2 contentSize => m_contentSize;
        private bool m_wasExpanded;
        private bool m_beginChildCalled;

        public ScrollView()
        {
            classList.Add("ScrollView");
        }

        protected override Vector2 MeasureContent(LayoutConstraint constraint)
        {
            bool horizScroll = flags.horizontalScrollbar || flags.alwaysHorizontalScrollbar;
            bool vertScroll = !flags.noScrollbar;

            // Set constraints to infinity where scrolling is allowed
            var childConstraint = new LayoutConstraint(
                horizScroll ? float.PositiveInfinity : constraint.AvailableWidth,
                vertScroll ? float.PositiveInfinity : constraint.AvailableHeight,
                horizScroll ? MeasureMode.Undefined : constraint.WidthMode,
                vertScroll ? MeasureMode.Undefined : constraint.HeightMode
            );

            m_contentSize = base.MeasureContent(childConstraint);

            // Our own size should be bounded by the parent constraint (we don't expand infinitely ourselves)
            float w = constraint.WidthMode == MeasureMode.Exactly ? constraint.AvailableWidth : Math.Min(m_contentSize.X, constraint.AvailableWidth);
            float h = constraint.HeightMode == MeasureMode.Exactly ? constraint.AvailableHeight : Math.Min(m_contentSize.Y, constraint.AvailableHeight);

            return new Vector2(w, h);
        }

        protected override void ArrangeContent(Rect finalRect)
        {
            var padding = resolvedLayoutState.padding;
            bool horizScroll = flags.horizontalScrollbar || flags.alwaysHorizontalScrollbar;
            bool vertScroll = !flags.noScrollbar;

            float virtualWidth = horizScroll ? Math.Max(finalRect.width, m_contentSize.X + padding.horizontal) : finalRect.width;
            float virtualHeight = vertScroll ? Math.Max(finalRect.height, m_contentSize.Y + padding.vertical) : finalRect.height;

            Rect virtualRect = new Rect(finalRect.x, finalRect.y, virtualWidth, virtualHeight);
            base.ArrangeContent(virtualRect);
        }

        public override bool OnBeginRender()
        {
            bool shouldRender = base.OnBeginRender();
            if (!shouldRender) return false;

            var size = this.layoutRect.size;
            
            unsafe 
            {
                if (size.X <= 0 || size.Y <= 0) 
                {
                    return false;
                }

                m_beginChildCalled = true;
                m_wasExpanded = ImGui.BeginChild((byte*)s_scrollViewId, size, ImGuiChildFlags.None, flags.Value);
                
                if (m_wasExpanded)
                {
                    float scrollX = ImGui.GetScrollX();
                    float scrollY = ImGui.GetScrollY();
                    
                    var currentOffset = RenderEngine.Context.CurrentRenderOffset;
                    RenderEngine.Context.PushRenderOffset(currentOffset + new Vector2(scrollX, scrollY));
                    
                    // Expand internal scrollable area
                    ImGui.Dummy(m_contentSize);
                }
                
                return m_wasExpanded;
            }
        }

        public override void OnEndRender()
        {
            if (m_beginChildCalled)
            {
                if (m_wasExpanded)
                {
                    RenderEngine.Context.PopRenderOffset();
                }
                ImGui.EndChild();
                m_beginChildCalled = false;
            }
            
            base.OnEndRender();
        }
    }
}
