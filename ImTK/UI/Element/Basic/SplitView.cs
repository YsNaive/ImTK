using Hexa.NET.ImGui;
using ImTK.Log;
using ImTK.UI.Persistence;
using System;
using System.Numerics;

namespace ImTK.UI
{
    public class SplitView : VisualElement
    {
        [Persistent]
        private int m_fixedPaneIndex = 0;
        
        public int fixedPaneIndex
        {
            get => m_fixedPaneIndex;
            set
            {
                m_fixedPaneIndex = Math.Clamp(value, 0, 1);
                UpdateSplitLayout();
            }
        }

        [Persistent]
        private float m_fixedPaneDimension = 200f;
        
        public float fixedPaneDimension
        {
            get => m_fixedPaneDimension;
            set
            {
                m_fixedPaneDimension = value;
                UpdateSplitLayout();
            }
        }

        [Persistent]
        public float minSplitSize { get; set; } = 50f;

        public float splitBarSize { get; set; } = 4f;

        public VisualElement fixedElement => hierarchy.childCount > m_fixedPaneIndex ? hierarchy.ChildAt(m_fixedPaneIndex) : null;
        public VisualElement flexElement => hierarchy.childCount > (1 - m_fixedPaneIndex) ? hierarchy.ChildAt(1 - m_fixedPaneIndex) : null;

        public SplitView()
        {
            classList.Add("SplitView");
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1f; // 預設充滿父容器，避免被 shrink-wrap
        }

        public new void Add(VisualElement child)
        {
            if (hierarchy.childCount >= 2)
            {
                ImTKLog.Error("SplitView cannot have more than 2 children.");
                return;
            }
            base.Add(child);
            if (hierarchy.childCount == 2)
            {
                UpdateSplitLayout();
            }
        }



        private void UpdateSplitLayout()
        {
            if (hierarchy.childCount < 2) return;

            if (!style.itemSpacing.HasValue || style.itemSpacing.Value.Value.X != splitBarSize || style.itemSpacing.Value.Value.Y != splitBarSize)
            {
                style.itemSpacing = new Vector2(splitBarSize, splitBarSize);
            }

            var fixedChild = fixedElement;
            var flexChild = flexElement;
            if (fixedChild == null || flexChild == null) return;

            bool isRow = style.flexDirection.HasValue && style.flexDirection.Value.Value == FlexDirection.Row;

            if (isRow)
            {
                if (!fixedChild.style.width.HasValue || fixedChild.style.width.Value.Value != m_fixedPaneDimension)
                    fixedChild.style.width = m_fixedPaneDimension;
                
                if (fixedChild.style.height.HasValue)
                    fixedChild.style.height = null;
                
                if (flexChild.style.width.HasValue)
                    flexChild.style.width = null;
                
                if (flexChild.style.height.HasValue)
                    flexChild.style.height = null;

                if (!flexChild.style.minWidth.HasValue || flexChild.style.minWidth.Value.Value != minSplitSize)
                    flexChild.style.minWidth = minSplitSize;
                
                if (flexChild.style.minHeight.HasValue)
                    flexChild.style.minHeight = null;
            }
            else
            {
                if (fixedChild.style.width.HasValue)
                    fixedChild.style.width = null;
                
                if (!fixedChild.style.height.HasValue || fixedChild.style.height.Value.Value != m_fixedPaneDimension)
                    fixedChild.style.height = m_fixedPaneDimension;
                
                if (flexChild.style.width.HasValue)
                    flexChild.style.width = null;
                
                if (flexChild.style.height.HasValue)
                    flexChild.style.height = null;

                if (!flexChild.style.minHeight.HasValue || flexChild.style.minHeight.Value.Value != minSplitSize)
                    flexChild.style.minHeight = minSplitSize;
                
                if (flexChild.style.minWidth.HasValue)
                    flexChild.style.minWidth = null;
            }

            if (!flexChild.style.flexGrow.HasValue || flexChild.style.flexGrow.Value.Value != 1f)
                flexChild.style.flexGrow = 1f;
            
            if (!fixedChild.style.flexGrow.HasValue || fixedChild.style.flexGrow.Value.Value != 0f)
                fixedChild.style.flexGrow = 0f;
        }

