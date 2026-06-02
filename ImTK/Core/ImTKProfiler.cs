using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ImTK.Core
{
    /// <summary>
    /// A lightweight, tree-based profiler for tracking execution time of various subsystems.
    /// It maintains a rolling history of durations per frame for visualization.
    /// </summary>
    public static class ImTKProfiler
    {
        public class ProfilerNode
        {
            public IntPtr CachedTreeNodeId = IntPtr.Zero;
            public IntPtr NamePtr = IntPtr.Zero;
            private string m_name;
            public string Name
            {
                get => m_name;
                set
                {
                    m_name = value;
                    if (NamePtr != IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.Marshal.FreeCoTaskMem(NamePtr);
                    }
                    NamePtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(value);

                    if (CachedTreeNodeId != IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.Marshal.FreeCoTaskMem(CachedTreeNodeId);
                    }
                    CachedTreeNodeId = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8($"##{value}");
                }
            }
            public ProfilerNode Parent;
            public ConcurrentDictionary<string, ProfilerNode> Children = new ConcurrentDictionary<string, ProfilerNode>();
            public ConcurrentDictionary<string, ProfilerNode> RelativePathCache = new ConcurrentDictionary<string, ProfilerNode>();
            public volatile ProfilerNode[] ChildrenArray = Array.Empty<ProfilerNode>();

            public ProfilerNode GetOrAddRelativePath(string path)
            {
                if (RelativePathCache.TryGetValue(path, out var node)) return node;
                var resolved = ImTKProfiler.GetOrCreateNodeByPath(path, this);
                RelativePathCache.TryAdd(path, resolved);
                return resolved;
            }

            public ProfilerNode GetOrAddChild(string name, ProfilerNode parent)
            {
                if (Children.TryGetValue(name, out var existing)) return existing;
                var newNode = new ProfilerNode { Name = name, Parent = parent };
                if (Children.TryAdd(name, newNode))
                {
                    lock (this)
                    {
                        var list = new System.Collections.Generic.List<ProfilerNode>(Children.Count);
                        foreach (var kvp in Children)
                        {
                            list.Add(kvp.Value);
                        }
                        ChildrenArray = list.ToArray();
                    }
                    return newNode;
                }
                return Children[name];
            }
            
            // Callers that recorded into this scope
            public ConcurrentDictionary<(string, int), byte> Callers = new ConcurrentDictionary<(string, int), byte>();
            public volatile (string, int)[] CallersArray = Array.Empty<(string, int)>();

            public void AddCaller(string filePath, int lineNumber)
            {
                var callerKey = (filePath, lineNumber);
                if (Callers.TryAdd(callerKey, 1))
                {
                    lock (this)
                    {
                        var list = new System.Collections.Generic.List<(string, int)>(Callers.Keys);
                        CallersArray = list.ToArray();
                    }
                }
            }

            // Max 60 seconds at 60fps = 3600 frames
            public float[] History = new float[3600];
            public float[] GcHistory = new float[3600];
            
            public int Head = 0;
            public int SampleCount = 0; 
            
            public long CurrentFrameSelfTicks = 0;
            public long CurrentFrameSelfGcBytes = 0;

            public float GetLatestMs()
            {
                if (SampleCount == 0) return 0f;
                int idx = Head - 1;
                if (idx < 0) idx += 3600;
                return History[idx];
            }

            public float GetAverageMs(int framesToAverage)
            {
                if (SampleCount == 0) return 0f;
                int count = Math.Min(SampleCount, framesToAverage);
                float sum = 0f;
                for (int i = 0; i < count; i++)
                {
                    int idx = Head - 1 - i;
                    if (idx < 0) idx += 3600;
                    sum += History[idx];
                }
                return sum / count;
            }
            
            public float GetLatestGcKb()
            {
                if (SampleCount == 0) return 0f;
                int idx = Head - 1;
                if (idx < 0) idx += 3600;
                return GcHistory[idx];
            }

            public float GetAverageGcKb(int framesToAverage)
            {
                if (SampleCount == 0) return 0f;
                int count = Math.Min(SampleCount, framesToAverage);
                float sum = 0f;
                for (int i = 0; i < count; i++)
                {
                    int idx = Head - 1 - i;
                    if (idx < 0) idx += 3600;
                    sum += GcHistory[idx];
                }
                return sum / count;
            }
        }

        private static readonly ProfilerNode s_root = new ProfilerNode { Name = "Root" };
        private static readonly Stopwatch s_stopwatch = Stopwatch.StartNew();

        private static readonly ThreadLocal<Stack<ProfilerNode>> s_scopeStack = new ThreadLocal<Stack<ProfilerNode>>(() => 
        {
            var stack = new Stack<ProfilerNode>();
            stack.Push(s_root);
            return stack;
        });

        private static readonly ConcurrentDictionary<string, ProfilerNode> s_pathCache = new ConcurrentDictionary<string, ProfilerNode>();

        internal static ProfilerNode GetOrCreateNodeByPath(string path, ProfilerNode startNode)
        {
            if (path.IndexOf('/') == -1 && path.IndexOf('\\') == -1)
            {
                return startNode.GetOrAddChild(path, startNode);
            }

            var parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            ProfilerNode current = startNode;
            foreach (var part in parts)
            {
                current = current.GetOrAddChild(part, current);
            }
            return current;
        }

        public static ProfilerNode Root => s_root;

        public static ProfilerScope Scope(string path = null, [System.Runtime.CompilerServices.CallerLineNumber] int callerLine = 0, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            return new ProfilerScope(path, false, callerFile, callerLine, null);
        }

        public static ProfilerScope ScopeRelative(string relatedPath, [System.Runtime.CompilerServices.CallerLineNumber] int callerLine = 0, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            return new ProfilerScope(relatedPath, true, callerFile, callerLine, null);
        }

        public static ProfilerScope Scope(string groupPath, string name, [System.Runtime.CompilerServices.CallerLineNumber] int callerLine = 0, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            return new ProfilerScope(groupPath, false, callerFile, callerLine, name);
        }

        public static ProfilerScope ScopeRelative(string groupPath, string name, [System.Runtime.CompilerServices.CallerLineNumber] int callerLine = 0, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            return new ProfilerScope(groupPath, true, callerFile, callerLine, name);
        }

        /// <summary>
        /// Commits all accumulated metrics for the current frame into the history buffer.
        /// </summary>
        internal static void EndFrame()
        {
            CommitRecursive(s_root);
        }

        private static void CommitRecursive(ProfilerNode node)
        {
            float childrenTimeSum = 0f;
            float childrenGcSum = 0f;
            foreach (var child in node.ChildrenArray)
            {
                CommitRecursive(child);
                int lastHead = child.Head == 0 ? 3599 : child.Head - 1;
                childrenTimeSum += child.History[lastHead];
                childrenGcSum += child.GcHistory[lastHead];
            }

            float selfMs = (float)Interlocked.Exchange(ref node.CurrentFrameSelfTicks, 0) / Stopwatch.Frequency * 1000f;
            float selfGc = (float)Interlocked.Exchange(ref node.CurrentFrameSelfGcBytes, 0) / 1024f;

            float totalTime = Math.Max(0f, selfMs) + childrenTimeSum;
            float totalGc = Math.Max(0f, selfGc) + childrenGcSum;

            node.History[node.Head] = totalTime;
            node.GcHistory[node.Head] = totalGc;
            
            node.Head = (node.Head + 1) % 3600;
            if (node.SampleCount < 3600) node.SampleCount++;
        }

        public struct ProfilerScope : IDisposable
        {
            private readonly ProfilerNode m_parentNode;
            private readonly ProfilerNode m_currentNode;
            private readonly long m_startTicks;
            private readonly long m_startGcBytes;

            public ProfilerScope(string path, bool isRelative, string callerFile, int callerLine, string dynamicName)
            {
                var stack = s_scopeStack.Value;
                m_parentNode = stack.Peek();

                if (string.IsNullOrEmpty(path))
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(callerFile);
                    if (string.IsNullOrEmpty(name)) name = "Unknown";
                    m_currentNode = m_parentNode.GetOrAddChild(name, m_parentNode);
                }
                else
                {
                    ProfilerNode groupNode;
                    if (!isRelative)
                    {
                        groupNode = s_pathCache.GetOrAdd(path, static p => GetOrCreateNodeByPath(p, s_root));
                    }
                    else
                    {
                        groupNode = m_parentNode.GetOrAddRelativePath(path);
                    }

                    if (dynamicName != null)
                    {
                        m_currentNode = groupNode.GetOrAddChild(dynamicName, groupNode);
                    }
                    else
                    {
                        m_currentNode = groupNode;
                    }
                }

                if (!string.IsNullOrEmpty(callerFile)) 
                {
                    m_currentNode.AddCaller(callerFile, callerLine);
                }

                stack.Push(m_currentNode);

                m_startGcBytes = GC.GetAllocatedBytesForCurrentThread();
                m_startTicks = s_stopwatch.ElapsedTicks;
            }

            public void Dispose()
            {
                long elapsedTicks = s_stopwatch.ElapsedTicks - m_startTicks;
                long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - m_startGcBytes;
                
                Interlocked.Add(ref m_currentNode.CurrentFrameSelfTicks, elapsedTicks);
                Interlocked.Add(ref m_currentNode.CurrentFrameSelfGcBytes, allocatedBytes);
                
                if (m_parentNode != null)
                {
                    Interlocked.Add(ref m_parentNode.CurrentFrameSelfTicks, -elapsedTicks);
                    Interlocked.Add(ref m_parentNode.CurrentFrameSelfGcBytes, -allocatedBytes);
                }
                
                s_scopeStack.Value.Pop();
            }
        }
    }
}
