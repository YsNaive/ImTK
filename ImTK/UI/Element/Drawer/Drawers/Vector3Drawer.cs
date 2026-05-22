using System;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(System.Numerics.Vector3), allowInheritType: false)]
    public class Vector3Drawer : FieldDrawer<System.Numerics.Vector3>
    {
        private Vector3Field m_field;

        public Vector3Drawer()
        {
            m_field = new Vector3Field("##" + label);
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

        public override void SetValueWithoutNotify(System.Numerics.Vector3 newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_field.SetValueWithoutNotify(newValue);
        }

        public override System.Numerics.Vector3 value
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
