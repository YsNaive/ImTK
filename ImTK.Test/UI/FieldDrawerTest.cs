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
            // Value Type
            var intEvt = ValueChangedEvent<int>.GetPooled(5, 10);
            ImTKAssert.IsFalse(intEvt.isInternalChange, "Int change should not be internal change.");
            intEvt.Dispose();

            // Ref Type - Same Reference
            var obj = new object();
            var refEvt = ValueChangedEvent<object>.GetPooled(obj, obj);
            ImTKAssert.IsTrue(refEvt.isInternalChange, "Same reference should be marked as internal change.");
            refEvt.Dispose();

            // Ref Type - Different Reference
            var obj2 = new object();
            var diffRefEvt = ValueChangedEvent<object>.GetPooled(obj, obj2);
            ImTKAssert.IsFalse(diffRefEvt.isInternalChange, "Different reference should not be internal change.");
            diffRefEvt.Dispose();
        }

        private void TestSetValueWithoutNotify()
        {
            var drawer = new IntField();
            drawer.value = 5;
            bool eventFired = false;
            drawer.RegisterCallback<ValueChangedEvent<int>>(evt => eventFired = true);

            drawer.SetValueWithoutNotify(10);

            ImTKAssert.IsFalse(eventFired, "SetValueWithoutNotify should not fire event.");
            ImTKAssert.AreEqual(10, drawer.value, "Value should be updated.");
        }

        private void TestSetValueWithChanged()
        {
            var drawer = new IntField();
            drawer.value = 5;
            bool eventFired = false;
            drawer.RegisterCallback<ValueChangedEvent<int>>(evt =>
            {
                eventFired = true;
                ImTKAssert.AreEqual(5, evt.previousValue);
                ImTKAssert.AreEqual(5, evt.newValue); // Since we pass 5
            });

            // Even if same value, SetValueWithChanged ignores equality check
            drawer.SetValueWithChanged(5);

            ImTKAssert.IsTrue(eventFired, "SetValueWithChanged should fire event even if value is the same.");
        }

        private void TestNotifyValueChanged()
        {
            var drawer = new ObjectDrawer();
            var dummy = new object();
            drawer.value = dummy;

            bool eventFired = false;
            drawer.RegisterCallback<ValueChangedEvent<object>>(evt =>
            {
                eventFired = true;
                ImTKAssert.IsTrue(evt.isInternalChange, "NotifyValueChanged should trigger internal change flag.");
                ImTKAssert.AreEqual(dummy, evt.newValue);
            });

            drawer.NotifyValueChanged();

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
