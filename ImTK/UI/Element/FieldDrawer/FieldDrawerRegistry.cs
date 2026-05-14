using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ImTK.UI
{
    public static class FieldDrawerRegistry
    {
        private class DrawerDefinition
        {
            public Type drawerType;
            public CustomFieldDrawerAttribute attribute;
        }

        private static readonly List<DrawerDefinition> s_drawerDefinitions = new List<DrawerDefinition>();
        // Cache to speed up lookup: (DataType, ModifierType) -> DrawerType
        private static readonly Dictionary<(Type, Type), Type> s_typeCache = new Dictionary<(Type, Type), Type>();

        static FieldDrawerRegistry()
        {
            Initialize();
        }

        private static void Initialize()
        {
            s_drawerDefinitions.Clear();
            s_typeCache.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsAbstract || !typeof(IFieldDrawer).IsAssignableFrom(type))
                            continue;

                        var attr = type.GetCustomAttribute<CustomFieldDrawerAttribute>();
                        if (attr != null)
                        {
                            s_drawerDefinitions.Add(new DrawerDefinition
                            {
                                drawerType = type,
                                attribute = attr
                            });
                        }
                    }
                }
                catch (ReflectionTypeLoadException) { }
            }
        }

        public static Type FindDrawerType(Type targetType, IEnumerable<Attribute> modifiers)
        {
            Type modifierType = null;
            if (modifiers != null)
            {
                // Simple implementation: try to find the first modifier that matches
                foreach (var mod in modifiers)
                {
                    modifierType = mod.GetType();
                    var specificType = FindDrawerTypeInternal(targetType, modifierType);
                    if (specificType != null)
                        return specificType;
                }
            }

            return FindDrawerTypeInternal(targetType, null);
        }

        private static Type FindDrawerTypeInternal(Type targetType, Type modifierType)
        {
            var key = (targetType, modifierType);
            if (s_typeCache.TryGetValue(key, out Type cachedType))
                return cachedType;

            Type bestMatch = null;
            int bestDepth = int.MaxValue;

            foreach (var def in s_drawerDefinitions)
            {
                if (def.attribute.requiredModifier != modifierType)
                    continue;

                if (def.attribute.targetType == targetType)
                {
                    bestMatch = def.drawerType;
                    break;
                }

                if (def.attribute.allowInheritType && def.attribute.targetType.IsAssignableFrom(targetType))
                {
                    int depth = GetInheritanceDepth(targetType, def.attribute.targetType);
                    if (depth < bestDepth)
                    {
                        bestDepth = depth;
                        bestMatch = def.drawerType;
                    }
                }
            }

            // Fallback for objects if allowInheritType matched none, or just return null and let factory handle it
            if (bestMatch != null)
            {
                s_typeCache[key] = bestMatch;
            }

            return bestMatch;
        }

        private static int GetInheritanceDepth(Type child, Type parent)
        {
            if (child == parent) return 0;
            if (child.IsInterface) return 1; // Simplified for interfaces

            int depth = 0;
            Type current = child;
            while (current != null && current != parent)
            {
                depth++;
                current = current.BaseType;
            }
            return depth;
        }
    }
}
