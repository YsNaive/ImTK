using System;
using ImTK.Core;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Rect), allowInheritType: false)]
    public class RectDrawer : FieldDrawer<Rect>
    {
        private RectField m_field;

        public RectDrawer()
        {
            m_field = new RectField("##" + label);
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

        public override void SetValueWithoutNotify(Rect newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_field.SetValueWithoutNotify(newValue);
        }

        public override Rect value
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
