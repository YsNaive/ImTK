using System;
using System.Reflection;
using System.Collections.Generic;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(object), allowInheritType: true)]
    public class ObjectDrawer : FoldoutDrawer<object>
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
                        RegisterGenericCallback(ve, field.FieldType);
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
                        RegisterGenericCallback(ve, prop.PropertyType);
                        Add(ve);
                    }
                }
            }

            m_initialized = true;
        }

        private void RegisterGenericCallback(VisualElement element, Type valueType)
        {
            var method = GetType().GetMethod(nameof(RegisterGenericCallbackInternal), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(valueType);
            genericMethod.Invoke(this, new object[] { element });
        }

        private void RegisterGenericCallbackInternal<T>(VisualElement element)
        {
            element.RegisterCallback<ValueChangedEvent<T>>(OnChildValueChangedGeneric<T>);
        }

        private void OnChildValueChangedGeneric<T>(ValueChangedEvent<T> evt)
        {
            OnChildValueChanged(evt);
        }

        private void OnChildValueChanged(IValueChangedEvent evt)
        {
            if (m_value == null || ((UIEventBase)evt).source == null) return;

            var sourceElement = ((UIEventBase)evt).source;

            if (m_memberMap.TryGetValue(sourceElement, out var memberInfo))
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

        public override void OnRender()
        {
            base.OnRender();

            if (!m_initialized && m_value != null)
            {
                RebuildChildren();
            }
        }
    }
}
