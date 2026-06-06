using System;
using System.Collections.Generic;
using System.Reflection;

namespace ImTK.UI
{
    public class FieldDrawerFactory
    {
        private Type m_valueType;
        private object m_value;
        private string m_label;
        private readonly List<Attribute> m_modifiers = new List<Attribute>();
        private Type m_forcedDrawerType;

        private FieldDrawerFactory() { }

        public static FieldDrawerFactory Create()
        {
            return new FieldDrawerFactory();
        }

        public FieldDrawerFactory FromValue(object value)
        {
            m_value = value;
            if (value != null && m_valueType == null)
            {
                m_valueType = value.GetType();
            }
            return this;
        }

        public FieldDrawerFactory FromType(Type type)
        {
            m_valueType = type;
            return this;
        }

        public FieldDrawerFactory Label(string label)
        {
            m_label = label;
            return this;
        }

        public FieldDrawerFactory AddModifier(Attribute modifier)
        {
            if (modifier != null)
                m_modifiers.Add(modifier);
            return this;
        }

        public FieldDrawerFactory AddModifiers(IEnumerable<Attribute> modifiers)
        {
            if (modifiers != null)
                m_modifiers.AddRange(modifiers);
            return this;
        }

        public FieldDrawerFactory AddModifiersFromMember(MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                AddModifiers(memberInfo.GetCustomAttributes());
            }
            return this;
        }

        public FieldDrawerFactory ForceDrawerType(Type drawerType)
        {
            m_forcedDrawerType = drawerType;
            return this;
        }

        public IFieldDrawer Build()
        {
            if (m_valueType == null && m_value != null)
            {
                m_valueType = m_value.GetType();
            }

            if (m_valueType == null)
            {
                throw new InvalidOperationException("Cannot build drawer without a known type.");
            }

            Type drawerType = m_forcedDrawerType;
            if (drawerType == null)
            {
                drawerType = FieldDrawerRegistry.FindDrawerType(m_valueType, m_modifiers);
            }

            if (drawerType == null)
            {
                // Fallback to ObjectDrawer or similar if none found?
                // If we can't find one, we might throw or return null.
                return null;
            }

            if (drawerType.IsGenericTypeDefinition)
            {
                if (m_valueType == typeof(Enum))
                {
                    // 無法使用泛型 EnumDrawer 來渲染未知的 Enum 基底型別
                    return null;
                }
                Type searchType = m_valueType;
                if (searchType.IsGenericType && searchType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    searchType = Nullable.GetUnderlyingType(searchType);
                }

                if (searchType.IsGenericType && drawerType.GetGenericArguments().Length == searchType.GetGenericArguments().Length)
                {
                    try
                    {
                        drawerType = drawerType.MakeGenericType(searchType.GetGenericArguments());
                    }
                    catch
                    {
                        drawerType = drawerType.MakeGenericType(searchType);
                    }
                }
                else
                {
                    drawerType = drawerType.MakeGenericType(searchType);
                }
            }

            var drawer = (IFieldDrawer)Activator.CreateInstance(drawerType);

            if (!string.IsNullOrEmpty(m_label))
            {
                drawer.label = m_label;
            }

            if (m_value != null)
            {
                drawer.value = m_value;
            }

            foreach (var mod in m_modifiers)
            {
                drawer.ApplyModifier(mod);
            }

            return drawer;
        }
    }
}
