using System;
using System.Reflection;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(object), allowInheritType: true)]
    public class ObjectDrawer : FieldDrawer<object>
    {
        private bool m_initialized = false;

        public override void SetValueWithoutNotify(object newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RebuildChildren();
        }

        public override object value
        {
            get => base.value;
            set
            {
                base.value = value;
                RebuildChildren();
            }
        }

        private void RebuildChildren()
        {
            Clear(); // clear visual children
            m_initialized = false;

            if (m_value == null) return;

            var type = m_value.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var drawer = FieldDrawerFactory.Create()
                    .FromType(field.FieldType)
                    .Label(field.Name)
                    .AddModifiersFromMember(field)
                    .Build();

                if (drawer != null)
                {
                    drawer.value = field.GetValue(m_value);

                    if (drawer is VisualElement ve)
                    {
                        ve.RegisterCallback<ValueChangedEvent>(OnChildValueChanged);
                        Add(ve);
                    }
                }
            }

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var drawer = FieldDrawerFactory.Create()
                    .FromType(prop.PropertyType)
                    .Label(prop.Name)
                    .AddModifiersFromMember(prop)
                    .Build();

                if (drawer != null)
                {
                    drawer.value = prop.GetValue(m_value);

                    if (drawer is VisualElement ve)
                    {
                        ve.RegisterCallback<ValueChangedEvent>(OnChildValueChanged);
                        Add(ve);
                    }
                }
            }

            m_initialized = true;
        }

        private void OnChildValueChanged(ValueChangedEvent evt)
        {
            if (m_value == null) return;

            // Optional: update the actual field/property if we cache member info,
            // but for simplicity, we assume the compound drawer just signals that "some child changed".
            // A more advanced binder would sync back the value.
            NotifyValueChanged();
        }

        protected override void OnRenderSelf()
        {
            base.OnRenderSelf();

            if (!m_initialized && m_value != null)
            {
                RebuildChildren();
            }
        }
    }
}
