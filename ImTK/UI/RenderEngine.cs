using System;
using ImGuiNET;
using ImTK.Core;

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
        [ThreadStatic]
        private static System.Collections.Generic.List<RenderOp> t_tempRenderList;

        public static void ComputeStyleRecursive(VisualElement node)
        {
            if (t_tempRenderList == null) t_tempRenderList = new System.Collections.Generic.List<RenderOp>(32);
            t_tempRenderList.Clear();
            BuildRenderListRecursive(node, t_tempRenderList);
            ComputeStyleFlat(t_tempRenderList);
        }



        private static void BuildRenderListRecursive(VisualElement node, System.Collections.Generic.List<RenderOp> list)
        {
            list.Add(new RenderOp { Element = node, Type = RenderOpType.Begin, SkipCount = 0 });
            int beginIndex = list.Count - 1;

            if (node.resolvedLayoutState.display != DisplayStyle.None)
            {
                int childCount = node.hierarchy.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    BuildRenderListRecursive(node.hierarchy.ChildAt(i), list);
                }
            }

            list.Add(new RenderOp { Element = node, Type = RenderOpType.End, SkipCount = 0 });
            list[beginIndex] = new RenderOp { Element = node, Type = RenderOpType.Begin, SkipCount = (list.Count - 1) - beginIndex - 1 };
        }

        public static void RenderFlat(VisualElement node)
        {
            if (t_tempRenderList == null) t_tempRenderList = new System.Collections.Generic.List<RenderOp>(32);
            t_tempRenderList.Clear();
            BuildRenderListRecursive(node, t_tempRenderList);
            RenderFlat(t_tempRenderList);
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

                    bool shouldRenderChildren = node.OnBeginRender();
                    node.OnRender();

                    if (!shouldRenderChildren && op.SkipCount > 0)
                    {
                        // Skip children, go straight to the End operation
                        i += op.SkipCount;
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
