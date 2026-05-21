using ImTK.UI;
using ImTK.Test.Framework;
using System;

namespace ImTK.Test.UI.Element.FieldDrawer
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
            TestObjectDrawerDeepNesting();
            TestFactoryFallback();
            TestCircularUpdateProtection();
            TestLayoutMode();
        }

        private class NestedClass
        {
            public int InnerVal = 42;
        }

        private class ParentClass
        {
            public string Name = "Parent";
            public NestedClass Child = new NestedClass();
        }

        private void TestObjectDrawerDeepNesting()
        {
            var drawer = new ObjectDrawer();
            var data = new ParentClass();
            drawer.value = data;

            // Rebuild happens inside Render or manually if we force it.
            // Since we're headless, we can mock the render or rely on value setter
            // the setter calls RebuildChildren in ObjectDrawer.

            // Check if children are created
            int childCount = 0;
            foreach (var child in drawer.hierarchy.Children())
            {
                childCount++;
            }
            ImTKAssert.IsTrue(childCount > 0, "ObjectDrawer should generate child elements for properties.");
        }

        private class UnknownType { }

        private void TestFactoryFallback()
        {
            // Factory should fallback to ObjectDrawer or return null when allowInheritType allows object
            // object is the root, ObjectDrawer is registered with allowInheritType = true
            var drawer = FieldDrawerFactory.Create().FromType(typeof(UnknownType)).Build();
            ImTKAssert.NotNull(drawer, "Factory should fallback and return a drawer (likely ObjectDrawer) for unknown types due to inherit object.");
            ImTKAssert.IsTrue(drawer is ObjectDrawer, "Fallback drawer should be ObjectDrawer.");
        }

        private void TestCircularUpdateProtection()
        {
            var drawer = new IntDrawer();
            drawer.value = 10;
            EventDispatcher.ProcessQueue();

            int eventCount = 0;
            drawer.RegisterCallback<ValueChangedEvent<int>>(evt =>
            {
                eventCount++;
                // Simulate data binding triggering a set without notify
                drawer.SetValueWithoutNotify(evt.newValue);
            });

            drawer.value = 20; // Trigger once
            EventDispatcher.ProcessQueue();

            ImTKAssert.AreEqual(1, eventCount, "Event should only fire once, preventing circular loop.");
        }

        private void TestLayoutMode()
        {
            var drawer = new IntDrawer();
            drawer.layoutMode = DrawerLayoutMode.Expand;
            ImTKAssert.AreEqual(DrawerLayoutMode.Expand, drawer.layoutMode, "LayoutMode should be modifiable.");

            var objDrawer = new ObjectDrawer();
            ImTKAssert.AreEqual(DrawerLayoutMode.Expand, objDrawer.layoutMode, "ObjectDrawer should default to Expand mode.");
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
            var drawer = new IntDrawer();
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
            var drawer = new IntDrawer();
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
