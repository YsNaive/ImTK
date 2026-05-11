using System;
using System.Collections.Generic;

namespace ImTK.UI
{
    public static class EventPool<T> where T : UIEventBase, new()
    {
        private static readonly Stack<T> s_pool = new Stack<T>();

        public static T Get()
        {
            T evt;
            if (s_pool.Count > 0)
            {
                evt = s_pool.Pop();
            }
            else
            {
                evt = new T();
            }
            evt.Init();
            return evt;
        }

        public static void Release(T evt)
        {
            evt.Init(); // Clear state
            s_pool.Push(evt);
        }
    }
}
