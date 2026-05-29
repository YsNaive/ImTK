using Hexa.NET.ImGui;
using ImTK.Core;
using ImTK.Log;
using System;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(float), requiredModifier: typeof(SliderFloatAttribute), allowInheritType: false)]
    public class SliderFloatDrawer : FieldDrawer<float>
    {

        
        public float min { get; private set; } = 0f;
        public float max { get; private set; } = 1f;
        
        public SliderFloatDrawer()
        {
            m_contentContainer.Add(new FieldElement(this));
        }

        public override void ApplyModifier(Attribute modifier)
        {
            base.ApplyModifier(modifier);
            if (modifier is SliderFloatAttribute sliderAttr)
            {
                if (sliderAttr.min >= sliderAttr.max)
                {
                    ImTKLog.Error($"Invalid SliderFloat range: min ({sliderAttr.min}) is greater than or equal to max ({sliderAttr.max}).");
                }
                this.min = sliderAttr.min;
                this.max = sliderAttr.max;
            }
        }

        private class SliderStyle : VisualElement.Style
        {
            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken && prop.key == VisualElement.StyleKey.ColorFamily.Hash)
                {
                    string prefix = "--normal";
                    if (prop.enumValue == (int)ThemeColorFamily.Success) prefix = "--success";
                    else if (prop.enumValue == (int)ThemeColorFamily.Info) prefix = "--info";
                    else if (prop.enumValue == (int)ThemeColorFamily.Warning) prefix = "--warning";
                    else if (prop.enumValue == (int)ThemeColorFamily.Danger) prefix = "--danger";

                    output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.SliderGrab, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-sub-text").Hash });
                    output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.SliderGrabActive, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-text").Hash });
                }
                base.ComputeHighlevelToken(prop, output);
            }
        }

        private class FieldElement : VisualElement<SliderStyle>
        {
            private readonly SliderFloatDrawer m_drawer;
            public FieldElement(SliderFloatDrawer drawer)
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

                bool changed = ImGui.SliderFloat(m_drawer.cachedId, ref v, m_drawer.min, m_drawer.max, "%.3f", ImGuiSliderFlags.None);

                if (changed || ImGui.IsItemDeactivatedAfterEdit())
                {
                    m_drawer.SetValueWithChanged(v);
                }
            }
        }
    }
}
