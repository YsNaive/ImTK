using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ImTK;
using dashboard.Dashboard.Entities;

namespace dashboard.Dashboard.Core
{
    public static class Registry
    {
        private static readonly ConcurrentQueue<Action> dispatcherQueue = new ConcurrentQueue<Action>();

        private static readonly DashEntity?[] m_entities = new DashEntity?[256];
        private static readonly Dictionary<string, DashEntityWindow> m_groupWindows = new Dictionary<string, DashEntityWindow>();

        private static readonly Dictionary<byte, Type> typeTable = new Dictionary<byte, Type>();
        private static readonly Dictionary<byte, bool> isReferenceTable = new Dictionary<byte, bool>();

        private static HashSet<string> used_group = new HashSet<string>();
        private static HashSet<string> using_group = new HashSet<string>();

        static Registry()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName?.StartsWith("System") == true || assembly.FullName?.StartsWith("Microsoft") == true)
                    continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(DashEntity)))
                        {
                            var attr = type.GetCustomAttribute<EntityTypeAttribute>();
                            if (attr != null)
                            {
                                typeTable[attr.TypeId] = type;
                                isReferenceTable[attr.TypeId] = attr.IsReference;
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException) { }
            }
        }

        public static void Initialize()
        {
            used_group = CacheHandler.LoadUsedGroups();

            foreach (var group in used_group)
            {
                GetOrCreateWindow(group);
            }
        }

        public static void Reset()
        {
            dispatcherQueue.Enqueue(() => {
                Array.Clear(m_entities, 0, m_entities.Length);

                foreach (var kvp in m_groupWindows)
                {
                    kvp.Value.Clear();
                }

                var groupsToDrop = new List<string>();
                foreach (var group in used_group)
                {
                    if (!using_group.Contains(group))
                    {
                        groupsToDrop.Add(group);
                    }
                }

                foreach (var group in groupsToDrop)
                {
                    if (m_groupWindows.TryGetValue(group, out var win))
                    {
                        win.Close();
                        m_groupWindows.Remove(group);
                    }
                }

                used_group = new HashSet<string>(using_group);
                using_group.Clear();

                CacheHandler.SaveUsedGroups(used_group);
            });
        }

        public static void CreateEntity(byte entityId, byte typeId, string path)
        {
            dispatcherQueue.Enqueue(() => {
                if (!typeTable.TryGetValue(typeId, out Type? entityType))
                {
                    Console.WriteLine($"[Registry] Unsupported TypeId {typeId:X2} for path '{path}'"); // Error log, keep it
                    return;
                }

                try
                {
                    var entityObj = Activator.CreateInstance(entityType, new object[] { entityId, typeId, path });
                    if (entityObj is DashEntity entity)
                    {
                        // Console.WriteLine($"[Registry] Instantiated {entityType.Name} for ID {entityId} ({path})"); // TODO: Integrate into Debug Log system
                        // Remove orphaned visual elements if entity already exists
                        var existingEntity = m_entities[entityId];
                        if (existingEntity != null)
                        {
                            m_entities[entityId] = null;
                            if (m_groupWindows.TryGetValue(existingEntity.group, out var existingWindow))
                            {
                                int index = existingWindow.IndexOf(existingEntity);
                                existingWindow.Remove(existingEntity);

                                m_entities[entityId] = entity;
                                string groupName = entity.group;
                                using_group.Add(groupName);

                                bool cacheChanged = false;
                                if (!used_group.Contains(groupName))
                                {
                                    used_group.Add(groupName);
                                    cacheChanged = true;
                                }

                                if (cacheChanged)
                                {
                                    CacheHandler.SaveUsedGroups(used_group);
                                }

                                var window = GetOrCreateWindow(groupName);

                                // Insert at old position if same window, else add to end
                                if (index != -1 && existingWindow == window)
                                {
                                    window.Insert(index, entity);
                                }
                                else
                                {
                                    window.Add(entity);
                                }
                                return;
                            }
                        }

                        m_entities[entityId] = entity;

                        string newGroupName = entity.group;
                        using_group.Add(newGroupName);

                        bool newCacheChanged = false;
                        if (!used_group.Contains(newGroupName))
                        {
                            used_group.Add(newGroupName);
                            newCacheChanged = true;
                        }

                        if (newCacheChanged)
                        {
                            CacheHandler.SaveUsedGroups(used_group);
                        }

                        var newWindow = GetOrCreateWindow(newGroupName);
                        newWindow.Add(entity);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Registry] Failed to create entity: {ex.Message}");
                }
            });
        }

        public static void SyncEntity(byte entityId, byte[] payload)
        {
            dispatcherQueue.Enqueue(() => {
                var entity = m_entities[entityId];
                if (entity == null)
                {
                    Console.WriteLine($"[Registry] Sync Error: Entity ID {entityId} not found."); // Keep error
                    return;
                }

                bool isReference = false;
                isReferenceTable.TryGetValue(entity.typeId, out isReference);

                if (isReference)
                {
                    if (payload.Length < 1) return;

                    byte opcode = payload[0];
                    byte[] data = new byte[payload.Length - 1];
                    Array.Copy(payload, 1, data, 0, data.Length);

                    entity.receive(opcode, data);
                }
                else
                {
                    entity.receive(0x00, payload);
                }
            });
        }

        private static DashEntityWindow GetOrCreateWindow(string groupName)
        {
            if (m_groupWindows.TryGetValue(groupName, out var existingWindow))
            {
                if (!existingWindow.isOpen)
                {
                    existingWindow.isOpen = true;
                    if (WindowView.openedWindows.IndexOf(existingWindow) == -1)
                    {
                        WindowView.openedWindows.Add(existingWindow);
                    }
                }
                return existingWindow;
            }

            var newWindow = new DashEntityWindow(groupName);
            WindowView.openedWindows.Add(newWindow);
            m_groupWindows[groupName] = newWindow;
            return newWindow;
        }

        public static void ProcessQueue()
        {
            while (dispatcherQueue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Registry] Dispatcher exception: {ex.Message}");
                }
            }
        }

        private class RegistryModule : ImTKModule
        {
            private RegistryModule() { } // Implicitly called by ImTK initialization

            public override void Update(double deltaTime)
            {
                Registry.ProcessQueue();
            }

            public override void Render(double deltaTime)
            {
            }
        }
    }
}