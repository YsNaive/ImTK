using System;
using ImTK.Core;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Vector2Int), allowInheritType: false)]
    public class Vector2IntDrawer : FieldDrawer<Vector2Int>
    {
        private Vector2IntField m_field;

        public Vector2IntDrawer()
        {
            m_field = new Vector2IntField("##" + label);
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

        public override void SetValueWithoutNotify(Vector2Int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_field.SetValueWithoutNotify(newValue);
        }

        public override Vector2Int value
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
