using System;
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
            public string Name;
            public ProfilerNode Parent;
            public Dictionary<string, ProfilerNode> Children = new Dictionary<string, ProfilerNode>();
            
            // Callers that recorded into this scope
            public HashSet<string> Callers = new HashSet<string>();

            // Max 60 seconds at 60fps = 3600 frames
            public float[] History = new float[3600];
            public float[] GcHistory = new float[3600];
            
            public int Head = 0;
            public int SampleCount = 0; 
            
            public float CurrentFrameAccumulator = 0f;
            public float CurrentFrameGcAccumulator = 0f;

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

        public static ProfilerNode Root => s_root;

        /// <summary>
        /// Begins a profiling scope. The scope will automatically record its duration when disposed.
        /// </summary>
        public static ProfilerScope Scope(string name, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "", [System.Runtime.CompilerServices.CallerLineNumber] int callerLine = 0)
        {
            return new ProfilerScope(name, callerFile, callerLine);
        }

        /// <summary>
        /// Begins a profiling scope at an absolute path from the root, bypassing the current thread stack.
        /// </summary>
        public static ProfilerScope ScopeAbsolute(string category, string name, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "", [System.Runtime.CompilerServices.CallerLineNumber] int callerLine = 0)
        {
            return new ProfilerScope(category, name, callerFile, callerLine);
        }

        /// <summary>
        /// Commits all accumulated metrics for the current frame into the history buffer.
        /// </summary>
        internal static void EndFrame()
        {
            CommitRecursive(s_root);
        }

        private static float CommitRecursive(ProfilerNode node)
        {
            float childrenTimeSum = 0f;
            float childrenGcSum = 0f;
            foreach (var child in node.Children.Values)
            {
                childrenTimeSum += CommitRecursive(child);
                childrenGcSum += child.GcHistory[child.Head == 0 ? 3599 : child.Head - 1]; // Approximate for virtual nodes, though it's complex. Actually, just add the current frame accumulators.
            }

            if (node.CurrentFrameAccumulator == 0f && childrenTimeSum > 0f)
            {
                node.CurrentFrameAccumulator = childrenTimeSum;
            }
            if (node.CurrentFrameGcAccumulator == 0f && childrenGcSum > 0f)
            {
                node.CurrentFrameGcAccumulator = childrenGcSum; // But wait, GC is in Bytes.
            }
            
            // To be accurate for GC: we should have returned a struct. But let's just do it directly.
            float timeVal = node.CurrentFrameAccumulator;
            float gcVal = node.CurrentFrameGcAccumulator;

            node.History[node.Head] = timeVal;
            node.GcHistory[node.Head] = gcVal;
            
            node.Head = (node.Head + 1) % 3600;
            if (node.SampleCount < 3600) node.SampleCount++;
            
            node.CurrentFrameAccumulator = 0f;
            node.CurrentFrameGcAccumulator = 0f;

            return timeVal;
        }

        public struct ProfilerScope : IDisposable
        {
            private readonly ProfilerNode m_parentNode;
            private readonly ProfilerNode m_currentNode;
            private readonly long m_startTicks;
            private readonly long m_startGcBytes;
            private readonly bool m_isAbsolute;

            public ProfilerScope(string name, string callerFile, int callerLine)
            {
                m_isAbsolute = false;
                var stack = s_scopeStack.Value;
                m_parentNode = stack.Peek();

                if (!m_parentNode.Children.TryGetValue(name, out m_currentNode))
                {
                    m_currentNode = new ProfilerNode { Name = name, Parent = m_parentNode };
                    m_parentNode.Children[name] = m_currentNode;
                }
                stack.Push(m_currentNode);

                if (!string.IsNullOrEmpty(callerFile)) m_currentNode.Callers.Add($"{System.IO.Path.GetFileName(callerFile)}:{callerLine}");

                m_startGcBytes = GC.GetAllocatedBytesForCurrentThread();
                m_startTicks = s_stopwatch.ElapsedTicks;
            }

            public ProfilerScope(string category, string name, string callerFile, int callerLine)
            {
                m_isAbsolute = true;
                var stack = s_scopeStack.Value;
                m_parentNode = stack.Peek(); // Saved for reference to maintain push/pop balance

                if (!s_root.Children.TryGetValue(category, out var catNode))
                {
                    catNode = new ProfilerNode { Name = category, Parent = s_root };
                    s_root.Children[category] = catNode;
                }

                if (!catNode.Children.TryGetValue(name, out m_currentNode))
                {
                    m_currentNode = new ProfilerNode { Name = name, Parent = catNode };
                    catNode.Children[name] = m_currentNode;
                }
                
                stack.Push(m_currentNode);
                
                if (!string.IsNullOrEmpty(callerFile)) m_currentNode.Callers.Add($"{System.IO.Path.GetFileName(callerFile)}:{callerLine}");

                m_startGcBytes = GC.GetAllocatedBytesForCurrentThread();
                m_startTicks = s_stopwatch.ElapsedTicks;
            }

            public void Dispose()
            {
                long elapsedTicks = s_stopwatch.ElapsedTicks - m_startTicks;
                long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - m_startGcBytes;
                
                float ms = (float)elapsedTicks / Stopwatch.Frequency * 1000f;
                float kb = allocatedBytes / 1024f;
                
                m_currentNode.CurrentFrameAccumulator += ms;
                m_currentNode.CurrentFrameGcAccumulator += kb;
                
                s_scopeStack.Value.Pop();
            }
        }
    }
}
