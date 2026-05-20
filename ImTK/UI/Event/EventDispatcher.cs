using System;
using System.Collections.Generic;
using ImTK.Log;

namespace ImTK.UI
{
    public static class EventDispatcher
    {
        private static readonly LogContext s_log = new LogContext("EventDispatcher");
        private static readonly Queue<UIEventBase> s_eventQueue = new Queue<UIEventBase>();

        private static HashSet<VisualElement> s_dirtyBufferA = new HashSet<VisualElement>();
        private static HashSet<VisualElement> s_dirtyBufferB = new HashSet<VisualElement>();
        private static HashSet<VisualElement> s_currentDirtyBuffer = s_dirtyBufferA;

        internal static void MarkHierarchyDirty(VisualElement element)
        {
            if (element != null)
            {
                s_currentDirtyBuffer.Add(element);
            }
        }

        internal static void ProcessDirtyElements()
        {
            if (s_currentDirtyBuffer.Count == 0) return;

            var elementsToProcess = s_currentDirtyBuffer;
            s_currentDirtyBuffer = (s_currentDirtyBuffer == s_dirtyBufferA) ? s_dirtyBufferB : s_dirtyBufferA;
            s_currentDirtyBuffer.Clear();

            foreach (var element in elementsToProcess)
            {
                var evt = EventPool<HierarchyChangedEvent>.Get();
                evt.source = element;
                evt.current = element;
                evt.StopPropagation();

                try
                {
                    element.HandleEvent(evt);
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Exception occurred during HierarchyChangedEvent dispatch");
                }
                finally
                {
                    evt.Dispose();
                }
            }
            elementsToProcess.Clear();
        }

        public static void Enqueue(UIEventBase evt)
        {
            s_eventQueue.Enqueue(evt);
        }

        public static void ProcessQueue()
        {
            while (s_eventQueue.Count > 0)
            {
                UIEventBase evt = s_eventQueue.Dequeue();

                bool bubbles = evt.bubbles;

                try
                {
                    VisualElement currentElement = evt.source;

                    while (currentElement != null)
                    {
                        evt.current = currentElement;

                        // Optimize bubbling by checking if the element has callbacks for this event
                        // Note: For custom event overrides, developers must hook into HandleEvent,
                        // so we still always call HandleEvent but we can skip upwards propagation if no parents have listeners.
                        currentElement.HandleEvent(evt);

                        if (evt.IsPropagationStopped || !bubbles)
                        {
                            break;
                        }

                        // Bubble up
                        currentElement = currentElement.hierarchy.parent;
                    }
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Exception occurred during event dispatch: {evt.GetType().Name}");
                }
                finally
                {
                    evt.Dispose();
                }
            }
        }

        /// <summary>
        /// Clears all pending events in the queue safely. Useful for resetting state between isolated tests.
        /// </summary>
        public static void ClearQueue()
        {
            while (s_eventQueue.Count > 0)
            {
                var evt = s_eventQueue.Dequeue();
                evt.Dispose();
            }
            s_dirtyBufferA.Clear();
            s_dirtyBufferB.Clear();
        }
    }
}
