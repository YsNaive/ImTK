using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public abstract class FoldoutDrawer<T> : FieldDrawer<T>
    {
        public bool isExpanded
        {
            get => layoutMode == DrawerLayoutMode.Expand;
            set => layoutMode = value ? DrawerLayoutMode.Expand : DrawerLayoutMode.Inline;
        }

        protected override void OnRenderIcon(ImDrawListPtr drawList, Rect iconRect)
        {
            uint color = ImGui.GetColorU32(ImGuiCol.Text);
            System.Numerics.Vector2 center = iconRect.center;

            float radius = iconRect.width * 0.5f * 0.7f;

            if (isExpanded)
            {
                // Down arrow
                drawList.AddTriangleFilled(
                    new System.Numerics.Vector2(center.X - radius, center.Y - radius * 0.5f),
                    new System.Numerics.Vector2(center.X + radius, center.Y - radius * 0.5f),
                    new System.Numerics.Vector2(center.X, center.Y + radius),
                    color);
            }
            else
            {
                // Right arrow
                drawList.AddTriangleFilled(
                    new System.Numerics.Vector2(center.X - radius * 0.5f, center.Y - radius),
                    new System.Numerics.Vector2(center.X + radius, center.Y),
                    new System.Numerics.Vector2(center.X - radius * 0.5f, center.Y + radius),
                    color);
            }
        }

        protected override void OnRenderLabel()
        {
            base.OnRenderLabel();
        }

        protected override void OnRenderLayout()
        {
            float currentLabelWidth = labelWidth.Value;
            float frameHeight = ImGui.GetFrameHeight();
            float iconSize = frameHeight * 0.8f;
            float yOffset = (frameHeight - iconSize) * 0.5f;

            System.Numerics.Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Rect iconRect = new Rect(
                new System.Numerics.Vector2(cursorPos.X, cursorPos.Y + yOffset),
                new System.Numerics.Vector2(iconSize, iconSize)
            );

            float headerWidth = ImGui.GetContentRegionAvail().X;

            ImGui.SetNextItemAllowOverlap();
            if (ImGui.InvisibleButton($"###foldout_btn_{this.GetHashCode()}", new System.Numerics.Vector2(headerWidth, frameHeight)))
            {
                isExpanded = !isExpanded;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                ImGui.GetWindowDrawList().AddRectFilled(
                    cursorPos,
                    new System.Numerics.Vector2(cursorPos.X + headerWidth, cursorPos.Y + frameHeight),
                    ImGui.GetColorU32(ImGuiCol.HeaderHovered)
                );
            }

            ImGui.SetCursorScreenPos(cursorPos);

            ImGui.Dummy(new System.Numerics.Vector2(iconSize, frameHeight));
            OnRenderIcon(ImGui.GetWindowDrawList(), iconRect);

            if (layoutMode == DrawerLayoutMode.Inline)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                if (!string.IsNullOrEmpty(label))
                {
                    float currentX = ImGui.GetCursorPosX();
                    float targetX = currentLabelWidth;
                    if (currentX < targetX)
                    {
                        ImGui.SameLine(targetX);
                    }
                    else
                    {
                        ImGui.SameLine();
                    }
                }
                else
                {
                    ImGui.SameLine();
                }

                ImGui.SetNextItemWidth(-1);

                OnRenderSelf();
            }
            else if (layoutMode == DrawerLayoutMode.Expand)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                float indent = iconSize + ImGui.GetStyle().ItemInnerSpacing.X;
                ImGui.Indent(indent);
                ImGui.SetNextItemWidth(-1);

                OnRenderSelf();
                foreach (var child in Children())
                {
                    child.Render();
                }

                ImGui.Unindent(indent);
            }
        }
    }
}
