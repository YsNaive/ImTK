using Hexa.NET.ImGui;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    public class TreeNode : VisualElement
    {
        private NativeUtf8Buffer m_nativeIdBuffer = new NativeUtf8Buffer();
        
        // Data populated by TreeView
        public int indentDepth { get; set; }
        public bool hasChildren { get; set; }
        public bool isExpanded { get; set; }
        public bool isSelected { get; set; }
        
        public object itemData { get; set; }
        
        public VisualElement contentElement { get; private set; }

        public TreeNode(VisualElement content)
        {
            useNativeLayout = true; // Skip Yoga
            contentElement = content;
            if (contentElement != null)
            {
                contentElement.useNativeLayout = true;
                ImTK.Core.ImTKApplication.ScheduleDeferred(() => 
                {
                    this.Add(contentElement);
                });
            }
        }



        public void SetNodeId(int uniqueId)
        {
            m_nativeIdBuffer.SetString($"###treenode_btn_{uniqueId}");
        }

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        public override bool OnBeginRender()
        {
            if (m_nativeIdBuffer.IsEmpty) return false;

            var size = new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight());
            ImGui.BeginGroup();

            var originalPos = ImGui.GetCursorScreenPos();
            
            // Draw InvisibleButton for the entire row FIRST so we can check hover correctly!
            bool clicked = false;
            unsafe
            {
                clicked = ImGui.InvisibleButton((byte*)m_nativeIdBuffer.Data, size);
            }
            
            bool isHovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
            
            // Render selection background
            if (isSelected)
            {
                ImGui.GetWindowDrawList().AddRectFilled(
                    originalPos,
                    originalPos + size,
                    ImGui.GetColorU32(ImGuiCol.HeaderActive)
                );
            }
            else if (isHovered)
            {
                ImGui.GetWindowDrawList().AddRectFilled(
                    originalPos,
                    originalPos + size,
                    ImGui.GetColorU32(ImGuiCol.HeaderHovered)
                );
            }

            // Handle Click
            if (clicked)
            {
                float arrowXStart = originalPos.X + (indentDepth * 15f);
                float arrowXEnd = arrowXStart + 20f;
                bool clickedOnArrow = ImGui.GetMousePos().X >= arrowXStart && ImGui.GetMousePos().X <= arrowXEnd;
                
                if (clickedOnArrow && hasChildren)
                {
                    isExpanded = !isExpanded;
                    var evt = EventPool<TreeNodeExpandedEvent>.Get();
                    evt.node = this;
                    evt.isExpanded = isExpanded;
                    evt.source = this;
                    EventDispatcher.Enqueue(evt);
                }
                else
                {
                    var selEvt = EventPool<TreeNodeSelectedEvent>.Get();
                    selEvt.node = this;
                    selEvt.source = this;
                    EventDispatcher.Enqueue(selEvt);
                }
            }

            // Restore cursor pos for drawing content (still inside the Group)
            ImGui.SetCursorScreenPos(originalPos);

            // Render indent
            if (indentDepth > 0)
            {
                ImGui.Indent(indentDepth * 15f);
            }

            // Draw arrow if has children
            if (hasChildren)
            {
                var drawList = ImGui.GetWindowDrawList();
                float arrowSize = ImGui.GetFontSize() * 0.6f;
                // Center the arrow in the 20px space allocated for it
                Vector2 center = ImGui.GetCursorScreenPos() + new Vector2(10f, size.Y * 0.5f);
                uint color = ImGui.GetColorU32(ImGuiCol.Text);

                if (isExpanded)
                {
                    Vector2 p1 = center + new Vector2(-arrowSize * 0.5f, -arrowSize * 0.25f);
                    Vector2 p2 = center + new Vector2(arrowSize * 0.5f, -arrowSize * 0.25f);
                    Vector2 p3 = center + new Vector2(0, arrowSize * 0.5f);
                    drawList.AddTriangleFilled(p1, p2, p3, color);
                }
                else
                {
                    Vector2 p1 = center + new Vector2(-arrowSize * 0.2f, -arrowSize * 0.4f);
                    Vector2 p2 = center + new Vector2(-arrowSize * 0.2f, arrowSize * 0.4f);
                    Vector2 p3 = center + new Vector2(arrowSize * 0.35f, 0);
                    drawList.AddTriangleFilled(p1, p2, p3, color);
                }
            }
            
            // Use Dummy instead of SetCursorPosX to properly advance and register the window/group bounding box 
            // without triggering ImGui's boundary extension assert.
            ImGui.Dummy(new Vector2(20f, 0f));
            ImGui.SameLine(0, 0);

            // Fix Label layoutRect so it doesn't clip to 0x0 when overflow is hidden
            if (contentElement != null)
            {
                contentElement.layoutRect = new Rect(ImGui.GetCursorScreenPos(), new Vector2(size.X - (indentDepth * 15f) - 20f, size.Y));
            }

            // Let RenderEngine render the contentElement (which is in our hierarchy)
            return true;
        }

        public override void OnEndRender()
        {
            if (indentDepth > 0)
            {
                ImGui.Unindent(indentDepth * 15f);
            }
            ImGui.EndGroup();
        }
    }
}
