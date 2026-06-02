using System;
using Hexa.NET.ImGui;
using System.Numerics;

namespace ImTK.UI
{
    public class NumericDragLabelElement : Label
    {
        public Action<float> onDrag;

        public NumericDragLabelElement() : base()
        {
        }

        public NumericDragLabelElement(string text) : base(text)
        {
        }

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        protected override Vector2 MeasureContent(LayoutConstraint constraint)
        {
            var size = base.MeasureContent(constraint);
            float minWidth = ImGui.GetFrameHeight();
            size.X = Math.Max(size.X, minWidth);
            return size;
        }

        public override void OnRender()
        {
            if (m_textBuffer.IsEmpty) return;

            System.Numerics.Vector2 textSize;
            unsafe {
                textSize = ImGui.CalcTextSize((byte*)m_textBuffer.Data);
            }
            float frameHeight = ImGui.GetFrameHeight();
            
            // Optional: allow subsequent items to overlap this button
            ImGui.SetNextItemAllowOverlap();

            var buttonPos = ImGui.GetCursorScreenPos();
            float dragWidth = Math.Max(textSize.X, this.layoutRect.width);
            if (dragWidth <= 0f) dragWidth = 1f; // Prevent zero-size assertion
            ImGui.InvisibleButton("##drag_num", new Vector2(dragWidth, frameHeight)); // Note: Using static string instead of "##drag_" + text to avoid GC
            
            bool isDragActive = ImGui.IsItemActive();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
            }

            var endPos = ImGui.GetCursorScreenPos();
            float textYOffset = (frameHeight - textSize.Y) * 0.5f;
            ImGui.SetCursorScreenPos(new Vector2(buttonPos.X, buttonPos.Y + textYOffset));
            unsafe { ImGui.TextUnformatted((byte*)m_textBuffer.Data); }
            ImGui.SetCursorScreenPos(endPos);

            if (isDragActive && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                float delta = ImGui.GetIO().MouseDelta.X;
                if (delta != 0 && onDrag != null)
                {
                    onDrag(delta);
                }
            }
        }
    }
}
