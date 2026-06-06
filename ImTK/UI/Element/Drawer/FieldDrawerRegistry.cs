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
                        if (type.IsAbstract)
                            continue;

                        bool isDrawer = typeof(IFieldDrawer).IsAssignableFrom(type);
                        if (!isDrawer)
                        {
                            foreach (var iface in type.GetInterfaces())
                            {
                                if (iface == typeof(IFieldDrawer))
                                {
                                    isDrawer = true;
                                    break;
                                }
                            }
                        }

                        if (!isDrawer)
                            continue;

                        var attr = type.GetCustomAttribute<CustomFieldDrawerAttribute>();
                        if (attr != null)
                        {
                            s_drawerDefinitions.Add(new DrawerDefinition
                            {
                                drawerType = type,
                                attribute = attr
                            });
                            ImTK.Log.ImTKLog.Trace($"Registered Drawer: {type.Name} for target {attr.targetType.Name}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException) { }
            }

            ImTK.Log.ImTKLog.Info($"Registered {s_drawerDefinitions.Count} FieldDrawers.");
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

            ImTK.Log.ImTKLog.Trace($"Finding drawer for {targetType.Name} (IsGeneric: {targetType.IsGenericType})");

            Type searchType = targetType;
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                searchType = Nullable.GetUnderlyingType(targetType);
            }

            Type bestMatch = null;
            int bestDepth = int.MaxValue;

            foreach (var def in s_drawerDefinitions)
            {
                if (def.attribute.requiredModifier != modifierType)
                    continue;

                if (def.attribute.targetType == searchType)
                {
                    bestMatch = def.drawerType;
                    break;
                }

                if (searchType.IsGenericType && def.attribute.targetType.IsGenericTypeDefinition)
                {
                    bool match = searchType.GetGenericTypeDefinition() == def.attribute.targetType;
                    if (match)
                    {
                        bestMatch = def.drawerType;
                        break;
                    }
                }

                if (def.attribute.allowInheritType && def.attribute.targetType.IsAssignableFrom(searchType))
                {
                    int depth = GetInheritanceDepth(searchType, def.attribute.targetType);
                    if (depth < bestDepth)
                    {
                        bestDepth = depth;
                        bestMatch = def.drawerType;
                    }
                }
            }

            ImTK.Log.ImTKLog.Trace($"Found bestMatch for {targetType.Name}: {bestMatch?.Name ?? "null"}");

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
