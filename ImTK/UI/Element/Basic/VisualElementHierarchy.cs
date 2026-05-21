using System;
using System.Collections.Generic;
using ImTK.Core;
using ImTK.Log;

namespace ImTK.UI
{
    public class VisualElementHierarchy : IVisualElementHierarchy
    {
        private static readonly LogContext s_log = new LogContext("VisualElementHierarchy");
        private readonly VisualElement m_owner;
        private readonly List<VisualElement> m_children = new List<VisualElement>();

        public VisualElement parent { get; internal set; }

        public VisualElementHierarchy(VisualElement owner)
        {
            m_owner = owner;
        }

        public int childCount => m_children.Count;

        public VisualElement childAt(int index) => m_children[index];

        internal bool CheckSafeState()
        {
            if (ImTKApplication.CurrentState == ApplicationState.GuiRender)
            {
                s_log.Error("Cannot modify VisualElement hierarchy during GuiRender state. Use Event System or delay the operation.");
                return false;
            }
            return true;
        }

        public void Add(VisualElement child)
        {
            if (!CheckSafeState()) return;

            if (child == null) throw new ArgumentNullException(nameof(child));

            NodeType type = child.GetNodeType();
            if (type == NodeType.LogicNode)
            {
                child.parent.Remove(child);
            }
            else if (type == NodeType.PhysicsNode)
            {
                child.hierarchy.parent.hierarchy.Remove(child);
            }

            m_children.Add(child);
            child.hierarchy.parent = m_owner;

            EventDispatcher.MarkHierarchyDirty(m_owner);
        }

        public void AddRange(IEnumerable<VisualElement> children)
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            foreach (var child in children)
            {
                Add(child);
            }
        }

        public void Remove(VisualElement child)
        {
            if (!CheckSafeState()) return;

            if (child == null) return;
            if (m_children.Remove(child))
            {
                child.hierarchy.parent = null;
            }

            EventDispatcher.MarkHierarchyDirty(m_owner);
        }

        public void Clear()
        {
            if (!CheckSafeState()) return;

            foreach (var child in m_children)
            {
                child.hierarchy.parent = null;
            }
            m_children.Clear();

            EventDispatcher.MarkHierarchyDirty(m_owner);
        }

        public IEnumerable<VisualElement> Children()
        {
            return m_children;
        }
    }

    public interface IVisualElementHierarchy
        {
            VisualElement parent { get; }
            int childCount { get; }
            VisualElement childAt(int index);
            void Add(VisualElement child);
            void Remove(VisualElement child);
            void Clear();
            void AddRange(IEnumerable<VisualElement> children);
            IEnumerable<VisualElement> Children();
        }
}