        public override bool OnBeginRender()
        {
            if (hierarchy.childCount >= 2)
            {
                bool isRow = style.flexDirection.HasValue && style.flexDirection.Value.Value == FlexDirection.Row;
                float totalSize = isRow ? layoutRect.width : layoutRect.height;

                if (totalSize > 0)
                {
                    float maxSplit = Math.Max(minSplitSize, totalSize - minSplitSize - splitBarSize);
                    if (m_fixedPaneDimension > maxSplit)
                    {
                        fixedPaneDimension = maxSplit; // This will trigger UpdateSplitLayout internally
                    }
                }
            }
            return base.OnBeginRender();
        }

        public override void OnRender()
        {
            base.OnRender();

            if (hierarchy.childCount < 2) return;

            var child0 = hierarchy.ChildAt(0);
            if (child0 == null) return;

            bool isRow = style.flexDirection.HasValue && style.flexDirection.Value.Value == FlexDirection.Row;
            
            Rect splitterRect;
            if (isRow)
            {
                float x = child0.layoutRect.max.X;
                splitterRect = new Rect(x, layoutRect.y, splitBarSize, layoutRect.height);
            }
            else
            {
                float y = child0.layoutRect.max.Y;
                splitterRect = new Rect(layoutRect.x, y, layoutRect.width, splitBarSize);
            }

            ImGui.SetCursorScreenPos(splitterRect.position - RenderEngine.Context.CurrentRenderOffset);
            
            // 使用唯一 ID 防止多個 SplitView 衝突
            ImGui.InvisibleButton($"##Splitter_{GetHashCode()}", splitterRect.size);

            bool isHovered = ImGui.IsItemHovered();
            bool isActive = ImGui.IsItemActive();

            Vector2 renderPos = splitterRect.position - RenderEngine.Context.CurrentRenderOffset;
            var drawList = ImGui.GetWindowDrawList();

            // 畫底色 (預設為 Button 顏色，懸浮或按下狀態給予高亮回饋)
            uint bgColor;
            if (isActive) bgColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);
            else if (isHovered) bgColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
            else bgColor = ImGui.GetColorU32(ImGuiCol.Button);
            
            drawList.AddRectFilled(renderPos, renderPos + splitterRect.size, bgColor);

            // 畫中間的抓取提示線 (Grip lines)
            Vector2 center = renderPos + splitterRect.size * 0.5f;
            uint gripColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
            
            if (isRow)
            {
                drawList.AddLine(new Vector2(center.X - 1, center.Y - 8), new Vector2(center.X - 1, center.Y + 8), gripColor);
                drawList.AddLine(new Vector2(center.X + 1, center.Y - 8), new Vector2(center.X + 1, center.Y + 8), gripColor);
            }
            else
            {
                drawList.AddLine(new Vector2(center.X - 8, center.Y - 1), new Vector2(center.X + 8, center.Y - 1), gripColor);
                drawList.AddLine(new Vector2(center.X - 8, center.Y + 1), new Vector2(center.X + 8, center.Y + 1), gripColor);
            }

            if (isHovered)
            {
                ImGui.SetMouseCursor(isRow ? ImGuiMouseCursor.ResizeEw : ImGuiMouseCursor.ResizeNs);
            }

            if (isActive)
            {
                var delta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left);
                float deltaVal = isRow ? delta.X : delta.Y;

                if (deltaVal != 0)
                {
                    if (m_fixedPaneIndex == 1)
                    {
                        deltaVal = -deltaVal;
                    }

                    float maxSplitSize = isRow ? layoutRect.width - minSplitSize - splitBarSize : layoutRect.height - minSplitSize - splitBarSize;
                    float newSize = Math.Clamp(m_fixedPaneDimension + deltaVal, minSplitSize, maxSplitSize);

                    if (newSize != m_fixedPaneDimension)
                    {
                        fixedPaneDimension = newSize;
                        ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
                    }
                }
            }

        }
    }
}
