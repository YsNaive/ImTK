using System;
using System.Collections.Concurrent;
using System.Threading;
using ImTK.Log;

namespace ImTK.Event
{
    /// <summary>
    /// Thread dispatcher for the ImTK framework.
    /// Ensures that cross-thread tasks, such as UI updates and OpenGL calls,
    /// are safely executed on the main application thread during the LateUpdate phase.
    /// </summary>
    public static class ImTKDispatcher
    {
        private static readonly LogContext s_log = new LogContext("ImTKDispatcher");
        private static readonly ConcurrentQueue<Action> s_actionQueue = new ConcurrentQueue<Action>();
        private static int s_mainThreadId = -1;

        /// <summary>
        /// True if the current executing thread is the main thread.
        /// Useful for defensive assertions before making unsafe OpenGL or UI calls.
        /// </summary>
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == s_mainThreadId;

        /// <summary>
        /// Internal initialization called during the earliest phase of the application startup.
        /// </summary>
        internal static void InitializeMainThread()
        {
            s_mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Enqueues an action to be executed on the main thread.
        /// If the current thread is already the main thread, the action is executed immediately.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void Enqueue(Action action)
        {
            if (action == null) return;

            if (IsMainThread)
            {
                // Optimization: execute synchronously if already on main thread
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Exception occurred while executing action synchronously on main thread.");
                }
            }
            else
            {
                s_actionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Processes all enqueued actions.
        /// This should be called by the core lifecycle (e.g., during OnLateUpdate).
        /// </summary>
        internal static void ProcessQueue()
        {
            if (!IsMainThread)
            {
                s_log.Error("ProcessQueue must be called from the main thread.");
                return;
            }

            while (s_actionQueue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Exception occurred during dispatched action execution.");
                }
            }
        }
    }
}
