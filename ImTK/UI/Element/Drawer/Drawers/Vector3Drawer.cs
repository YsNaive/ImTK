using System;
using ImGuiNET;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(System.Numerics.Vector3), allowInheritType: false)]
    public class Vector3Drawer : FieldDrawer<System.Numerics.Vector3>
    {
        private FloatDrawer m_xDrawer;
        private FloatDrawer m_yDrawer;
        private FloatDrawer m_zDrawer;

        public Vector3Drawer()
        {
            m_xDrawer = new FloatDrawer() { label = "X" };
            m_yDrawer = new FloatDrawer() { label = "Y" };
            m_zDrawer = new FloatDrawer() { label = "Z" };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector3(evt.newValue, m_yDrawer.value, m_zDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector3(m_xDrawer.value, evt.newValue, m_zDrawer.value)));
            m_zDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector3(m_xDrawer.value, m_yDrawer.value, evt.newValue)));
        }

        public override void SetValueWithoutNotify(System.Numerics.Vector3 newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.X);
            m_yDrawer.SetValueWithoutNotify(newValue.Y);
            m_zDrawer.SetValueWithoutNotify(newValue.Z);
        }

        public override System.Numerics.Vector3 value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_xDrawer.SetValueWithoutNotify(value.X);
                m_yDrawer.SetValueWithoutNotify(value.Y);
                m_zDrawer.SetValueWithoutNotify(value.Z);
            }
        }

        protected override void OnRenderSelf()
        {
            float availableWidth = ImGuiNET.ImGui.GetContentRegionAvail().X;
            float itemSpacing = ImGuiNET.ImGui.GetStyle().ItemInnerSpacing.X;
            float childWidth = (availableWidth - itemSpacing * 2) / 3.0f;
            float frameHeight = ImGuiNET.ImGui.GetFrameHeight();
            System.Numerics.Vector2 startPos = ImGuiNET.ImGui.GetCursorScreenPos();

            m_xDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 0 + itemSpacing * 0, startPos.Y, childWidth, frameHeight);
            m_xDrawer.Render();

            m_yDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 1 + itemSpacing * 1, startPos.Y, childWidth, frameHeight);
            m_yDrawer.Render();

            m_zDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 2 + itemSpacing * 2, startPos.Y, childWidth, frameHeight);
            m_zDrawer.Render();

            ImGuiNET.ImGui.SetCursorScreenPos(new System.Numerics.Vector2(startPos.X, startPos.Y + frameHeight + ImGuiNET.ImGui.GetStyle().ItemSpacing.Y));
        }
    }
}