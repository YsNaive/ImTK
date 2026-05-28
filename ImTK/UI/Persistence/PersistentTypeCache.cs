using System;
using System.Collections.Generic;
using System.Reflection;
using ImTK.Log;

namespace ImTK.UI.Persistence
{
    internal static class PersistentTypeCache
    {
        private enum PersistentDataType { Float, Int, String, Bool }

        private class MemberAccessor
        {
            public string Key;
            public PersistentDataType DataType;
            public Func<object, object> Getter;
            public Action<object, object> Setter;
        }

        private static readonly Dictionary<Type, List<MemberAccessor>> _cache = new Dictionary<Type, List<MemberAccessor>>();

        public static void WriteState(VisualElement element, StateWriter writer)
        {
            var accessors = GetOrCreate(element.GetType());
            foreach (var acc in accessors)
            {
                object value = acc.Getter(element);
                if (value == null) continue;

                string fullLocalKey = $"{element.persistenceKey}.{acc.Key}";
                switch (acc.DataType)
                {
                    case PersistentDataType.Float: writer.WriteFloat(fullLocalKey, (float)value); break;
                    case PersistentDataType.Int: writer.WriteInt(fullLocalKey, (int)value); break;
                    case PersistentDataType.String: writer.WriteString(fullLocalKey, (string)value); break;
                    case PersistentDataType.Bool: writer.WriteBool(fullLocalKey, (bool)value); break;
                }
            }
        }

        public static void ReadState(VisualElement element, StateReader reader)
        {
            var accessors = GetOrCreate(element.GetType());
            foreach (var acc in accessors)
            {
                object currentValue = acc.Getter(element);
                if (currentValue == null) continue;

                string fullLocalKey = $"{element.persistenceKey}.{acc.Key}";
                switch (acc.DataType)
                {
                    case PersistentDataType.Float: 
                        acc.Setter(element, reader.ReadFloat(fullLocalKey, (float)currentValue));
                        break;
                    case PersistentDataType.Int:
                        acc.Setter(element, reader.ReadInt(fullLocalKey, (int)currentValue));
                        break;
                    case PersistentDataType.String:
                        acc.Setter(element, reader.ReadString(fullLocalKey, (string)currentValue));
                        break;
                    case PersistentDataType.Bool:
                        acc.Setter(element, reader.ReadBool(fullLocalKey, (bool)currentValue));
                        break;
                }
            }
        }

        private static List<MemberAccessor> GetOrCreate(Type type)
        {
            if (_cache.TryGetValue(type, out var list))
                return list;

            list = new List<MemberAccessor>();
            var visitedTypes = new HashSet<Type>();
            visitedTypes.Add(type);

            BuildAccessorsRecursive(
                type, 
                list, 
                visitedTypes, 
                prefix: "", 
                parentGetter: obj => obj, 
                parentSetter: (obj, val) => {}, // root setter does nothing
                includeAllMembers: false
            );

            _cache[type] = list;
            return list;
        }

        private static void BuildAccessorsRecursive(
            Type currentType, 
            List<MemberAccessor> list, 
            HashSet<Type> visitedTypes, 
            string prefix, 
            Func<object, object> parentGetter, 
            Action<object, object> parentSetter,
            bool includeAllMembers)
        {
            var fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<PersistentAttribute>();
                bool shouldInclude = attr != null || (includeAllMembers && field.IsPublic);
                if (!shouldInclude) continue;

                string memberKey = attr?.Key ?? field.Name;
                string fullKey = string.IsNullOrEmpty(prefix) ? memberKey : $"{prefix}.{memberKey}";

                Func<object, object> currentGetter = rootObj => 
                {
                    object parentObj = parentGetter(rootObj);
                    return parentObj == null ? null : field.GetValue(parentObj);
                };

                Action<object, object> currentSetter = (rootObj, val) => 
                {
                    object parentObj = parentGetter(rootObj);
                    if (parentObj == null) return;
                    
                    field.SetValue(parentObj, val);
                    
                    if (currentType.IsValueType)
                    {
                        parentSetter(rootObj, parentObj);
                    }
                };

                ProcessMember(field.FieldType, list, visitedTypes, fullKey, currentGetter, currentSetter, attr, includeAllMembers);
            }

