using System;
using ImGuiNET;
using ImTK.Core;
using System.Numerics;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Vector2Int), allowInheritType: false)]
    public class Vector2IntDrawer : FieldDrawer<Vector2Int>
    {
        private IntDrawer m_xDrawer;
        private IntDrawer m_yDrawer;

        public Vector2IntDrawer()
        {
            m_xDrawer = new IntDrawer() { label = "X", labelWidth = null, iconType = IconElement.IconType.Null };
            m_yDrawer = new IntDrawer() { label = "Y", labelWidth = null, iconType = IconElement.IconType.Null };

            m_xDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Vector2Int(evt.newValue, m_yDrawer.value)));
            m_yDrawer.RegisterValueChangedCallback((evt) => SetValueWithChanged(new Vector2Int(m_xDrawer.value, evt.newValue)));
            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_xDrawer.style.flexGrow = 1;
            m_yDrawer.style.flexGrow = 1;

            m_contentContainer.Add(m_xDrawer);
            m_contentContainer.Add(m_yDrawer);
        }

        public override void SetValueWithoutNotify(Vector2Int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_xDrawer.SetValueWithoutNotify(newValue.x);
            m_yDrawer.SetValueWithoutNotify(newValue.y);
        }

        public override Vector2Int value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_xDrawer.SetValueWithoutNotify(value.x);
                m_yDrawer.SetValueWithoutNotify(value.y);
            }
        }

        protected internal override bool CheckHoverState()
        {
            return ImGuiNET.ImGui.IsItemHovered(ImGuiNET.ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }
    }
}