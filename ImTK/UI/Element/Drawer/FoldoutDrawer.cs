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
            set
            {
                layoutMode = value ? DrawerLayoutMode.Expand : DrawerLayoutMode.Inline;
                m_contentContainer.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                if (m_iconElement != null)
                {
                    m_iconElement.type = value ? IconElement.IconType.DownArrow : IconElement.IconType.RightArrow;
                }
            }
        }

        protected FoldoutDrawer() : base()
        {
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = AlignItems.Stretch;
            // Initially expanded? Let's say expand. Or inline means collapsed for foldout.
            isExpanded = false;
        }

        protected internal bool m_isHeaderHovered;

        private class FoldoutHeaderContainer : VisualElement
        {
            private FoldoutDrawer<T> m_drawer;
            public FoldoutHeaderContainer(FoldoutDrawer<T> drawer)
            {
                m_drawer = drawer;
            }

            public override void OnRender()
            {
                ImGui.SetNextItemAllowOverlap();
                
                if (ImGui.InvisibleButton($"###foldout_btn_{m_drawer.GetHashCode()}", this.layoutRect.size))
                {
                    m_drawer.isExpanded = !m_drawer.isExpanded;
                }

                m_drawer.m_isHeaderHovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
                if (m_drawer.m_isHeaderHovered)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    ImGui.GetWindowDrawList().AddRectFilled(
                        this.layoutRect.position,
                        this.layoutRect.position + this.layoutRect.size,
                        ImGui.GetColorU32(ImGuiCol.HeaderHovered)
                    );
                }
            }
        }

        protected override VisualElement CreateHeaderContainer()
        {
            return new FoldoutHeaderContainer(this);
        }

    }
}
