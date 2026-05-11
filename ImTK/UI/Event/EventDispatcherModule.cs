using ImTK.Core;

namespace ImTK.UI
{
    public class EventDispatcherModule : ImTKModule
    {
        protected EventDispatcherModule() { }

        protected internal override void OnInitializeSelf()
        {
        }

        protected internal override void OnInitializeDependencies()
        {
        }

        protected internal override void OnLogicUpdate()
        {
            EventDispatcher.ProcessDirtyElements();
            EventDispatcher.ProcessQueue();
        }

        protected internal override void OnClose()
        {
        }
    }
}
