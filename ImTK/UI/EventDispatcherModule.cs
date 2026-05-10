using ImTK.Core;

namespace ImTK.UI
{
    public class EventDispatcherModule : ImTKModule
    {
        protected internal override void OnInitializeSelf()
        {
        }

        protected internal override void OnInitializeDependencies()
        {
        }

        protected internal override void OnLogicUpdate()
        {
            EventDispatcher.ProcessQueue();
        }

        protected internal override void OnClose()
        {
        }
    }
}