            var props = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<PersistentAttribute>();
                bool isPropPublic = prop.GetMethod != null && prop.GetMethod.IsPublic;
                bool shouldInclude = attr != null || (includeAllMembers && isPropPublic);
                if (!shouldInclude) continue;

                if (!prop.CanRead || !prop.CanWrite)
                {
                    if (attr != null) // Only warn if explicitly tagged
                        ImTKLog.Error(new InvalidOperationException($"Property must have get and set"), $"[Persistent] attribute on {currentType.Name}.{prop.Name} requires both get and set accessors.");
                    continue;
                }

                string memberKey = attr?.Key ?? prop.Name;
                string fullKey = string.IsNullOrEmpty(prefix) ? memberKey : $"{prefix}.{memberKey}";

                Func<object, object> currentGetter = rootObj => 
                {
                    object parentObj = parentGetter(rootObj);
                    return parentObj == null ? null : prop.GetValue(parentObj);
                };

                Action<object, object> currentSetter = (rootObj, val) => 
                {
                    object parentObj = parentGetter(rootObj);
                    if (parentObj == null) return;

                    prop.SetValue(parentObj, val);

                    if (currentType.IsValueType)
                    {
                        parentSetter(rootObj, parentObj);
                    }
                };

                ProcessMember(prop.PropertyType, list, visitedTypes, fullKey, currentGetter, currentSetter, attr, includeAllMembers);
            }
        }

        private static void ProcessMember(
            Type memberType, 
            List<MemberAccessor> list, 
            HashSet<Type> visitedTypes, 
            string fullKey, 
            Func<object, object> currentGetter, 
            Action<object, object> currentSetter, 
            PersistentAttribute attr,
            bool parentIncludeAllMembers)
        {
            if (TryGetDataType(memberType, out var dataType))
            {
                list.Add(new MemberAccessor
                {
                    Key = fullKey,
                    DataType = dataType,
                    Getter = currentGetter,
                    Setter = currentSetter
                });
            }
            else
            {
                bool flatten = attr != null ? attr.Flatten : true; // Default flatten true if implicit
                if (flatten)
                {
                    if (visitedTypes.Contains(memberType))
                    {
                        ImTKLog.Error(new InvalidOperationException($"Circular reference detected in [Persistent] graph"), $"Type {memberType.Name} creates a circular reference at {fullKey}.");
                        return;
                    }

                    bool nextIncludeAll = attr != null ? attr.IncludeAllMembers : parentIncludeAllMembers;
                    
                    visitedTypes.Add(memberType);
                    BuildAccessorsRecursive(memberType, list, visitedTypes, fullKey, currentGetter, currentSetter, nextIncludeAll);
                    visitedTypes.Remove(memberType);
                }
                else if (attr != null) // Explicitly tagged but not flattened and not supported leaf
                {
                    ImTKLog.Error(new NotSupportedException($"Unsupported member type {memberType.Name}"), $"[Persistent] on {fullKey} is of unsupported type {memberType.Name}. Only float, int, string, bool, or nested objects with Flatten=true are supported.");
                }
            }
        }

        private static bool TryGetDataType(Type t, out PersistentDataType dataType)
        {
            if (t == typeof(float)) { dataType = PersistentDataType.Float; return true; }
            if (t == typeof(int)) { dataType = PersistentDataType.Int; return true; }
            if (t == typeof(string)) { dataType = PersistentDataType.String; return true; }
            if (t == typeof(bool)) { dataType = PersistentDataType.Bool; return true; }
            
            dataType = default;
            return false;
        }
    }
}
