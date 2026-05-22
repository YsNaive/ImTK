using System;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(System.Numerics.Vector2), allowInheritType: false)]
    public class Vector2Drawer : FieldDrawer<System.Numerics.Vector2>
    {
        private Vector2Field m_field;

        public Vector2Drawer()
        {
            m_field = new Vector2Field("##" + label);
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

        public override void SetValueWithoutNotify(System.Numerics.Vector2 newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_field.SetValueWithoutNotify(newValue);
        }

        public override System.Numerics.Vector2 value
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
