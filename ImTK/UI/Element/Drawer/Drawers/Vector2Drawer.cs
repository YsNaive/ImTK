using System;
using ImGuiNET;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(System.Numerics.Vector2), allowInheritType: false)]
    public class Vector2Drawer : FieldDrawer<System.Numerics.Vector2>
    {
        private FloatDrawer m_xDrawer;
        private FloatDrawer m_yDrawer;

        public Vector2Drawer()
        {
            m_xDrawer = new FloatDrawer() { label = "X" };
            m_yDrawer = new FloatDrawer() { label = "Y" };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector2(evt.newValue, m_yDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector2(m_xDrawer.value, evt.newValue)));

            // We do not add them to hierarchy because we manually call Render on them with overrideRenderRect
        }

        public override void SetValueWithoutNotify(System.Numerics.Vector2 newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.X);
            m_yDrawer.SetValueWithoutNotify(newValue.Y);
        }

        public override System.Numerics.Vector2 value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_xDrawer.SetValueWithoutNotify(value.X);
                m_yDrawer.SetValueWithoutNotify(value.Y);
            }
        }

        protected override void OnRenderLayout()
        {
            float currentLabelWidth = labelWidth.Value;
            float frameHeight = ImGui.GetFrameHeight();
            float iconSize = frameHeight * 0.8f;
            float yOffset = (frameHeight - iconSize) * 0.5f;

            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Rect iconRect = new Rect(
                new Vector2(cursorPos.X, cursorPos.Y + yOffset),
                new Vector2(iconSize, iconSize)
            );

            ImGui.Dummy(new Vector2(iconSize, frameHeight));
            OnRenderIcon(ImGui.GetWindowDrawList(), iconRect);

            if (layoutMode == DrawerLayoutMode.Inline)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                if (!string.IsNullOrEmpty(label))
                {
                    float currentX = ImGui.GetCursorPosX();
                    float targetX = currentLabelWidth;
                    if (currentX < targetX)
                    {
                        ImGui.SameLine(targetX);
                    }
                    else
                    {
                        ImGui.SameLine();
                    }
                }
                else
                {
                    ImGui.SameLine();
                }

                float availableWidth = ImGui.GetContentRegionAvail().X;
                float itemSpacing = ImGui.GetStyle().ItemInnerSpacing.X;
                float childWidth = (availableWidth - itemSpacing) / 2f;
                Vector2 startPos = ImGui.GetCursorScreenPos();

                m_xDrawer.overrideRenderRect = new Rect(startPos.X, startPos.Y, childWidth, frameHeight);
                m_xDrawer.Render();

                m_yDrawer.overrideRenderRect = new Rect(startPos.X + childWidth + itemSpacing, startPos.Y, childWidth, frameHeight);
                m_yDrawer.Render();

                // Move cursor to the next line since FloatDrawer Render with overrideRenderRect doesn't do it automatically in the same way
                ImGui.SetCursorScreenPos(new Vector2(cursorPos.X, cursorPos.Y + frameHeight + ImGui.GetStyle().ItemSpacing.Y));
            }
            else if (layoutMode == DrawerLayoutMode.Expand)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                float indent = iconSize + ImGui.GetStyle().ItemInnerSpacing.X;
                ImGui.Indent(indent);

                // For Expand mode, we still render them horizontally on a new line
                float availableWidth = ImGui.GetContentRegionAvail().X;
                float itemSpacing = ImGui.GetStyle().ItemInnerSpacing.X;
                float childWidth = (availableWidth - itemSpacing) / 2f;
                Vector2 startPos = ImGui.GetCursorScreenPos();

                m_xDrawer.overrideRenderRect = new Rect(startPos.X, startPos.Y, childWidth, frameHeight);
                m_xDrawer.Render();

                m_yDrawer.overrideRenderRect = new Rect(startPos.X + childWidth + itemSpacing, startPos.Y, childWidth, frameHeight);
                m_yDrawer.Render();

                ImGui.SetCursorScreenPos(new Vector2(cursorPos.X, startPos.Y + frameHeight + ImGui.GetStyle().ItemSpacing.Y));

                ImGui.Unindent(indent);
            }
        }
    }
}
