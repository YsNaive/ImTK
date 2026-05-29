using Hexa.NET.ImGui;
using ImTK.Core;
using ImTK.Log;
using System;
using System.Runtime.CompilerServices;

namespace ImTK.UI
{
    public enum RenderOpType : byte
    {
        Begin,
        End
    }

    public struct RenderOp
    {
        public VisualElement Element;
        public RenderOpType Type;
        public int SkipCount;
    }

    public static partial class RenderEngine
    {
        private static readonly ConditionalWeakTable<VisualElement, RenderListCache> s_renderCaches = new();

        public static void Render(VisualElement root)
        {
            if (!s_renderCaches.TryGetValue(root, out var cache))
            {
                cache = new RenderListCache();
                s_renderCaches.Add(root, cache);
            }

            if (cache.isDirty)
            {
                using (ImTKProfiler.Scope("Persistent/"+ root.GetType().Name))
                {
                    cache.Update(root);
                    string persistId = root.persistenceKey;
                    if (!string.IsNullOrEmpty(persistId))
                    {
                        Persistence.ViewStatePersister.LoadNewStates(persistId, cache.renderList);
                    }
                }
            }
            
            using (ImTKProfiler.Scope("Render/ComputeStyle/"+root.GetType().Name))
            {
                ComputeStyleFlat(cache.renderList);
            }
            
            using (ImTKProfiler.Scope("Render/Render/" + root.GetType().Name))
            {
                RenderFlat(cache.renderList);
            }
        }

        internal static void MarkRenderDirty(VisualElement element)
        {
            VisualElement current = element;
            while (current != null)
            {
                if (s_renderCaches.TryGetValue(current, out var cache))
                {
                    cache.MarkDirty();
                }
                if (current is IRenderRoot)
                {
                    break;
                }
                current = current.parent;
            }
        }

        public static void SaveAllPersistentStates()
        {
            var statesToSave = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<RenderOp>>();
            foreach (var kvp in s_renderCaches)
            {
                VisualElement root = kvp.Key;
                RenderListCache cache = kvp.Value;
                if (cache.renderList == null || cache.renderList.Count == 0) continue;

                string persistId = root.persistenceKey;
                if (!string.IsNullOrEmpty(persistId))
                {
                    statesToSave[persistId] = cache.renderList;
                }
            }
            if (statesToSave.Count > 0)
            {
                Persistence.ViewStatePersister.SaveAllStates(statesToSave);
            }
        }

        internal static RenderListCache GetRenderCache(VisualElement root)
        {
            s_renderCaches.TryGetValue(root, out var cache);
            return cache;
        }
        /// <summary>
        /// Manages the dynamic rendering state during the VisualElement layout and render pass.
        /// Helps defer commands that require an active ImGui Window and tracks inherited state (like Font Families).
        /// </summary>
        public static class Context
        {
            // --- Font State Tracking ---
            private static readonly System.Collections.Generic.Stack<int> s_fontFamilyHashStack = new System.Collections.Generic.Stack<int>();

            public static int CurrentFontFamilyHash
            {
                get
                {
                    if (s_fontFamilyHashStack.Count > 0)
                    {
                        return s_fontFamilyHashStack.Peek();
                    }
                    return ImTKTheme.GlobalTheme.fontFamilyHash;
                }
            }

            public static void PushFontState(int familyHash)
            {
                s_fontFamilyHashStack.Push(familyHash);
            }

            public static void PopFontState()
            {
                if (s_fontFamilyHashStack.Count > 0)
                {
                    s_fontFamilyHashStack.Pop();
                }
            }

            // --- Window Scoped Commands ---
            private static bool s_isInsideWindow = false;
            private static readonly System.Collections.Generic.Queue<Action> s_pendingWindowCommands = new System.Collections.Generic.Queue<Action>();

            public static float CurrentDpiScale { get; set; } = 1.0f;

            public static bool IsInsideWindow
            {
                get => s_isInsideWindow;
                set => s_isInsideWindow = value;
            }

            /// <summary>
            /// Enqueues a command that must be executed inside an ImGui.Begin() / ImGui.End() block.
            /// If currently inside a window, it executes immediately.
            /// </summary>
            public static void EnqueueWindowCommand(Action command)
            {
                if (s_isInsideWindow)
                {
                    command?.Invoke();
                }
                else
                {
                    s_pendingWindowCommands.Enqueue(command);
                }
            }

            /// <summary>
            /// Flushes all pending window commands. Called by Window.Begin().
            /// </summary>
            public static void FlushPendingCommands()
            {
                while (s_pendingWindowCommands.Count > 0)
                {
                    var command = s_pendingWindowCommands.Dequeue();
                    command?.Invoke();
                }
            }

