using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(float), allowInheritType: false)]
    public class FloatDrawer : FieldDrawer<float>
    {
        public float mouseStep { get; set; } = -1f;

        public float step { get; set; } = 0f;

        public FloatDrawer()
        {
            m_contentContainer.Add(new FieldElement(this));
        }

        protected override Label CreateLabelElement()
        {
            var labelObj = new NumericDragLabelElement();
            labelObj.onDrag = (delta) => {
                float deltaValue = 0.01f;
                if(this.mouseStep > 0)
                {
                    deltaValue = this.mouseStep;
                }
                
                float dragDelta = delta * deltaValue;
                if (delta == 0) dragDelta = 0;
                
                this.SetValueWithChanged(this.value + dragDelta);
            };
            return labelObj;
        }

        private class FieldElement : VisualElement
        {
            private readonly FloatDrawer m_drawer;
            public FieldElement(FloatDrawer drawer)
            {
                m_drawer = drawer;
                this.style.flexGrow = 1;
            }

            protected override Vector2 MeasureContent(LayoutConstraint constraint)
            {
                return new Vector2(0, ImGui.GetFrameHeight());
            }

            public override void OnRender()
            {
                ImGui.SetNextItemWidth(this.layoutRect.width);
                float v = m_drawer.value;

                bool changed = ImGui.InputFloat(m_drawer.cachedId, ref v, m_drawer.step, m_drawer.step * 100f, "%.3f", ImGuiInputTextFlags.None);

                if (changed || ImGui.IsItemDeactivatedAfterEdit())
                {
                    m_drawer.SetValueWithChanged(v);
                }
            }

        }
    }
}
