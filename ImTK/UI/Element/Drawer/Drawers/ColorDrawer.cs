using Hexa.NET.ImGui;
using ImTK.Core;
using ImTK.Log;
using System;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Color), allowInheritType: false)]
    public class ColorDrawer : FieldDrawer<Color>
    {
        public bool showInputs { get; set; } = false;
        public bool enableAlpha { get; set; } = true;

        public ColorDrawer()
        {
            m_contentContainer.Add(new FieldElement(this));
        }

        private class FieldElement : VisualElement
        {
            private readonly ColorDrawer m_drawer;
            public FieldElement(ColorDrawer drawer)
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

                ImGuiColorEditFlags flags = ImGuiColorEditFlags.None;
                if (!m_drawer.showInputs)
                {
                    flags |= ImGuiColorEditFlags.NoInputs;
                }

                if (!m_drawer.enableAlpha)
                {
                    flags |= ImGuiColorEditFlags.NoAlpha;
                }
                else
                {
                    flags |= ImGuiColorEditFlags.AlphaPreviewHalf;
                }

                Vector4 v = m_drawer.value.rgba;

                bool changed = ImGui.ColorEdit4(m_drawer.cachedId, ref v, flags);

                if (changed || ImGui.IsItemDeactivatedAfterEdit())
                {
                    ImTKLog.Trace($"ColorDrawer changed! New color: {v}");
                    m_drawer.SetValueWithChanged(new Color(v));
                }
            }
        }
    }
}
