using System;
using System.Collections.Generic;
using ImTK.Log;

namespace ImTK.UI
{
    public static class EventDispatcher
    {
        private static readonly LogContext s_log = new LogContext("EventDispatcher");
        private static readonly Queue<UIEventBase> s_eventQueue = new Queue<UIEventBase>();
        private static readonly HashSet<VisualElement> s_hierarchyDirtyElements = new HashSet<VisualElement>();

        internal static void MarkHierarchyDirty(VisualElement element)
        {
            if (element != null)
            {
                s_hierarchyDirtyElements.Add(element);
            }
        }

        internal static void ProcessDirtyElements()
        {
            if (s_hierarchyDirtyElements.Count == 0) return;

            VisualElement[] elementsToProcess = new VisualElement[s_hierarchyDirtyElements.Count];
            s_hierarchyDirtyElements.CopyTo(elementsToProcess);
            s_hierarchyDirtyElements.Clear();

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

                // Check if this event type does not bubble
                bool bubbles = true;
                if (evt is ValueChangedEvent valEvt)
                {
                    bubbles = valEvt.bubbles;
                }

                try
                {
                    VisualElement currentElement = evt.source;

                    while (currentElement != null)
                    {
                        evt.current = currentElement;
                        currentElement.HandleEvent(evt);

                        if (evt.IsPropagationStopped || !bubbles)
                        {
                            break;
                        }

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
            s_hierarchyDirtyElements.Clear();
        }
    }
}
