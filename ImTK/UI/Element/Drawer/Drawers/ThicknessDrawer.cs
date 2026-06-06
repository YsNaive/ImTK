using System;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Thickness), allowInheritType: false)]
    public class ThicknessDrawer : FieldDrawer<Thickness>
    {
        private FloatDrawer m_lDrawer;
        private FloatDrawer m_tDrawer;
        private FloatDrawer m_rDrawer;
        private FloatDrawer m_bDrawer;

        public ThicknessDrawer()
        {
            m_lDrawer = new FloatDrawer() { label = "L", labelWidth = null, iconType = IconElement.IconType.Null };
            m_tDrawer = new FloatDrawer() { label = "T", labelWidth = null, iconType = IconElement.IconType.Null };
            m_rDrawer = new FloatDrawer() { label = "R", labelWidth = null, iconType = IconElement.IconType.Null };
            m_bDrawer = new FloatDrawer() { label = "B", labelWidth = null, iconType = IconElement.IconType.Null };

            m_lDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Thickness(evt.newValue, m_tDrawer.value, m_rDrawer.value, m_bDrawer.value)));
            m_tDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Thickness(m_lDrawer.value, evt.newValue, m_rDrawer.value, m_bDrawer.value)));
            m_rDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Thickness(m_lDrawer.value, m_tDrawer.value, evt.newValue, m_bDrawer.value)));
            m_bDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Thickness(m_lDrawer.value, m_tDrawer.value, m_rDrawer.value, evt.newValue)));
            
            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_contentContainer.style.flexWrap = FlexWrap.NoWrap;
            m_lDrawer.style.flexGrow = 1;
            m_tDrawer.style.flexGrow = 1;
            m_rDrawer.style.flexGrow = 1;
            m_bDrawer.style.flexGrow = 1;

            m_contentContainer.Add(m_lDrawer);
            m_contentContainer.Add(m_tDrawer);
            m_contentContainer.Add(m_rDrawer);
            m_contentContainer.Add(m_bDrawer);
        }

        public override void SetValueWithoutNotify(Thickness newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_lDrawer.SetValueWithoutNotify(newValue.left);
            m_tDrawer.SetValueWithoutNotify(newValue.top);
            m_rDrawer.SetValueWithoutNotify(newValue.right);
            m_bDrawer.SetValueWithoutNotify(newValue.bottom);
        }

        public override Thickness value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_lDrawer.SetValueWithoutNotify(value.left);
                m_tDrawer.SetValueWithoutNotify(value.top);
                m_rDrawer.SetValueWithoutNotify(value.right);
                m_bDrawer.SetValueWithoutNotify(value.bottom);
            }
        }

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }
    }
}
