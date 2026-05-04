            private static void ProcessPendingQueuesAndStateChanges()
            {
                // Add pending objects (copy to array to prevent modification during iteration)
                if (s_pendingAdd.Count > 0)
                {
                    var adding = s_pendingAdd.ToArray();
                    s_pendingAdd.Clear();
                    foreach (var obj in adding)
                    {
                        s_objects.Add(obj);
                        if (obj.m_enabled)
                        {
                            obj.m_activeInHierarchy = true;
                            obj.OnEnable();
                        }
                    }
                }

                // Check enable/disable state changes for modules
                foreach (var module in s_modules.Values)
                {
                    if (module.m_enabled && !module.m_activeInHierarchy)
                    {
                        module.m_activeInHierarchy = true;
                        module.OnEnable();
                    }
                    else if (!module.m_enabled && module.m_activeInHierarchy)
                    {
                        module.m_activeInHierarchy = false;
                        module.OnDisable();
                    }
                }

                // Check enable/disable state changes for objects (copy to array)
                var currentObjects = s_objects.ToArray();
                foreach (var obj in currentObjects)
                {
                    if (obj.m_enabled && !obj.m_activeInHierarchy)
                    {
                        obj.m_activeInHierarchy = true;
                        obj.OnEnable();
                    }
                    else if (!obj.m_enabled && obj.m_activeInHierarchy)
                    {
                        obj.m_activeInHierarchy = false;
                        obj.OnDisable();
                    }
                }

                // Remove pending objects
                if (s_pendingRemove.Count > 0)
                {
                    var removing = s_pendingRemove.ToArray();
                    s_pendingRemove.Clear();
                    foreach (var obj in removing)
                    {
                        if (obj.m_activeInHierarchy)
                        {
                            obj.m_activeInHierarchy = false;
                            obj.OnDisable();
                        }
                        obj.OnDestroy();
                        s_objects.Remove(obj);
                    }
                }
            }
