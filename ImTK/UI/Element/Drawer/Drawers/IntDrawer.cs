using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(int), allowInheritType: false)]
    public class IntDrawer : FieldDrawer<int>
    {
        public float mouseStep { get; set; } = -1f;

        public int step { get; set; } = 0;

        public IntDrawer()
        {
            m_contentContainer.Add(new FieldElement(this));
        }

        protected override Label CreateLabelElement()
        {
            var labelObj = new NumericDragLabelElement();
            labelObj.onDrag = (delta) => {
                int deltaValue = 1;
                if(this.mouseStep > 0)
                {
                    deltaValue = (int)this.mouseStep;
                }
                
                int dragDelta = delta > 0 ? deltaValue : -deltaValue;
                if (delta == 0) dragDelta = 0;
                
                this.SetValueWithChanged(this.value + dragDelta);
            };
            return labelObj;
        }

        private class FieldElement : VisualElement
        {
            private readonly IntDrawer m_drawer;
            public FieldElement(IntDrawer drawer)
            {
                m_drawer = drawer;
                this.style.flexGrow = 1;
            }

            protected override Vector2 MeasureContent(LayoutConstraint constraint)
            {
                return new Vector2(0, GetFrameHeight());
            }

            public override void OnRender()
            {
                ImGui.SetNextItemWidth(this.layoutRect.width);
                int v = m_drawer.value;

                bool changed = ImGui.InputInt(m_drawer.cachedId, ref v, m_drawer.step, m_drawer.step * 100, ImGuiInputTextFlags.None);

                if (changed || ImGui.IsItemDeactivatedAfterEdit())
                {
                    m_drawer.SetValueWithChanged(v);
                }
            }

        }
    }
}
