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

        protected override void OnRenderSelf()
        {
            float availableWidth = ImGuiNET.ImGui.GetContentRegionAvail().X;
            float itemSpacing = ImGuiNET.ImGui.GetStyle().ItemInnerSpacing.X;
            float childWidth = (availableWidth - itemSpacing * 1) / 2.0f;
            float frameHeight = ImGuiNET.ImGui.GetFrameHeight();
            System.Numerics.Vector2 startPos = ImGuiNET.ImGui.GetCursorScreenPos();

            // Submit a Dummy to allocate the space and advance the ImGui layout engine safely
            ImGuiNET.ImGui.Dummy(new System.Numerics.Vector2(availableWidth, frameHeight));

            // The cursor has advanced to the next line. We don't need to manually reset it to the next line at the end.
            // The absolute layout rendering of our child components will just draw over the Dummy area.
            m_xDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 0 + itemSpacing * 0, startPos.Y, childWidth, frameHeight);
            m_xDrawer.Render();

            m_yDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 1 + itemSpacing * 1, startPos.Y, childWidth, frameHeight);
            m_yDrawer.Render();

        }
    }
}