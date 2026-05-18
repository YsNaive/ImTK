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
            TestCheckBox();
            TestTextField();
        }

        private void TestTextElement()
        {
            var textElem = new TextElement("Hello");
            ImTKAssert.AreEqual("Hello", textElem.text, "TextElement initialization failed.");

            textElem.text = "World";
            ImTKAssert.AreEqual("World", textElem.text, "TextElement setter failed.");
        }

        private void TestCheckBox()
        {
            var checkBox = new CheckBox("Toggle", false);
            ImTKAssert.AreEqual(false, checkBox.value, "CheckBox initial value failed.");

            bool eventFired = false;
            checkBox.onValueChanged += evt => {
                eventFired = true;
                ImTKAssert.AreEqual(false, evt.previousValue, "CheckBox prev value mismatch.");
                ImTKAssert.AreEqual(true, evt.newValue, "CheckBox new value mismatch.");
            };

            checkBox.value = true;
            ImTKAssert.IsTrue(eventFired, "CheckBox onValueChanged event not fired.");
            ImTKAssert.AreEqual(true, checkBox.value, "CheckBox value update failed.");
        }

        private void TestTextField()
        {
            var textField = new TextField("Input", "Init");
            ImTKAssert.AreEqual("Init", textField.value, "TextField initial value failed.");

            bool eventFired = false;
            textField.onValueChanged += evt => {
                eventFired = true;
                ImTKAssert.AreEqual("Init", evt.previousValue, "TextField prev value mismatch.");
                ImTKAssert.AreEqual("NewText", evt.newValue, "TextField new value mismatch.");
            };

            textField.value = "NewText";
            ImTKAssert.IsTrue(eventFired, "TextField onValueChanged event not fired.");
            ImTKAssert.AreEqual("NewText", textField.value, "TextField value update failed.");
        }
    }
}
