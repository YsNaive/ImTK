using System;
using System.Reflection;
using System.Collections.Generic;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(object), allowInheritType: true)]
    public class ObjectDrawer : FieldDrawer<object>
    {
        private bool m_initialized = false;
        private Dictionary<VisualElement, MemberInfo> m_memberMap = new Dictionary<VisualElement, MemberInfo>();

        public ObjectDrawer()
        {
            layoutMode = DrawerLayoutMode.Expand;
        }

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
            m_memberMap.Clear();
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
                        m_memberMap[ve] = field;
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
                        m_memberMap[ve] = prop;
                        ve.RegisterCallback<ValueChangedEvent>(OnChildValueChanged);
                        Add(ve);
                    }
                }
            }

            m_initialized = true;
        }

        private void OnChildValueChanged(ValueChangedEvent evt)
        {
            if (m_value == null || evt.source == null) return;

            if (m_memberMap.TryGetValue(evt.source, out var memberInfo))
            {
                if (memberInfo is FieldInfo field)
                {
                    field.SetValue(m_value, evt.newValueObj);
                }
                else if (memberInfo is PropertyInfo prop)
                {
                    prop.SetValue(m_value, evt.newValueObj);
                }
            }

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
