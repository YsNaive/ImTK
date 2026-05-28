using System;
using System.Collections.Generic;
using ImTK.Core;
using ImTK.Log;

namespace ImTK.UI
{
    public class VisualElementHierarchy : IVisualElementHierarchy
    {

        private readonly VisualElement m_owner;
        private readonly List<VisualElement> m_children = new List<VisualElement>();

        public VisualElement parent { get; internal set; }

        public VisualElementHierarchy(VisualElement owner)
        {
            m_owner = owner;
        }

        public int childCount => m_children.Count;

        public VisualElement ChildAt(int index) => m_children[index];

        internal bool CheckSafeState()
        {
            if (ImTKApplication.CurrentState == ApplicationState.GuiRender)
            {
                ImTKLog.Error("Cannot modify VisualElement hierarchy during GuiRender state. Use Event System or delay the operation.");
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
            m_owner.GetRenderRoot()?.RenderCache.MarkDirty();
            m_owner.MarkMeasureDirty();
            m_owner.MarkArrangeDirty();
        }

        public void AddRange(IEnumerable<VisualElement> children)
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            foreach (var child in children)
            {
                Add(child);
            }
        }

        internal void SortChildren(Comparison<VisualElement> comparison, bool notify = true)
        {
            if (!CheckSafeState()) return;
            m_children.Sort(comparison);
            if (notify) EventDispatcher.MarkHierarchyDirty(m_owner);
            m_owner.GetRenderRoot()?.RenderCache.MarkDirty();
            m_owner.MarkMeasureDirty();
            m_owner.MarkArrangeDirty();
        }

        internal void Insert(int index, VisualElement child, bool notify = true)
        {
            if (!CheckSafeState()) return;
            if (child == null) throw new ArgumentNullException(nameof(child));

            NodeType type = child.GetNodeType();
            if (type == NodeType.LogicNode)
            {
                child.parent?.Remove(child);
            }
            else if (type == NodeType.PhysicsNode)
            {
                child.hierarchy.parent?.hierarchy.Remove(child);
            }

            m_children.Insert(index, child);
            child.hierarchy.parent = m_owner;

            if (notify) EventDispatcher.MarkHierarchyDirty(m_owner);
            m_owner.GetRenderRoot()?.RenderCache.MarkDirty();
            m_owner.MarkMeasureDirty();
            m_owner.MarkArrangeDirty();
        }

        public void Remove(VisualElement child, bool notify = true)
        {
            if (!CheckSafeState()) return;

            if (child == null) return;
            if (m_children.Remove(child))
            {
                child.hierarchy.parent = null;
            }

            if (notify) EventDispatcher.MarkHierarchyDirty(m_owner);
            m_owner.GetRenderRoot()?.RenderCache.MarkDirty();
            m_owner.MarkMeasureDirty();
            m_owner.MarkArrangeDirty();
        }

        public void Clear(bool notify = true)
        {
            if (!CheckSafeState()) return;

            foreach (var child in m_children)
            {
                child.hierarchy.parent = null;
            }
            m_children.Clear();

            if (notify) EventDispatcher.MarkHierarchyDirty(m_owner);
            m_owner.GetRenderRoot()?.RenderCache.MarkDirty();
            m_owner.MarkMeasureDirty();
            m_owner.MarkArrangeDirty();
        }

        public IEnumerable<VisualElement> Children()
        {
            return m_children;
        }

        void IVisualElementHierarchy.Remove(VisualElement child) => Remove(child, true);
        void IVisualElementHierarchy.Clear() => Clear(true);
    }

    public interface IVisualElementHierarchy
        {
            VisualElement parent { get; }
            int childCount { get; }
            VisualElement ChildAt(int index);
            void Add(VisualElement child);
            void Remove(VisualElement child);
            void Clear();
            void AddRange(IEnumerable<VisualElement> children);
            IEnumerable<VisualElement> Children();
        }
}
