using System;

namespace ImTK.UI
{
    public class MouseEnterEvent : UIEventBase
    {
        public override void Dispose()
        {
            EventPool<MouseEnterEvent>.Release(this);
        }
    }

    public class MouseLeaveEvent : UIEventBase
    {
        public override void Dispose()
        {
            EventPool<MouseLeaveEvent>.Release(this);
        }
    }

    public class ClickEvent : UIEventBase
    {
        public override void Dispose()
        {
            EventPool<ClickEvent>.Release(this);
        }
    }
}
