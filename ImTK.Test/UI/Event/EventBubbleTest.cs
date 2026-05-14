using ImTK.Test.Framework;
using ImTK.UI;

namespace ImTK.Test.UI.Event
{
    public class EventBubbleTest : IIntegrationTest
    {
        public string TestName => "Event Bubble Logic";
        public bool IsManualOnly => false;

        private class DummyEvent : UIEventBase
        {
            public bool bubbles = true;
            public override void Dispose() { }
        }

        public void Run()
        {
            var parent = new VisualElement();
            var child = new VisualElement();
            parent.hierarchy.Add(child);

            bool parentReceived = false;
            bool childReceived = false;

            parent.RegisterCallback<DummyEvent>(evt => parentReceived = true);
            child.RegisterCallback<DummyEvent>(evt => childReceived = true);

            var dummyEvent = EventPool<DummyEvent>.Get();
            dummyEvent.source = child;

            // Dispatch
            EventDispatcher.Enqueue(dummyEvent);
            EventDispatcher.ProcessQueue();

            ImTKAssert.IsTrue(childReceived, "Child should receive the event.");
            ImTKAssert.IsTrue(parentReceived, "Event should bubble up to parent.");
        }
    }
}