            /// <summary>
            /// Clears context state. Useful for resetting state between frames or on application close.
            /// </summary>
            public static void Reset()
            {
                s_fontFamilyHashStack.Clear();
                s_pendingWindowCommands.Clear();
                s_isInsideWindow = false;
                CurrentDpiScale = 1.0f;
            }
        }

        [ThreadStatic]
        private static System.Collections.Generic.Stack<System.Collections.Generic.List<RenderOp>> t_listPool;

        private static System.Collections.Generic.List<RenderOp> GetList()
        {
            if (t_listPool == null) t_listPool = new System.Collections.Generic.Stack<System.Collections.Generic.List<RenderOp>>();
            if (t_listPool.Count > 0) return t_listPool.Pop();
            return new System.Collections.Generic.List<RenderOp>(32);
        }

        private static void ReleaseList(System.Collections.Generic.List<RenderOp> list)
        {
            list.Clear();
            t_listPool.Push(list);
        }

        public static void ComputeStyleRecursive(VisualElement node)
        {
            var list = GetList();
            BuildRenderListRecursive(node, list);
            ComputeStyleFlat(list);
            ReleaseList(list);
        }



        internal static void BuildRenderListRecursive(VisualElement node, System.Collections.Generic.List<RenderOp> list)
        {
            list.Add(new RenderOp { Element = node, Type = RenderOpType.Begin, SkipCount = 0 });
            int beginIndex = list.Count - 1;

            int childCount = node.hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                BuildRenderListRecursive(node.hierarchy.ChildAt(i), list);
            }

            list.Add(new RenderOp { Element = node, Type = RenderOpType.End, SkipCount = 0 });
            list[beginIndex] = new RenderOp { Element = node, Type = RenderOpType.Begin, SkipCount = (list.Count - 1) - beginIndex - 1 };
        }

        public static void RenderFlat(VisualElement node)
        {
            var list = GetList();
            BuildRenderListRecursive(node, list);
            RenderFlat(list);
            ReleaseList(list);
        }

        public static void RenderFlat(System.Collections.Generic.List<RenderOp> renderList)
        {
            for (int i = 0; i < renderList.Count; i++)
            {
                var op = renderList[i];
                var node = op.Element;

                if (op.Type == RenderOpType.Begin)
                {
                    if (node.resolvedLayoutState.display == DisplayStyle.None)
                    {
                        if (node.m_wasHovered)
                        {
                            var evt = EventPool<MouseLeaveEvent>.Get();
                            evt.source = node;
                            EventDispatcher.Enqueue(evt);
                            node.m_wasHovered = false;
                        }
                        
                        i += op.SkipCount + 1;
                        continue;
                    }

                    if (node.m_useAutoId)
                    {
                        ImGui.PushID(node.m_elementId);
                    }

                    if (node.pickingMode == PickingMode.Ignore)
                    {
                        ImGui.SetNextItemAllowOverlap();
                    }

                    node.requiredStyle.Push();

                    try
                    {
                        bool shouldRenderChildren = node.OnBeginRender();
                        node.OnRender();

                        if (!shouldRenderChildren && op.SkipCount > 0)
                        {
                            // Skip children, go straight to the End operation
                            i += op.SkipCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        ImTKLog.Error(ex, $"Exception during Begin/Render of {node.GetType().Name}");
                        if (op.SkipCount > 0)
                        {
                            i += op.SkipCount; // Skip children to prevent cascading errors
                        }
                    }
                }
                else // RenderOpType.End
                {
                    node.OnEndRender();

                    bool isSelfHovered = false;
                    if (node.pickingMode != PickingMode.Ignore)
                    {
                        isSelfHovered = node.CheckHoverState();
                    }

                    bool isAnyChildHovered = false;
                    int childCount = node.hierarchy.childCount;
                    for (int j = 0; j < childCount; j++)
                    {
                        var child = node.hierarchy.ChildAt(j);
                        if (child.m_wasHovered)
                        {
                            isAnyChildHovered = true;
                        }
                    }

                    bool isEffectivelyHovered = isSelfHovered || isAnyChildHovered;

                    if (isEffectivelyHovered && !node.m_wasHovered)
                    {
                        var evt = EventPool<MouseEnterEvent>.Get();
                        evt.source = node;
                        EventDispatcher.Enqueue(evt);
                    }
                    else if (!isEffectivelyHovered && node.m_wasHovered)
                    {
                        var evt = EventPool<MouseLeaveEvent>.Get();
                        evt.source = node;
                        EventDispatcher.Enqueue(evt);
                    }

                    node.m_wasHovered = isEffectivelyHovered;

                    node.requiredStyle.Pop();

                    if (node.m_useAutoId)
                    {
                        ImGui.PopID();
                    }
                }
            }
        }
    }
}
