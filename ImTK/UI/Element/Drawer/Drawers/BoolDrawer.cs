using System;
using System.Numerics;
using ImGuiNET;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(bool), allowInheritType: false)]
    public class BoolDrawer : FieldDrawer<bool>
    {
        public BoolDrawer()
        {
            m_contentContainer.Add(new FieldElement(this));
        }

        private class FieldElement : VisualElement
        {
            private readonly BoolDrawer m_drawer;
            public FieldElement(BoolDrawer drawer)
            {
                m_drawer = drawer;
                this.style.flexGrow = 1;
            }

            protected override Vector2 MeasureContent(LayoutConstraint constraint)
            {
                return new Vector2(0, ImGuiNET.ImGui.GetFrameHeight());
            }

            public override void OnRender()
            {
                ImGuiNET.ImGui.SetNextItemWidth(this.layoutRect.width);
                bool v = m_drawer.value;
                if (ImGuiNET.ImGui.Checkbox("##" + m_drawer.label, ref v))
                {
                    m_drawer.SetValueWithChanged(v);
                }
            }

        }
    }
}
