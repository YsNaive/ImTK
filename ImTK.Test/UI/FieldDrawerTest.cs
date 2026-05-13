using ImTK.UI;
using ImTK.Test.Framework;
using System;

namespace ImTK.Test.UI
{
    public class FieldDrawerTest : IHeadlessTest
    {
        public void Run()
        {
            TestValueChangedEvent();
            TestSetValueWithoutNotify();
            TestSetValueWithChanged();
            TestNotifyValueChanged();
            TestRegistryInheritance();
            TestFactoryModifiers();
        }

        private void TestValueChangedEvent()
        {
            var intEvt = ValueChangedEvent<int>.GetPooled(5, 10);
            ImTKAssert.IsFalse(intEvt.isInternalChange, "Int change should not be internal change.");
            intEvt.Dispose();

            var obj = new object();
            var refEvt = ValueChangedEvent<object>.GetPooled(obj, obj);
            ImTKAssert.IsTrue(refEvt.isInternalChange, "Same reference should be marked as internal change.");
            refEvt.Dispose();

            var obj2 = new object();
            var diffRefEvt = ValueChangedEvent<object>.GetPooled(obj, obj2);
            ImTKAssert.IsFalse(diffRefEvt.isInternalChange, "Different reference should not be internal change.");
            diffRefEvt.Dispose();
        }

        private void TestSetValueWithoutNotify()
        {
            var drawer = new IntField();
            drawer.value = 5;
            EventDispatcher.ProcessQueue(); // Clear queue

            bool eventFired = false;
            drawer.RegisterCallback<ValueChangedEvent<int>>(evt => eventFired = true);

            drawer.SetValueWithoutNotify(10);
            EventDispatcher.ProcessQueue();

            ImTKAssert.IsFalse(eventFired, "SetValueWithoutNotify should not fire event.");
            ImTKAssert.AreEqual(10, drawer.value, "Value should be updated.");
        }

        private void TestSetValueWithChanged()
        {
            var drawer = new IntField();
            drawer.value = 5;
            EventDispatcher.ProcessQueue(); // Ensure initial set event is cleared

            bool eventFired = false;
            drawer.RegisterCallback<ValueChangedEvent<int>>(evt =>
            {
                eventFired = true;
                ImTKAssert.AreEqual(5, evt.previousValue);
                ImTKAssert.AreEqual(5, evt.newValue);
            });

            drawer.SetValueWithChanged(5);
            EventDispatcher.ProcessQueue(); // Dispatch the event synchronously for test

            ImTKAssert.IsTrue(eventFired, "SetValueWithChanged should fire event even if value is the same.");
        }

        private void TestNotifyValueChanged()
        {
            var drawer = new ObjectDrawer();
            var dummy = new object();
            drawer.value = dummy;
            EventDispatcher.ProcessQueue();

            bool eventFired = false;
            drawer.RegisterCallback<ValueChangedEvent<object>>(evt =>
            {
                eventFired = true;
                ImTKAssert.IsTrue(evt.isInternalChange, "NotifyValueChanged should trigger internal change flag.");
                ImTKAssert.AreEqual(dummy, evt.newValue);
            });

            drawer.NotifyValueChanged();
            EventDispatcher.ProcessQueue();

            ImTKAssert.IsTrue(eventFired, "NotifyValueChanged should fire event.");
        }

        private class BaseType { }
        private class ChildType : BaseType { }

        [CustomFieldDrawer(typeof(BaseType), allowInheritType: true)]
        private class BaseTypeDrawer : FieldDrawer<BaseType> { }

        private void TestRegistryInheritance()
        {
            var drawerType = FieldDrawerRegistry.FindDrawerType(typeof(ChildType), null);
            ImTKAssert.AreEqual(typeof(BaseTypeDrawer), drawerType, "Registry should find parent drawer if allowInheritType is true.");
        }

        private class DummyAttribute : Attribute { }

        [CustomFieldDrawer(typeof(string), requiredModifier: typeof(DummyAttribute))]
        private class SpecialStringField : FieldDrawer<string>
        {
            public bool applied = false;
            public override void ApplyModifier(Attribute modifier)
            {
                if (modifier is DummyAttribute) applied = true;
            }
        }

        private void TestFactoryModifiers()
        {
            var drawer = FieldDrawerFactory.Create()
                .FromType(typeof(string))
                .AddModifier(new DummyAttribute())
                .Build();

            ImTKAssert.NotNull(drawer, "Factory should build drawer based on modifier.");
            ImTKAssert.IsTrue(drawer is SpecialStringField, "Factory should pick the one with matching modifier.");
            ImTKAssert.IsTrue(((SpecialStringField)drawer).applied, "Factory should call ApplyModifier.");
        }
    }
}
