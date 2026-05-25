using System;
using ImTK.Test.Framework;
using ImTK.UI;

namespace ImTK.Test.UI.Element
{
    public class BasicElementsTest : IHeadlessTest
    {
        public void Run()
        {
            TestTextElement();
            TestBoolDrawer();
            TestStringDrawer();
        }

        private void TestTextElement()
        {
            var textElem = new TextElement("Hello");
            ImTKAssert.AreEqual("Hello", textElem.text, "TextElement initialization failed.");

            textElem.text = "World";
            ImTKAssert.AreEqual("World", textElem.text, "TextElement setter failed.");
        }

        private void TestBoolDrawer()
        {
            var boolDrawer = new BoolDrawer { label = "Toggle", value = false };
            ImTKAssert.AreEqual(false, boolDrawer.value, "BoolDrawer initial value failed.");

            bool eventFired = false;
            boolDrawer.RegisterValueChangedCallback(evt => {
                eventFired = true;
                ImTKAssert.AreEqual(false, evt.previousValue, "BoolDrawer prev value mismatch.");
                ImTKAssert.AreEqual(true, evt.newValue, "BoolDrawer new value mismatch.");
            });

            boolDrawer.value = true;
            EventDispatcher.ProcessQueue();
            ImTKAssert.IsTrue(eventFired, "BoolDrawer ValueChangedEvent not fired.");
            ImTKAssert.AreEqual(true, boolDrawer.value, "BoolDrawer value update failed.");
        }

        private void TestStringDrawer()
        {
            var stringDrawer = new StringDrawer { label = "Input" };
            stringDrawer.SetValueWithoutNotify("Init");
            ImTKAssert.AreEqual("Init", stringDrawer.value, "StringDrawer initial value failed.");

            bool eventFired = false;
            stringDrawer.RegisterValueChangedCallback(evt => {
                eventFired = true;
                ImTKAssert.AreEqual("Init", evt.previousValue, "StringDrawer prev value mismatch.");
                ImTKAssert.AreEqual("NewText", evt.newValue, "StringDrawer new value mismatch.");
            });

            stringDrawer.value = "NewText";
            EventDispatcher.ProcessQueue();
            ImTKAssert.IsTrue(eventFired, "StringDrawer ValueChangedEvent not fired.");
            ImTKAssert.AreEqual("NewText", stringDrawer.value, "StringDrawer value update failed.");
        }
    }
}
