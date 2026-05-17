using System;
using System.Collections.Generic;
using System.Reflection;

namespace ImTK.UI.Style
{
    public static class StyleMappingRegistry
    {
        private static Dictionary<Type, StyleMapping> s_cache = new Dictionary<Type, StyleMapping>();

        public static StyleMapping GetMappingFor(Type elementType)
        {
            if (s_cache.TryGetValue(elementType, out var mapping))
                return mapping;

            Type mappingType = elementType.GetNestedType("Mapping", BindingFlags.Public | BindingFlags.NonPublic);

            if (mappingType != null && typeof(StyleMapping).IsAssignableFrom(mappingType))
            {
                mapping = (StyleMapping)Activator.CreateInstance(mappingType);
            }
            else
            {
                if (elementType.BaseType != null && elementType.BaseType != typeof(object))
                {
                    mapping = GetMappingFor(elementType.BaseType);
                }
                else
                {
                    mapping = new StyleMapping();
                }
            }

            s_cache[elementType] = mapping;
            return mapping;
        }
    }
}
