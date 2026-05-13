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
        protected bool m_useAutoId = true;

        private Dictionary<Type, Delegate> m_callbacks;

        public VisualElementStyle style { get; } = new VisualElementStyle();

        public VisualElement()
        {
            m_elementId = ++s_elementCounter;
            hierarchy = new VisualElementHierarchy(this);
        }

        public void SetTheme(ImTKTheme theme)
        {
            ApplyTheme(theme);
            for (int i = 0; i < hierarchy.childCount; i++)
            {
                hierarchy.childAt(i).SetTheme(theme);
            }
        }

        protected virtual void ApplyTheme(ImTKTheme theme)
        {
            style.ClearThemeStyles();
            style.ApplyThemeColor(ImGuiCol.Text, theme.TextPrimary);
            // Default background mapping could be to Background2 for normal elements
            // Let derived classes like Window handle their own specific backgrounds if needed
        }

        public NodeType GetNodeType()
        {
            bool hasLogicalParent = this.parent != null;
            bool hasPhysicalParent = this.hierarchy.parent != null;

            if (!hasLogicalParent && !hasPhysicalParent) return NodeType.None;
            if (hasLogicalParent && hasPhysicalParent) return NodeType.LogicNode;
            if (!hasLogicalParent && hasPhysicalParent) return NodeType.PhysicsNode;
            return NodeType.Invalid;
        }

        public int childCount => contentContainer == this ? hierarchy.childCount : contentContainer.childCount;

        public VisualElement childAt(int index) => contentContainer == this ? hierarchy.childAt(index) : contentContainer.childAt(index);

        public void Add(VisualElement child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (!hierarchy.CheckSafeState()) return;

            NodeType type = child.GetNodeType();
            if (type == NodeType.LogicNode)
            {
                child.parent.Remove(child);
            }
            else if (type == NodeType.PhysicsNode)
            {
                child.hierarchy.parent.hierarchy.Remove(child);
            }

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

            EventDispatcher.MarkHierarchyDirty(this);
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

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public void Clear()
        {
            if (!hierarchy.CheckSafeState()) return;

            // Collect children to avoid modifying while iterating, just in case physical Clear has side effects
            var childrenToClear = new List<VisualElement>(contentContainer == this ? hierarchy.Children() : contentContainer.Children());
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

            EventDispatcher.MarkHierarchyDirty(this);
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

        // REMOVED 'virtual' to ensure protection. Kept 'public' temporarily for Test access, or use InternalsVisibleTo.
        // As requested before, we'll keep it 'internal' but we need the Test module to access it.
        /// <summary>
        /// 觸發元件渲染的公開入口點。
        /// 負責處理防護層邏輯 (PushID/PopID、MouseHover推導、事件分派)，並呼叫 OnRenderLayout。
        /// 不可被覆寫，子類別應實作 OnRenderLayout 或 OnRenderSelf。
        /// </summary>
        public void Render()
        {
            if (m_useAutoId)
            {
                ImGui.PushID(m_elementId);
            }

            if (pickingMode == PickingMode.Ignore)
            {
                ImGui.SetNextItemAllowOverlap();
            }

            int pushedColors = 0;
            int pushedVars = 0;

            // Helper to push a style entry
            void PushStyleEntry(StyleEntry entry)
            {
                if (entry.Type == StyleVarType.Color)
                {
                    ImGui.PushStyleColor((ImGuiCol)entry.Key, entry.ColorValue);
                    pushedColors++;
                }
                else if (entry.Type == StyleVarType.Float)
                {
                    ImGui.PushStyleVar((ImGuiStyleVar)entry.Key, entry.FloatValue);
                    pushedVars++;
                }
                else if (entry.Type == StyleVarType.Vector2)
                {
                    ImGui.PushStyleVar((ImGuiStyleVar)entry.Key, entry.Vector2Value);
                    pushedVars++;
                }
            }

            // 1. Push Theme styles
            if (style.m_themeStyles != null)
            {
                for (int i = 0; i < style.m_themeStyles.Count; i++)
                {
                    var themeEntry = style.m_themeStyles[i];
                    // Only push if it is NOT overridden
                    bool isOverridden = false;
                    if (style.m_overrideStyles != null)
                    {
                        for (int j = 0; j < style.m_overrideStyles.Count; j++)
                        {
                            if (style.m_overrideStyles[j].Type == themeEntry.Type && style.m_overrideStyles[j].Key == themeEntry.Key)
                            {
                                isOverridden = true;
                                break;
                            }
                        }
                    }

                    if (!isOverridden)
                    {
                        PushStyleEntry(themeEntry);
                    }
                }
            }

            // 2. Push Override styles
            if (style.m_overrideStyles != null)
            {
                for (int i = 0; i < style.m_overrideStyles.Count; i++)
                {
                    PushStyleEntry(style.m_overrideStyles[i]);
                }
            }

            OnRenderLayout();

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

            if (pushedColors > 0)
            {
                ImGui.PopStyleColor(pushedColors);
            }

            if (pushedVars > 0)
            {
                ImGui.PopStyleVar(pushedVars);
            }

            if (m_useAutoId)
            {
                ImGui.PopID();
            }
        }

        protected virtual void OnRenderLayout()
        {
            OnRenderSelf();

            int count = hierarchy.childCount;
            for (int i = 0; i < count; i++)
            {
                hierarchy.childAt(i).Render();
            }
        }

        protected virtual void OnRenderSelf()
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
