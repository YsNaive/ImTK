using System;
using System.Reflection;
using System.Collections.Generic;
using ImTK.Log;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(object), allowInheritType: true)]
    public class ObjectDrawer : FoldoutDrawer<object>
    {
        private bool m_childrenBuilt = false;
        private Dictionary<VisualElement, MemberInfo> m_memberMap = new Dictionary<VisualElement, MemberInfo>();

        public ObjectDrawer()
        {
        }

        public override DrawerLayoutMode layoutMode
        {
            get => base.layoutMode;
            set
            {
                base.layoutMode = value;
                if (value == DrawerLayoutMode.Expand && !m_childrenBuilt)
                {
                    ScheduleDeferred(RebuildChildren);
                }
            }
        }

        public override void SetValueWithoutNotify(object newValue)
        {
            var oldType = m_value?.GetType();
            var newType = newValue?.GetType();

            base.SetValueWithoutNotify(newValue);

            if (oldType != newType)
            {
                m_childrenBuilt = false;
                Clear();
                m_memberMap.Clear();
                if (isExpanded)
                {
                    ScheduleDeferred(RebuildChildren);
                }
            }
            else if (m_childrenBuilt && m_value != null)
            {
                // Sync values to existing UI
                foreach (var kvp in m_memberMap)
                {
                    if (kvp.Key is IFieldDrawer drawer)
                    {
                        var member = kvp.Value;
                        object childValue = null;
                        if (member is FieldInfo f) childValue = f.GetValue(m_value);
                        else if (member is PropertyInfo p) childValue = p.GetValue(m_value);

                        drawer.SetValueWithoutNotify(childValue);
                    }
                }
            }
        }

        public override object value
        {
            get => base.value;
            set
            {
                // Assign value directly, no need to duplicate SetValueWithoutNotify logic
                // if we use base.value, it will trigger SetValueWithoutNotify indirectly. Wait, base.value uses _SetValue which invokes SetValueWithoutNotify? 
                // No, base.value uses _SetValue which just sets m_value and sends event.
                // We should explicitly use the same type-checking logic as SetValueWithoutNotify.
                var oldType = m_value?.GetType();
                var newType = value?.GetType();
                
                base.value = value;
                
                if (oldType != newType)
                {
                    m_childrenBuilt = false;
                    Clear();
                    m_memberMap.Clear();
                    if (isExpanded)
                    {
                        ScheduleDeferred(RebuildChildren);
                    }
                }
                else if (m_childrenBuilt && m_value != null)
                {
                    foreach (var kvp in m_memberMap)
                    {
                        if (kvp.Key is IFieldDrawer drawer)
                        {
                            var member = kvp.Value;
                            object childValue = null;
                            if (member is FieldInfo f) childValue = f.GetValue(m_value);
                            else if (member is PropertyInfo p) childValue = p.GetValue(m_value);

                            // Using SetValueWithChanged to trigger events?
                            // Wait, if the parent's value was assigned explicitly, the child's value should just silently update to match, because the parent ALREADY triggered a value changed event for the whole object!
                            // So we just use SetValueWithoutNotify.
                            drawer.SetValueWithoutNotify(childValue);
                        }
                    }
                }
            }
        }

        private void RebuildChildren()
        {
            try
            {
                Clear(); // clear visual children
                m_memberMap.Clear();
                m_childrenBuilt = true;

                ImTK.Log.ImTKLog.Debug($"ObjectDrawer.RebuildChildren called! m_value is {(m_value == null ? "null" : m_value.GetType().Name)}");

                if (m_value == null)
                {
                    Add(new Label(" (null)"));
                    return;
                }

                var type = m_value.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                ImTK.Log.ImTKLog.Debug($"ObjectDrawer found {fields.Length} fields and {properties.Length} properties.");

                if (fields.Length == 0 && properties.Length == 0)
                {
                    Add(new Label(" (No public fields or properties)"));
                    return;
                }

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
                    else
                    {
                        ImTK.Log.ImTKLog.Warning($"ObjectDrawer: FieldDrawerFactory returned null for field {field.Name} of type {field.FieldType.Name}");
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
            }
            catch (Exception ex)
            {
                ImTK.Log.ImTKLog.Error(ex, "Exception in ObjectDrawer.RebuildChildren");
            }
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

        public override void OnEndRender()
        {
            base.OnEndRender();
        }
    }
}
