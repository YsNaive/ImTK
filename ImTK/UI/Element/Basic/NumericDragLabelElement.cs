using System;
using Hexa.NET.ImGui;
using System.Numerics;

namespace ImTK.UI
{
    public class NumericDragLabelElement : TextElement
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
            if (string.IsNullOrEmpty(text)) return;

            System.Numerics.Vector2 textSize;
            unsafe {
                fixed (byte* pText = System.Text.Encoding.UTF8.GetBytes(text + "\0")) {
                    textSize = ImGui.CalcTextSize(pText);
                }
            }
            float frameHeight = ImGui.GetFrameHeight();
            
            // Optional: allow subsequent items to overlap this button
            ImGui.SetNextItemAllowOverlap();

            var buttonPos = ImGui.GetCursorScreenPos();
            float dragWidth = Math.Max(textSize.X, this.layoutRect.width);
            ImGui.InvisibleButton("##drag_" + text, new Vector2(dragWidth, frameHeight));
            
            bool isDragActive = ImGui.IsItemActive();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
            }

            var endPos = ImGui.GetCursorScreenPos();
            float textYOffset = (frameHeight - textSize.Y) * 0.5f;
            ImGui.SetCursorScreenPos(new Vector2(buttonPos.X, buttonPos.Y + textYOffset));
            ImGui.TextUnformatted(text);
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
