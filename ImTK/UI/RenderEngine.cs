using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public static partial class RenderEngine
    {
        public static void ExecuteLayoutPhase(VisualElement node)
        {
            // Empty shell to be implemented later when Layout Engine is developed.
        }

        public static void RenderNode(VisualElement node)
        {
            if (node.m_isStyleDirty)
            {
                // Handled recursively
                // Handled recursively
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

            if (shouldRenderChildren)
            {
                int count = node.hierarchy.childCount;
                for (int i = 0; i < count; i++)
                {
                    RenderNode(node.hierarchy.ChildAt(i));
                }
            }

            node.OnEndRender();

            bool isSelfHovered = false;
            if (node.pickingMode != PickingMode.Ignore)
            {
                isSelfHovered = node.CheckHoverState();
            }

            bool isAnyChildHovered = false;
            int childCount = node.hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = node.hierarchy.ChildAt(i);
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
