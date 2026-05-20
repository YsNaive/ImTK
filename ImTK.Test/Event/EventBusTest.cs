using ImTK.Core;
using ImTK.Event;
using ImTK.Test.Framework;

namespace ImTK.Test.Event
{
    public class EventBusTest : IHeadlessTest
    {
        public struct OnTestMessageEvent : IImTKEvent
        {
            public int Value;
        }

        private class DummyObject : ImTKObject
        {
            public int ReceivedValue { get; private set; } = 0;

            protected internal override void OnEnable()
            {
                base.OnEnable();
                SubscribeEvent<OnTestMessageEvent>(e => ReceivedValue = e.Value);
            }
        }

        public void Run()
        {
            // Reset dispatcher context for tests
            ImTKDispatcher.InitializeMainThread();
            ImTKEventBus.ClearAll();

            var dummy = new DummyObject();

            // Trigger OnEnable to register the subscription
            dummy.OnEnable();

            // Publish an event
            ImTKEventBus.Publish(new OnTestMessageEvent { Value = 42 });

            // Process the dispatcher queue (which is normally called in LateUpdate)
            ImTKDispatcher.ProcessQueue();

            ImTKAssert.AreEqual(42, dummy.ReceivedValue, "DummyObject should have received the event with value 42.");

            // Test auto-unbinding
            dummy.OnDisable(); // Should unbind

            ImTKEventBus.Publish(new OnTestMessageEvent { Value = 99 });
            ImTKDispatcher.ProcessQueue();

            ImTKAssert.AreEqual(42, dummy.ReceivedValue, "DummyObject should NOT have received the second event after OnDisable.");
        }
    }
}
