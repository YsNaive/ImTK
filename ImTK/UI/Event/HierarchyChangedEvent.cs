using System;

namespace ImTK.UI
{
    public class HierarchyChangedEvent : UIEventBase
    {
        public HierarchyChangedEvent()
        {
        }

        public override void Dispose()
        {
            EventPool<HierarchyChangedEvent>.Release(this);
        }
    }
}
