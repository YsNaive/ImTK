using System;
using ImTK.Core;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(RectInt), allowInheritType: false)]
    public class RectIntDrawer : FieldDrawer<RectInt>
    {
        private RectIntField m_field;

        public RectIntDrawer()
        {
            m_field = new RectIntField("##" + label);
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

        public override void SetValueWithoutNotify(RectInt newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_field.SetValueWithoutNotify(newValue);
        }

        public override RectInt value
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
