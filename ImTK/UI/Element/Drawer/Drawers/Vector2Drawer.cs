using System;
using Hexa.NET.ImGui;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(System.Numerics.Vector2), allowInheritType: false)]
    public class Vector2Drawer : FieldDrawer<System.Numerics.Vector2>
    {
        private FloatDrawer m_xDrawer;
        private FloatDrawer m_yDrawer;

        public Vector2Drawer()
        {
            m_xDrawer = new FloatDrawer() { label = "x", labelWidth = null, iconType = IconElement.IconType.Null };
            m_yDrawer = new FloatDrawer() { label = "y", labelWidth = null, iconType = IconElement.IconType.Null };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector2(evt.newValue, m_yDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new System.Numerics.Vector2(m_xDrawer.value, evt.newValue)));

            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_contentContainer.style.flexWrap = FlexWrap.NoWrap;
            m_xDrawer.style.flexGrow = 1;
            m_yDrawer.style.flexGrow = 1;

            m_contentContainer.Add(m_xDrawer);
            m_contentContainer.Add(m_yDrawer);
        }

        public override void SetValueWithoutNotify(System.Numerics.Vector2 newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.X);
            m_yDrawer.SetValueWithoutNotify(newValue.Y);
        }

        public override System.Numerics.Vector2 value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_xDrawer.SetValueWithoutNotify(value.X);
                m_yDrawer.SetValueWithoutNotify(value.Y);
            }
        }
    }
}