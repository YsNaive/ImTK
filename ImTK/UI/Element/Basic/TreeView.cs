using ImTK.Log;
using System;
using System.Collections.Generic;

namespace ImTK.UI
{
    public class TreeView<TNode> : VisualElement, ITreeView where TNode : TreeNode
    {
        public bool allowMultiSelect { get; set; } = false;

        private List<TNode> m_selectedNodes = new List<TNode>();

        public IReadOnlyList<TNode> selectedNodes => m_selectedNodes;
        public TNode selectedNode => m_selectedNodes.Count > 0 ? m_selectedNodes[0] : null;

        public event Action<TNode> onSelectionChanged;

        public TreeView()
        {
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = AlignItems.Stretch;
            
            RegisterCallback<TreeNodeSelectedEvent>(OnNodeSelected);
        }

        private void OnNodeSelected(TreeNodeSelectedEvent evt)
        {
            if (evt.node is TNode tNode)
            {
                SetSelection(tNode);
                evt.StopPropagation();
            }
        }

        public void SetSelection(TNode node)
        {
            if (!allowMultiSelect)
            {
                ClearSelection();
                if (node != null)
                {
                    node.isSelected = true;
                    m_selectedNodes.Add(node);
                }
            }
            else
            {
                if (node != null)
                {
                    if (node.isSelected)
                    {
                        node.isSelected = false;
                        m_selectedNodes.Remove(node);
                    }
                    else
                    {
                        node.isSelected = true;
                        m_selectedNodes.Add(node);
                    }
                }
            }

            onSelectionChanged?.Invoke(node);
        }

        public void ClearSelection()
        {
            foreach (var n in m_selectedNodes)
            {
                n.isSelected = false;
            }
            m_selectedNodes.Clear();
        }

        public new void Add(VisualElement child)
        {
            if (!(child is TNode))
            {
                ImTKLog.Error($"Cannot add '{child.GetType().Name}' to TreeView<{typeof(TNode).Name}>. Only {typeof(TNode).Name} objects can be added.");
                return;
            }
            base.Add(child);
        }

        public void ExpandAll()
        {
            foreach (var child in hierarchy.Children())
            {
                if (child is TreeNode node)
                {
                    ExpandRecursive(node);
                }
            }
        }

        private void ExpandRecursive(TreeNode node)
        {
            node.isExpanded = true;
            foreach (var child in node.contentContainer.hierarchy.Children())
            {
                if (child is TreeNode childNode)
                {
                    ExpandRecursive(childNode);
                }
            }
        }

        public void CollapseAll()
        {
            foreach (var child in hierarchy.Children())
            {
                if (child is TreeNode node)
                {
                    CollapseRecursive(node);
                }
            }
        }

        private void CollapseRecursive(TreeNode node)
        {
            node.isExpanded = false;
            foreach (var child in node.contentContainer.hierarchy.Children())
            {
                if (child is TreeNode childNode)
                {
                    CollapseRecursive(childNode);
                }
            }
        }
    }
}
