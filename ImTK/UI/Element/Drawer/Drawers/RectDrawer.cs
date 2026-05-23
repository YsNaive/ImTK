using System;
using ImGuiNET;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Rect), allowInheritType: false)]
    public class RectDrawer : FieldDrawer<Rect>
    {
        private FloatDrawer m_xDrawer;
        private FloatDrawer m_yDrawer;
        private FloatDrawer m_wDrawer;
        private FloatDrawer m_hDrawer;

        public RectDrawer()
        {
            m_xDrawer = new FloatDrawer() { label = "X" };
            m_yDrawer = new FloatDrawer() { label = "Y" };
            m_wDrawer = new FloatDrawer() { label = "W" };
            m_hDrawer = new FloatDrawer() { label = "H" };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Rect(evt.newValue, m_yDrawer.value, m_wDrawer.value, m_hDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Rect(m_xDrawer.value, evt.newValue, m_wDrawer.value, m_hDrawer.value)));
            m_wDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Rect(m_xDrawer.value, m_yDrawer.value, evt.newValue, m_hDrawer.value)));
            m_hDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Rect(m_xDrawer.value, m_yDrawer.value, m_wDrawer.value, evt.newValue)));
        }

        public override void SetValueWithoutNotify(Rect newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.x);
            m_yDrawer.SetValueWithoutNotify(newValue.y);
            m_wDrawer.SetValueWithoutNotify(newValue.width);
            m_hDrawer.SetValueWithoutNotify(newValue.height);
        }

        public override Rect value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_xDrawer.SetValueWithoutNotify(value.x);
                m_yDrawer.SetValueWithoutNotify(value.y);
                m_wDrawer.SetValueWithoutNotify(value.width);
                m_hDrawer.SetValueWithoutNotify(value.height);
            }
        }

        public override void OnRender()
        {
            float availableWidth = ImGuiNET.ImGui.GetContentRegionAvail().X;
            float itemSpacing = ImGuiNET.ImGui.GetStyle().ItemInnerSpacing.X;
            float childWidth = (availableWidth - itemSpacing * 3) / 4.0f;
            float frameHeight = ImGuiNET.ImGui.GetFrameHeight();
            System.Numerics.Vector2 startPos = ImGuiNET.ImGui.GetCursorScreenPos();

            // Submit a Dummy to allocate the space and advance the ImGui layout engine safely
            ImGuiNET.ImGui.Dummy(new System.Numerics.Vector2(availableWidth, frameHeight));

            // The cursor has advanced to the next line. We don't need to manually reset it to the next line at the end.
            // The absolute layout rendering of our child components will just draw over the Dummy area.
            m_xDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 0 + itemSpacing * 0, startPos.Y, childWidth, frameHeight);
            RenderEngine.RenderNode(m_xDrawer);

            m_yDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 1 + itemSpacing * 1, startPos.Y, childWidth, frameHeight);
            RenderEngine.RenderNode(m_yDrawer);

            m_wDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 2 + itemSpacing * 2, startPos.Y, childWidth, frameHeight);
            RenderEngine.RenderNode(m_wDrawer);

            m_hDrawer.overrideRenderRect = new ImTK.Core.Rect(startPos.X + childWidth * 3 + itemSpacing * 3, startPos.Y, childWidth, frameHeight);
            RenderEngine.RenderNode(m_hDrawer);

        }
    }
}