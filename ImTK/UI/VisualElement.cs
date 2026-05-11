using System;
using System.Collections.Generic;
using ImGuiNET;

namespace ImTK.UI
{
    public class VisualElement : IVisualElementHierarchy
    {
        private static int s_elementCounter = 0;
        protected readonly int m_elementId;

        public VisualElementHierarchy hierarchy { get; }
        public virtual VisualElement contentContainer => this;

        public VisualElement parent { get; internal set; }

        public PickingMode pickingMode { get; set; } = PickingMode.Position;
        protected bool m_wasHovered = false;

        private Dictionary<Type, Delegate> m_callbacks;

        public VisualElement()
        {
            m_elementId = ++s_elementCounter;
            hierarchy = new VisualElementHierarchy(this);
        }

        public int childCount => contentContainer == this ? hierarchy.childCount : contentContainer.childCount;

        public VisualElement childAt(int index) => contentContainer == this ? hierarchy.childAt(index) : contentContainer.childAt(index);

        public void Add(VisualElement child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (!hierarchy.CheckSafeState()) return;

            child.parent = this;

            if (contentContainer == this)
            {
                hierarchy.Add(child);
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Add(child);
            }
        }

        public void Remove(VisualElement child)
        {
            if (child == null) return;
            if (!hierarchy.CheckSafeState()) return;

            if (contentContainer == this)
            {
                hierarchy.Remove(child);
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Remove(child);
            }

            if (child.parent == this)
            {
                child.parent = null;
            }
        }

        public void Clear()
        {
            if (!hierarchy.CheckSafeState()) return;

            var childrenToClear = contentContainer == this ? hierarchy.Children() : contentContainer.Children();
            foreach(var child in childrenToClear)
            {
                if (child.parent == this)
                {
                    child.parent = null;
                }
            }

            if (contentContainer == this)
            {
                hierarchy.Clear();
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Clear();
            }
        }

        public IEnumerable<VisualElement> Children()
        {
            return contentContainer == this ? hierarchy.Children() : contentContainer.Children();
        }

        public void RegisterCallback<TEventType>(Action<TEventType> callback) where TEventType : UIEventBase
        {
            if (m_callbacks == null) m_callbacks = new Dictionary<Type, Delegate>();

            Type type = typeof(TEventType);
            if (m_callbacks.TryGetValue(type, out var existing))
            {
                m_callbacks[type] = Delegate.Combine(existing, callback);
            }
            else
            {
                m_callbacks[type] = callback;
            }
        }

        public void UnregisterCallback<TEventType>(Action<TEventType> callback) where TEventType : UIEventBase
        {
            if (m_callbacks == null) return;

            Type type = typeof(TEventType);
            if (m_callbacks.TryGetValue(type, out var existing))
            {
                var newDelegate = Delegate.Remove(existing, callback);
                if (newDelegate == null)
                {
                    m_callbacks.Remove(type);
                }
                else
                {
                    m_callbacks[type] = newDelegate;
                }
            }
        }

        protected void SendEvent(UIEventBase evt)
        {
            evt.source = this;
            EventDispatcher.Enqueue(evt);
        }

        public virtual void InternalRender()
        {
            ImGui.PushID(m_elementId);

            if (pickingMode == PickingMode.Ignore)
            {
                ImGui.SetNextItemAllowOverlap();
            }

            RenderVisualTree();

            bool isSelfHovered = false;

            if (pickingMode != PickingMode.Ignore)
            {
                isSelfHovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
            }

            bool isAnyChildHovered = false;
            int count = hierarchy.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = hierarchy.childAt(i);
                child.InternalRender();
                if (child.m_wasHovered)
                {
                    isAnyChildHovered = true;
                }
            }

            bool isEffectivelyHovered = isSelfHovered || isAnyChildHovered;

            if (isEffectivelyHovered && !m_wasHovered)
            {
                var evt = EventPool<MouseEnterEvent>.Get();
                evt.source = this;
                EventDispatcher.Enqueue(evt);
            }
            else if (!isEffectivelyHovered && m_wasHovered)
            {
                var evt = EventPool<MouseLeaveEvent>.Get();
                evt.source = this;
                EventDispatcher.Enqueue(evt);
            }

            m_wasHovered = isEffectivelyHovered;

            ImGui.PopID();
        }

        protected virtual void RenderVisualTree()
        {
        }

        public virtual void HandleEvent(UIEventBase evt)
        {
            if (m_callbacks != null && m_callbacks.TryGetValue(evt.GetType(), out var callback))
            {
                callback.DynamicInvoke(evt);
            }
        }
    }
}
