using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public static class RenderEngine
    {
        public static void RenderNode(VisualElement node)
        {
            if (node.m_isStyleDirty)
            {
                node.resolvedStyle.Compute();
                node.m_isStyleDirty = false;
            }

            if (node.m_useAutoId)
            {
                ImGui.PushID(node.m_elementId);
            }

            if (node.pickingMode == PickingMode.Ignore)
            {
                ImGui.SetNextItemAllowOverlap();
            }

            node.internalStyle.PushToImGui(node.resolvedStyle);

            bool shouldRenderChildren = node.OnBeginRender();

            node.OnRender();

            if (shouldRenderChildren)
            {
                int count = node.hierarchy.childCount;
                for (int i = 0; i < count; i++)
                {
                    RenderNode(node.hierarchy.childAt(i));
                }
            }

            node.OnEndRender();

            bool isSelfHovered = false;

            if (node.pickingMode != PickingMode.Ignore)
            {
                isSelfHovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
            }

            bool isAnyChildHovered = false;
            int childCount = node.hierarchy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = node.hierarchy.childAt(i);
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

            node.internalStyle.PopFromImGui();

            if (node.m_useAutoId)
            {
                ImGui.PopID();
            }
        }
    }
}
