using System;
using Hexa.NET.ImGui;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Vector3Int), allowInheritType: false)]
    public class Vector3IntDrawer : FieldDrawer<Vector3Int>
    {
        private IntDrawer m_xDrawer;
        private IntDrawer m_yDrawer;
        private IntDrawer m_zDrawer;

        public Vector3IntDrawer()
        {
            m_xDrawer = new IntDrawer() { label = "X", labelWidth = null, iconType = IconElement.IconType.Null };
            m_yDrawer = new IntDrawer() { label = "Y", labelWidth = null, iconType = IconElement.IconType.Null };
            m_zDrawer = new IntDrawer() { label = "Z", labelWidth = null, iconType = IconElement.IconType.Null };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Vector3Int(evt.newValue, m_yDrawer.value, m_zDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Vector3Int(m_xDrawer.value, evt.newValue, m_zDrawer.value)));
            m_zDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Vector3Int(m_xDrawer.value, m_yDrawer.value, evt.newValue)));
            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_xDrawer.style.flexGrow = 1;
            m_yDrawer.style.flexGrow = 1;
            m_zDrawer.style.flexGrow = 1;

            m_contentContainer.Add(m_xDrawer);
            m_contentContainer.Add(m_yDrawer);
            m_contentContainer.Add(m_zDrawer);
        }

        public override void SetValueWithoutNotify(Vector3Int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.x);
            m_yDrawer.SetValueWithoutNotify(newValue.y);
            m_zDrawer.SetValueWithoutNotify(newValue.z);
        }

        public override Vector3Int value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_xDrawer.SetValueWithoutNotify(value.x);
                m_yDrawer.SetValueWithoutNotify(value.y);
                m_zDrawer.SetValueWithoutNotify(value.z);
            }
        }

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }
    }
}