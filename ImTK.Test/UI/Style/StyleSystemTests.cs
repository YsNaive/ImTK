using System.Collections.Generic;
using ImGuiNET;
using ImTK.Core;
using ImTK.UI;
using ImTK.Test.Framework;

namespace ImTK.Test.UI.Style
{
    public class StyleSystemTests : IHeadlessTest
    {
        public void Run()
        {
            TestHashedString();
            TestStyleValueImplicitOperators();
            TestClassListAndDirtyFlag();
            TestComputeStyleCascading();
        }

        private void TestHashedString()
        {
            var h1 = new HashedString("hello");
            var h2 = new HashedString("hello");
            var h3 = new HashedString("world");

            ImTKAssert.IsTrue(h1 == h2, "Identical strings should have identical hashes.");
            ImTKAssert.IsFalse(h1 == h3, "Different strings should have different hashes.");
        }

        private void TestStyleValueImplicitOperators()
        {
            StyleValue<Color> valValue = Color.Red;
            ImTKAssert.IsFalse(valValue.IsToken, "Assigned color should not be marked as token.");
            ImTKAssert.IsFalse(valValue.IsNull, "Assigned color should not be null.");
            ImTKAssert.AreEqual(Color.Red, valValue.Value, "Color value should match.");

            StyleValue<Color> valToken = "--bg-test";
            ImTKAssert.IsTrue(valToken.IsToken, "Assigned string should be marked as token.");
            ImTKAssert.IsFalse(valToken.IsNull, "Assigned string should not be null.");
            ImTKAssert.AreEqual(new HashedString("--bg-test"), valToken.Token, "Token name should match.");

            StyleValue<Color> valNull = StyleKeyword.Null;
            ImTKAssert.IsTrue(valNull.IsNull, "Assigned StyleKeyword.Null should be marked as null.");
        }

        private void TestClassListAndDirtyFlag()
        {
            var element = new VisualElement();
            ImTKAssert.IsFalse(element.classList.Has("test-class"), "Should not have class initially.");

            element.m_isStyleDirty = false; // Reset to false

            element.classList.Add("test-class");
            ImTKAssert.IsTrue(element.classList.Has("test-class"), "Class should be added.");
            ImTKAssert.IsTrue(element.m_isStyleDirty, "Adding class should mark style as dirty.");

            element.m_isStyleDirty = false;
            element.classList.Add("test-class"); // Adding existing
            ImTKAssert.IsFalse(element.m_isStyleDirty, "Adding existing class should not mark style as dirty.");

            element.classList.Remove("test-class");
            ImTKAssert.IsFalse(element.classList.Has("test-class"), "Class should be removed.");
            ImTKAssert.IsTrue(element.m_isStyleDirty, "Removing class should mark style as dirty.");
        }

        private void TestComputeStyleCascading()
        {
            // Setup Global
            var globalSheet = StyleSheet.Global;
            var globalBlock = globalSheet.AddBlock("test-btn");
            globalBlock.SetColor(ImGuiCol.Button, Color.Red);
            globalBlock.SetVar(ImGuiStyleVar.Alpha, 0.5f);

            // Setup Local
            var parent = new VisualElement();
            var localSheet = new StyleSheet();
            var localBlock = localSheet.AddBlock("test-btn");
            localBlock.SetColor(ImGuiCol.Button, Color.Green); // Overrides Global
            parent.localStyleSheet = localSheet;

            // Setup Element
            var element = new VisualElement();
            element.classList.Add("test-btn");
            parent.Add(element);

            // Setup Inline
            element.style.SetVar(ImGuiStyleVar.Alpha, 0.8f); // Overrides Global

            // Compute
            var computed = ComputeStyle.Overlay(element);

            bool foundColor = false;
            bool foundAlpha = false;

            foreach (var prop in computed)
            {
                if (prop.Type == StyleVarType.Color && prop.Key == (int)ImGuiCol.Button)
                {
                    ImTKAssert.AreEqual(Color.Green.u32, prop.ColorValue, "Local style should override global style.");
                    foundColor = true;
                }
                else if (prop.Type == StyleVarType.Float && prop.Key == (int)ImGuiStyleVar.Alpha)
                {
                    ImTKAssert.AreEqual(0.8f, prop.FloatValue, "Inline style should override global style.");
                    foundAlpha = true;
                }
            }

            ImTKAssert.IsTrue(foundColor, "Button color should be computed.");
            ImTKAssert.IsTrue(foundAlpha, "Alpha should be computed.");
        }
    }
}
