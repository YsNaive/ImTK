using System;
using ImTK.Core;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Vector3Int), allowInheritType: false)]
    public class Vector3IntDrawer : FieldDrawer<Vector3Int>
    {
        private Vector3IntField m_field;

        public Vector3IntDrawer()
        {
            m_field = new Vector3IntField("##" + label);
            m_field.onValueChanged += (evt) => SetValueWithChanged(evt.newValue);
            hierarchy.Add(m_field);
        }

        public override string label
        {
            get => base.label;
            set
            {
                base.label = value;
                if (m_field != null)
                {
                    m_field.label = "##" + value;
                }
            }
        }

        public override void SetValueWithoutNotify(Vector3Int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_field.SetValueWithoutNotify(newValue);
        }

        public override Vector3Int value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_field.SetValueWithoutNotify(value);
            }
        }

        protected override void OnRenderSelf()
        {
            base.OnRenderSelf();
        }
    }
}
