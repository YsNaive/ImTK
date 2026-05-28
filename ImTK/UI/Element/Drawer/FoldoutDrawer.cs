using System;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public abstract class FoldoutDrawer<T> : FieldDrawer<T>
    {
        public override DrawerLayoutMode layoutMode
        {
            get => base.layoutMode;
            set
            {
                base.layoutMode = value;
                bool expanded = (value == DrawerLayoutMode.Expand);
                m_contentContainer.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                if (m_iconElement != null)
                {
                    m_iconElement.type = expanded ? IconElement.IconType.DownArrow : IconElement.IconType.RightArrow;
                }
            }
        }

        public virtual bool isExpanded
        {
            get => layoutMode == DrawerLayoutMode.Expand;
            set => layoutMode = value ? DrawerLayoutMode.Expand : DrawerLayoutMode.Inline;
        }

        protected FoldoutDrawer() : base()
        {
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = AlignItems.Stretch;
            m_contentContainer.style.margin = new Thickness(this.theme.indentWidth, 0, 0, 0);
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
            var container = new FoldoutHeaderContainer(this);
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = AlignItems.Center;
            container.style.flexGrow = 1;
            return container;
        }

    }
}
