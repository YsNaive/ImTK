using System;
using Hexa.NET.ImGui;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(RectInt), allowInheritType: false)]
    public class RectIntDrawer : FieldDrawer<RectInt>
    {
        private IntDrawer m_xDrawer;
        private IntDrawer m_yDrawer;
        private IntDrawer m_wDrawer;
        private IntDrawer m_hDrawer;

        public RectIntDrawer()
        {
            m_xDrawer = new IntDrawer() { label = "X", labelWidth = null, iconType = IconElement.IconType.Null };
            m_yDrawer = new IntDrawer() { label = "Y", labelWidth = null, iconType = IconElement.IconType.Null };
            m_wDrawer = new IntDrawer() { label = "W", labelWidth = null, iconType = IconElement.IconType.Null };
            m_hDrawer = new IntDrawer() { label = "H", labelWidth = null, iconType = IconElement.IconType.Null };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new RectInt(evt.newValue, m_yDrawer.value, m_wDrawer.value, m_hDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new RectInt(m_xDrawer.value, evt.newValue, m_wDrawer.value, m_hDrawer.value)));
            m_wDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new RectInt(m_xDrawer.value, m_yDrawer.value, evt.newValue, m_hDrawer.value)));
            m_hDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new RectInt(m_xDrawer.value, m_yDrawer.value, m_wDrawer.value, evt.newValue)));
            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_xDrawer.style.flexGrow = 1;
            m_yDrawer.style.flexGrow = 1;
            m_wDrawer.style.flexGrow = 1;
            m_hDrawer.style.flexGrow = 1;

            m_contentContainer.Add(m_xDrawer);
            m_contentContainer.Add(m_yDrawer);
            m_contentContainer.Add(m_wDrawer);
            m_contentContainer.Add(m_hDrawer);
        }

        public override void SetValueWithoutNotify(RectInt newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.x);
            m_yDrawer.SetValueWithoutNotify(newValue.y);
            m_wDrawer.SetValueWithoutNotify(newValue.width);
            m_hDrawer.SetValueWithoutNotify(newValue.height);
        }

        public override RectInt value
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

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }
    }
}