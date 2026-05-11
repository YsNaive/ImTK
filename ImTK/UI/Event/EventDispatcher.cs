using System;
using System.Collections.Generic;
using ImTK.Log;

namespace ImTK.UI
{
    public static class EventDispatcher
    {
        private static readonly LogContext s_log = new LogContext("EventDispatcher");
        private static readonly Queue<UIEventBase> s_eventQueue = new Queue<UIEventBase>();

        public static void Enqueue(UIEventBase evt)
        {
            s_eventQueue.Enqueue(evt);
        }

        public static void ProcessQueue()
        {
            while (s_eventQueue.Count > 0)
            {
                UIEventBase evt = s_eventQueue.Dequeue();

                try
                {
                    VisualElement currentElement = evt.source;

                    while (currentElement != null)
                    {
                        evt.current = currentElement;
                        currentElement.HandleEvent(evt);

                        if (evt.IsPropagationStopped)
                        {
                            break;
                        }

                        currentElement = currentElement.parent;
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
    }
}
