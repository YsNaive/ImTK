using System;
using ImGuiNET;

namespace ImTK.UI
{
    public abstract class FoldoutDrawer<T> : FieldDrawer<T>
    {
        public bool isExpanded
        {
            get => layoutMode == DrawerLayoutMode.Expand;
            set => layoutMode = value ? DrawerLayoutMode.Expand : DrawerLayoutMode.Inline;
        }

        protected override void OnRenderLayout()
        {
            ImGui.SetNextItemOpen(isExpanded, ImGuiCond.Always);

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth;
            string displayLabel = string.IsNullOrEmpty(label) ? $"###foldout_{this.GetHashCode()}" : label;

            bool treeOpen = ImGui.TreeNodeEx(displayLabel, flags);

            isExpanded = treeOpen;

            if (isExpanded)
            {
                OnRenderSelf();

                foreach (var child in Children())
                {
                    child.Render();
                }

                ImGui.TreePop();
            }
            else
            {
                // When inline (collapsed), we only render self for logic but not children
                OnRenderSelf();
            }
        }
    }
}
