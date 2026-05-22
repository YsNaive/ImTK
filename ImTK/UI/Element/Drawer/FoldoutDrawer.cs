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

        protected override void OnRenderIcon(ImDrawListPtr drawList, ImRect iconRect)
        {
            float padding = 2.0f;
            uint color = ImGui.GetColorU32(ImGuiCol.Text);

            if (isExpanded)
            {
                drawList.AddTriangleFilled(new System.Numerics.Vector2(iconRect.min.X + padding, iconRect.min.Y + padding), new System.Numerics.Vector2(iconRect.max.X - padding, iconRect.min.Y + padding), new System.Numerics.Vector2((iconRect.min.X + iconRect.max.X) / 2, iconRect.max.Y - padding), color);
            }
            else
            {
                drawList.AddTriangleFilled(new System.Numerics.Vector2(iconRect.min.X + padding, iconRect.min.Y + padding), new System.Numerics.Vector2(iconRect.max.X - padding, (iconRect.min.Y + iconRect.max.Y) / 2), new System.Numerics.Vector2(iconRect.min.X + padding, iconRect.max.Y - padding), color);
            }

            ImGui.SetCursorScreenPos(iconRect.min);
            if (ImGui.InvisibleButton($"###foldout_btn_{this.GetHashCode()}", new System.Numerics.Vector2(iconRect.max.X - iconRect.min.X, iconRect.max.Y - iconRect.min.Y)))
            {
                isExpanded = !isExpanded;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        }

        protected override void OnRenderLabel()
        {
            base.OnRenderLabel();

            if (ImGui.IsItemClicked())
            {
                isExpanded = !isExpanded;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        }

        protected override void OnRenderLayout()
        {
            float labelWidth = theme.labelWidth;
            float frameHeight = ImGui.GetFrameHeight();

            System.Numerics.Vector2 cursorPos = ImGui.GetCursorScreenPos();
            ImRect iconRect = new ImRect(cursorPos, new System.Numerics.Vector2(cursorPos.X + frameHeight, cursorPos.Y + frameHeight));

            ImGui.Dummy(new System.Numerics.Vector2(frameHeight, frameHeight));
            OnRenderIcon(ImGui.GetWindowDrawList(), iconRect);

            if (layoutMode == DrawerLayoutMode.Inline)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                if (!string.IsNullOrEmpty(label))
                {
                    float currentX = ImGui.GetCursorPosX();
                    float targetX = labelWidth;
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

                float indent = frameHeight + ImGui.GetStyle().ItemInnerSpacing.X;
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
