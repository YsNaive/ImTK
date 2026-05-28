using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ImTK.Log;

namespace ImTK.Event
{
    /// <summary>
    /// A type-safe, global event bus for inter-module communication.
    /// It works closely with ImTKDispatcher to ensure that event handlers are executed
    /// safely on the main thread, preventing concurrency issues when modifying UI or core state.
    /// </summary>
    public static class ImTKEventBus
    {


        // Use a dictionary where the key is the event type and the value is a list of delegates
        private static readonly ConcurrentDictionary<Type, List<Delegate>> s_subscribers = new ConcurrentDictionary<Type, List<Delegate>>();

        /// <summary>
        /// Registers a handler for a specific event type.
        /// This method is intended to be called by base classes (like ImTKObject or ImTKModule)
        /// to manage lifecycle-bound subscriptions. Direct usage is discouraged to avoid memory leaks.
        /// </summary>
        /// <typeparam name="T">The type of the event, must implement IImTKEvent.</typeparam>
        /// <param name="handler">The action to perform when the event is published.</param>
        /// <returns>An Action that can be invoked to unsubscribe the handler.</returns>
        public static Action GlobalSubscribe<T>(Action<T> handler) where T : IImTKEvent
        {
            var eventType = typeof(T);

            s_subscribers.AddOrUpdate(
                eventType,
                _ => new List<Delegate> { handler },
                (_, list) =>
                {
                    lock (list)
                    {
                        list.Add(handler);
                    }
                    return list;
                });

            return () => Unsubscribe(eventType, handler);
        }

        private static void Unsubscribe(Type eventType, Delegate handler)
        {
            if (s_subscribers.TryGetValue(eventType, out var list))
            {
                lock (list)
                {
                    list.Remove(handler);
                }
            }
        }

        /// <summary>
        /// Publishes an event to all registered subscribers.
        /// The execution of handlers is safely dispatched to the main thread via ImTKDispatcher.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="evt">The event instance containing the payload.</param>
        public static void Publish<T>(T evt) where T : IImTKEvent
        {
            var eventType = typeof(T);

            if (s_subscribers.TryGetValue(eventType, out var list))
            {
                Delegate[] handlersCopy;
                lock (list)
                {
                    if (list.Count == 0) return;
                    handlersCopy = list.ToArray();
                }

                // Dispatch the execution to the main thread
                ImTKDispatcher.Enqueue(() =>
                {
                    foreach (var handler in handlersCopy)
                    {
                        try
                        {
                            ((Action<T>)handler)(evt);
                        }
                        catch (Exception ex)
                        {
                            ImTKLog.Error(ex, $"Exception in event handler for {eventType.Name}");
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Clears all subscriptions. Usually called during application shutdown or test teardown.
        /// </summary>
        internal static void ClearAll()
        {
            s_subscribers.Clear();
        }
    }
}
