using ImTK.Log;
using System;
using System.Collections.Generic;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public struct TreeViewNodeData<T>
    {
        public T Data;
        public int Depth;
        public bool HasChildren;
    }

    public abstract class TreeView<TData> : VisualElement
    {
        private IEnumerable<TData> m_itemsSource;
        public IEnumerable<TData> itemsSource
        {
            get => m_itemsSource;
            set
            {
                m_itemsSource = value;
                RebuildFlattenedList();
            }
        }

        public bool allowMultiSelect { get; set; } = false;

        private HashSet<TData> m_expandedItems = new HashSet<TData>();
        private List<TData> m_selectedItems = new List<TData>();
        
        private List<TreeViewNodeData<TData>> m_flattenedList = new List<TreeViewNodeData<TData>>();
        
        private List<TreeNode> m_nodePool = new List<TreeNode>();
        private NativeUtf8Buffer m_childIdBuffer = new NativeUtf8Buffer();

        public IReadOnlyList<TData> selectedItems => m_selectedItems;
        public TData selectedItem => m_selectedItems.Count > 0 ? m_selectedItems[0] : default;
        public TData hoveredItem { get; private set; }
        public TData externalHoveredItem { get; set; }

        public event Action<TData> onSelectionChanged;

        protected abstract VisualElement MakeItem();
        protected abstract void BindItem(VisualElement ui, TData item);
        protected abstract IEnumerable<TData> GetItemChildren(TData item);
        protected virtual TData GetItemParent(TData item) { return default; }
        protected abstract bool HasChildren(TData item);

        public TreeView()
        {
            useNativeLayout = false; // Participate in Yoga Layout
            
            RegisterCallback<TreeNodeSelectedEvent>(OnNodeSelected);
            RegisterCallback<TreeNodeExpandedEvent>(OnNodeExpanded);
            
            m_childIdBuffer.SetString($"TreeView_{m_elementId}");
        }

        private void OnNodeSelected(TreeNodeSelectedEvent evt)
        {
            if (evt.node.itemData is TData data)
            {
                SetSelection(data);
                evt.StopPropagation();
            }
        }

        private void OnNodeExpanded(TreeNodeExpandedEvent evt)
        {
            if (evt.node.itemData is TData data)
            {
                if (evt.isExpanded) m_expandedItems.Add(data);
                else m_expandedItems.Remove(data);
                
                RebuildFlattenedList();
                evt.StopPropagation();
            }
        }

        public void SetSelection(TData node)
        {
            if (!allowMultiSelect)
            {
                m_selectedItems.Clear();
                if (node != null) m_selectedItems.Add(node);
            }
            else
            {
                if (node != null)
                {
                    if (m_selectedItems.Contains(node)) m_selectedItems.Remove(node);
                    else m_selectedItems.Add(node);
                }
            }
            onSelectionChanged?.Invoke(node);
            RenderEngine.MarkRenderDirty(this);
        }

        public void ClearSelection()
        {
            m_selectedItems.Clear();
            onSelectionChanged?.Invoke(default);
            RenderEngine.MarkRenderDirty(this);
        }

        public void SetExpanded(TData item, bool expanded)
        {
            if (expanded) m_expandedItems.Add(item);
            else m_expandedItems.Remove(item);
            RebuildFlattenedList();
        }

        public void Reveal(TData item)
        {
            if (EqualityComparer<TData>.Default.Equals(item, default)) return;

            HashSet<TData> visited = new HashSet<TData>();
            TData current = GetItemParent(item);

            while (!EqualityComparer<TData>.Default.Equals(current, default))
            {
                if (!visited.Add(current)) break; // Circular dependency detected
                m_expandedItems.Add(current);
                current = GetItemParent(current);
            }

            RebuildFlattenedList();
            SetSelection(item);
            ScrollTo(item);
        }

        private int m_pendingScrollIndex = -1;

        public void ScrollTo(TData item)
        {
            int index = m_flattenedList.FindIndex(n => EqualityComparer<TData>.Default.Equals(n.Data, item));
            if (index >= 0)
            {
                m_pendingScrollIndex = index;
                RenderEngine.MarkRenderDirty(this);
            }
        }

        public void ExpandAll()
        {
            if (m_itemsSource == null) return;
            foreach (var root in m_itemsSource)
            {
                ExpandAllRecursive(root);
            }
            RebuildFlattenedList();
        }

        private void ExpandAllRecursive(TData data)
        {
            if (HasChildren(data))
            {
                m_expandedItems.Add(data);
                var children = GetItemChildren(data);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        ExpandAllRecursive(child);
                    }
                }
            }
        }

        public void CollapseAll()
        {
            m_expandedItems.Clear();
            RebuildFlattenedList();
        }

        public void RebuildFlattenedList()
        {
            m_flattenedList.Clear();
            if (m_itemsSource == null) return;

            foreach (var root in m_itemsSource)
            {
                AddNodeToListRecursive(root, 0);
            }
            RenderEngine.MarkRenderDirty(this);
        }

        private void AddNodeToListRecursive(TData data, int depth)
        {
            bool hasChildren = HasChildren(data);
            m_flattenedList.Add(new TreeViewNodeData<TData> 
            { 
                Data = data, 
                Depth = depth, 
                HasChildren = hasChildren 
            });

            if (hasChildren && m_expandedItems.Contains(data))
            {
                var children = GetItemChildren(data);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        AddNodeToListRecursive(child, depth + 1);
                    }
                }
            }
        }

        public override bool OnBeginRender()
        {
            base.OnBeginRender();
            // Do NOT render children recursively. We will handle virtualization manually.
            return false;
        }

        public override void OnRender()
        {
            var size = this.layoutRect.size;
            if (size.X <= 0 || size.Y <= 0) 
            {
                ImGui.Dummy(System.Numerics.Vector2.Zero);
                return;
            }

            unsafe
            {
                if (!ImGui.BeginChild((byte*)m_childIdBuffer.Data, size, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    ImGui.EndChild();
                    return;
                }
                
                if (m_pendingScrollIndex >= 0)
                {
                    float scrollY = m_pendingScrollIndex * ImGui.GetFrameHeight();
                    ImGui.SetScrollY(scrollY);
                    m_pendingScrollIndex = -1;
                }
                
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(m_flattenedList.Count);
                
                int poolIndex = 0;
                TData newHoveredItem = default;
                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var data = m_flattenedList[i];
                        
                        TreeNode node = GetOrCreateNode(poolIndex++);
                        node.indentDepth = data.Depth;
                        node.hasChildren = data.HasChildren;
                        node.isExpanded = m_expandedItems.Contains(data.Data);
                        node.isSelected = m_selectedItems.Contains(data.Data);
                        node.forceHoverState = EqualityComparer<TData>.Default.Equals(data.Data, externalHoveredItem);
                        node.itemData = data.Data;
                        
                        BindItem(node.contentElement, data.Data);
                        
                        RenderEngine.Render(node);
                        
                        if (node.m_wasHovered)
                        {
                            newHoveredItem = data.Data;
                        }
                    }
                }
                clipper.End();
                hoveredItem = newHoveredItem;
                
                ImGui.EndChild();
            }
        }

        private TreeNode GetOrCreateNode(int poolIndex)
        {
            if (poolIndex < m_nodePool.Count)
            {
                return m_nodePool[poolIndex];
            }
            
            var customUI = MakeItem();
            var node = new TreeNode(customUI);
            node.SetNodeId(poolIndex);
            
            // 遵守生命週期規範，在 Render 階段外的安全時機才將節點加入視覺樹中
            ImTK.Core.ImTKApplication.ScheduleDeferred(() => 
            {
                this.Add(node);
            });
            m_nodePool.Add(node);
            
            return node;
        }
    }
}
