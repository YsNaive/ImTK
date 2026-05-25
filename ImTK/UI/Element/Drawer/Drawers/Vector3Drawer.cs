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
            m_xDrawer = new FloatDrawer() { label = "X", labelWidth = null, iconType = IconElement.IconType.Null };
            m_yDrawer = new FloatDrawer() { label = "Y", labelWidth = null, iconType = IconElement.IconType.Null };
            m_zDrawer = new FloatDrawer() { label = "Z", labelWidth = null, iconType = IconElement.IconType.Null };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector3(evt.newValue, m_yDrawer.value, m_zDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector3(m_xDrawer.value, evt.newValue, m_zDrawer.value)));
            m_zDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector3(m_xDrawer.value, m_yDrawer.value, evt.newValue)));
            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_xDrawer.style.flexGrow = 1;
            m_yDrawer.style.flexGrow = 1;
            m_zDrawer.style.flexGrow = 1;

            m_contentContainer.Add(m_xDrawer);
            m_contentContainer.Add(m_yDrawer);
            m_contentContainer.Add(m_zDrawer);
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
    }
}