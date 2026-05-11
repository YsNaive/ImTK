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

            if (child.hierarchy.parent != null)
            {
                child.hierarchy.parent.hierarchy.Remove(child);
            }

            m_children.Add(child);
            child.hierarchy.parent = m_owner;
        }

        public void Remove(VisualElement child)
        {
            if (!CheckSafeState()) return;

            if (child == null) return;
            if (m_children.Remove(child))
            {
                child.hierarchy.parent = null;
            }
        }

        public void Clear()
        {
            if (!CheckSafeState()) return;

            foreach (var child in m_children)
            {
                child.hierarchy.parent = null;
            }
            m_children.Clear();
        }

        public IEnumerable<VisualElement> Children()
        {
            return m_children;
        }
    }
}
