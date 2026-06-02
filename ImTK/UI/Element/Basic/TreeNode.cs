using Hexa.NET.ImGui;
using ImTK.Log;
using System;

namespace ImTK.UI
{
    public interface ITreeView
    {
        // Interface for TreeView methods if needed by TreeNode
    }

    public class TreeNode : VisualElement
    {
        private class TreeNodeHeaderContainer : VisualElement
        {
            private TreeNode m_treeNode;
            public TreeNodeHeaderContainer(TreeNode treeNode)
            {
                m_treeNode = treeNode;
                this.style.flexDirection = FlexDirection.Row;
                this.style.alignItems = AlignItems.Center;
                this.style.flexGrow = 1;
            }

            protected internal override bool CheckHoverState()
            {
                return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
            }

            public override void OnRender()
            {
                ImGui.SetNextItemAllowOverlap();
                if (this.layoutRect.size.X <= 0f || this.layoutRect.size.Y <= 0f) return;
                
                string idStr = $"###treenode_btn_{m_treeNode.GetHashCode()}";
                
                ImGui.SetCursorScreenPos(this.layoutRect.position);
                unsafe
                {
                    fixed (char* idPtr = idStr)
                    {
                        if (ImGui.InvisibleButton((byte*)idPtr, this.layoutRect.size))
                        {
                            bool clickedOnArrow = ImGui.GetMousePos().X <= (this.layoutRect.position.X + 20);
                            var mode = m_treeNode.interactiveMode;
                            
                            if (m_treeNode.selectable)
                            {
                                if (!clickedOnArrow || mode == InteractiveMode.FullHeader)
                                {
                                    m_treeNode.isSelected = true;
                                    
                                    var selEvt = EventPool<TreeNodeSelectedEvent>.Get();
                                    selEvt.node = m_treeNode;
                                    selEvt.source = m_treeNode;
                                    EventDispatcher.Enqueue(selEvt);
                                }
                            }

                            bool shouldExpand = false;
                            if (mode == InteractiveMode.FullHeader) shouldExpand = true;
                            else if (mode == InteractiveMode.OnlyArrowIcon && clickedOnArrow) shouldExpand = true;

                            if (shouldExpand && !m_treeNode.isLeaf)
                            {
                                m_treeNode.isExpanded = !m_treeNode.isExpanded;
                            }
                        }
                    }
                }

                bool isHovered = CheckHoverState();
                
                if (m_treeNode.isSelected)
                {
                    ImGui.GetWindowDrawList().AddRectFilled(
                        this.layoutRect.position,
                        this.layoutRect.position + this.layoutRect.size,
                        ImGui.GetColorU32(ImGuiCol.HeaderActive)
                    );
                }
                else if (isHovered)
                {
                    ImGui.GetWindowDrawList().AddRectFilled(
                        this.layoutRect.position,
                        this.layoutRect.position + this.layoutRect.size,
                        ImGui.GetColorU32(ImGuiCol.HeaderHovered)
                    );
                }
            }
        }

        public VisualElement headerContainer { get; private set; }
        private VisualElement m_contentContainer;

        public override VisualElement contentContainer => m_contentContainer;

        private IconElement m_arrowIcon;
        private Label m_label;

        public enum InteractiveMode
        {
            OnlyArrowIcon,
            FullHeader,
            Auto,
            None
        }

        private InteractiveMode m_interactiveMode = InteractiveMode.Auto;
        public InteractiveMode interactiveMode
        {
            get
            {
                if (m_interactiveMode == InteractiveMode.Auto)
                {
                    if (isLeaf) return InteractiveMode.FullHeader;
                    return selectable ? InteractiveMode.OnlyArrowIcon : InteractiveMode.FullHeader;
                }
                if (isLeaf && m_interactiveMode != InteractiveMode.None) return InteractiveMode.None;
                return m_interactiveMode;
            }
            set => m_interactiveMode = value;
        }

        public bool selectable { get; set; } = true;

        public virtual bool isLeaf => childCount == 0;

        private bool m_isExpanded = false;
        public bool isExpanded
        {
            get => m_isExpanded;
            set
            {
                if (m_isExpanded != value)
                {
                    m_isExpanded = value;
                    m_contentContainer.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                    UpdateIconVisibility();
                    
                    if (value) OnExpand();
                    else OnCollapse();
                    
                    var evt = EventPool<TreeNodeExpandedEvent>.Get();
                    evt.node = this;
                    evt.isExpanded = value;
                    evt.source = this;
                    EventDispatcher.Enqueue(evt);
                }
            }
        }

        private bool m_isSelected = false;
        public bool isSelected
        {
            get => m_isSelected;
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    RenderEngine.MarkRenderDirty(this);
                }
            }
        }

        public string text
        {
            get => m_label?.text;
            set
            {
                if (m_label != null) m_label.text = value;
            }
        }

        protected virtual void OnExpand() { }
        protected virtual void OnCollapse() { }

        public TreeNode(string text = "")
        {
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = AlignItems.Stretch;

            headerContainer = new TreeNodeHeaderContainer(this);
            this.hierarchy.Add(headerContainer);

            m_arrowIcon = new IconElement();
            m_arrowIcon.type = IconElement.IconType.RightArrow;
            m_arrowIcon.style.width = 15;
            m_arrowIcon.style.height = 15;
            m_arrowIcon.style.margin = new Thickness(0, 5, 0, 0);
            
            // By default hidden if no children, to keep natural alignment
            m_arrowIcon.type = IconElement.IconType.None;
            
            headerContainer.Add(m_arrowIcon);

            m_label = new Label(text);
            m_label.style.flexGrow = 1;
            headerContainer.Add(m_label);

            m_contentContainer = new VisualElement();
            m_contentContainer.style.flexDirection = FlexDirection.Column;
            m_contentContainer.style.alignItems = AlignItems.Stretch;
            m_contentContainer.style.margin = new Thickness(20, 0, 0, 0);
            m_contentContainer.style.display = DisplayStyle.None;
            
            this.hierarchy.Add(m_contentContainer);
        }

        public new void Add(VisualElement child)
        {
            if (!(child is TreeNode))
            {
                ImTKLog.Error($"Cannot add '{child.GetType().Name}' to TreeNode. Only TreeNode objects can be added.");
                return;
            }
            base.Add(child);
            UpdateIconVisibility();
        }

        public new void Remove(VisualElement child)
        {
            base.Remove(child);
            UpdateIconVisibility();
        }

        public new void Clear()
        {
            base.Clear();
            UpdateIconVisibility();
        }

        protected virtual void UpdateIconVisibility()
        {
            bool hasChildren = !isLeaf;
            m_arrowIcon.type = hasChildren ? (m_isExpanded ? IconElement.IconType.DownArrow : IconElement.IconType.RightArrow) : IconElement.IconType.None;
        }

        public ITreeView GetTreeView()
        {
            var p = this.parent;
            while (p != null)
            {
                if (p is ITreeView tv) return tv;
                if (!(p is TreeNode)) break;
                p = p.parent;
            }
            return null;
        }
    }
}
