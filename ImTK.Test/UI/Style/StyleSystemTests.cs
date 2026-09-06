using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;
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
            TestStyleCascadingAndResolution();
            TestStyleInheritanceAndDiff();
            TestStyleRevertToThemeDefault();
            TestThemeTokenResolution();
            TestFontInheritanceAndSizing();
            TestDpiScalingHierarchyConsistency();
        }

        private bool HasColor(ImGuiStyleHandler handler, ImGuiCol col) => handler.GetActiveProperties().Any(p => p.dataType == StyleDataType.Color && p.key == (int)col);
        private bool HasVar(ImGuiStyleHandler handler, ImGuiStyleVar v) => handler.GetActiveProperties().Any(p => (p.dataType == StyleDataType.Float || p.dataType == StyleDataType.Vector2) && p.key == (int)v);
        private uint GetColor(ImGuiStyleHandler handler, ImGuiCol col) => handler.GetActiveProperties().First(p => p.dataType == StyleDataType.Color && p.key == (int)col).colorValue;
        private float GetFloat(ImGuiStyleHandler handler, ImGuiStyleVar v) => handler.GetActiveProperties().First(p => p.dataType == StyleDataType.Float && p.key == (int)v).floatValue;

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
            ImTKAssert.IsFalse(element.classList.Has(new HashedString("test-class")), "Should not have class initially.");

            element.m_isStyleDirty = false; // Reset to false

            element.classList.Add(new HashedString("test-class"));
            ImTKAssert.IsTrue(element.classList.Has(new HashedString("test-class")), "Class should be added.");
            ImTKAssert.IsTrue(element.m_isStyleDirty, "Adding class should mark style as dirty.");

            element.m_isStyleDirty = false;
            element.classList.Add(new HashedString("test-class")); // Adding existing
            ImTKAssert.IsFalse(element.m_isStyleDirty, "Adding existing class should not mark style as dirty.");

            element.classList.Remove(new HashedString("test-class"));
            ImTKAssert.IsFalse(element.classList.Has(new HashedString("test-class")), "Class should be removed.");
            ImTKAssert.IsTrue(element.m_isStyleDirty, "Removing class should mark style as dirty.");
        }

        private void TestStyleCascadingAndResolution()
        {
            var globalSheet = StyleSheet.Global;
            var globalBlock = globalSheet.AddBlock(new HashedString("cascade-test"));
            globalBlock.BackgroundColor(Color.Red);
            globalBlock.SetFloat(VisualElement.StyleKey.BorderRadius, 1.0f);

            var parent = new VisualElement();
            var localSheet = new StyleSheet();
            var localBlock = localSheet.AddBlock(new HashedString("cascade-test"));
            localBlock.BackgroundColor(Color.Green); 
            parent.localStyleSheet = localSheet;

            var element = new VisualElement();
            element.classList.Add(new HashedString("cascade-test"));
            parent.Add(element);

            element.style.SetFloat(VisualElement.StyleKey.BorderRadius, 5.0f); 

            RenderEngine.ComputeStyleRecursive(parent);

            ImTKAssert.IsTrue(!element.m_isStyleDirty, "Style should be computed.");
            
            ImTKAssert.IsTrue(HasColor(element.resolvedStyle, ImGuiCol.WindowBg), "WindowBg should be in resolved style");
            ImTKAssert.AreEqual(Color.Green.u32, GetColor(element.resolvedStyle, ImGuiCol.WindowBg), "Local sheet should override global sheet.");
            
            ImTKAssert.IsTrue(HasVar(element.resolvedStyle, ImGuiStyleVar.WindowRounding), "WindowRounding should be in resolved style");
            ImTKAssert.AreEqual(5.0f, GetFloat(element.resolvedStyle, ImGuiStyleVar.WindowRounding), "Inline style should override block styles.");
        }

        private void TestStyleInheritanceAndDiff()
        {
            var parent = new VisualElement();
            var child = new VisualElement();
            parent.Add(child);

            parent.style.SetColor(VisualElement.StyleKey.TextColor, Color.Blue);
            parent.style.SetFloat(VisualElement.StyleKey.BorderRadius, 10.0f);

            RenderEngine.ComputeStyleRecursive(parent);

            ImTKAssert.IsTrue(HasColor(parent.requiredStyle, ImGuiCol.Text), "Parent pushes TextColor");
            ImTKAssert.IsTrue(HasVar(parent.requiredStyle, ImGuiStyleVar.WindowRounding), "Parent pushes WindowRounding");

            ImTKAssert.IsFalse(HasColor(child.requiredStyle, ImGuiCol.Text), "Child should NOT push revert for Inheritable property.");
            ImTKAssert.IsTrue(HasVar(child.requiredStyle, ImGuiStyleVar.WindowRounding), "Child MUST push revert for Non-Inheritable property.");
        }

        private void TestStyleRevertToThemeDefault()
        {
            var parent = new VisualElement();
            var child = new VisualElement();
            parent.Add(child);

            float originalRounding = ImTKTheme.GlobalTheme.borderRadius;
            ImTKTheme.GlobalTheme.borderRadius = 7.7f;
            ImTKTheme.GlobalTheme.ApplyToImGui(); 

            parent.style.SetFloat(VisualElement.StyleKey.BorderRadius, 15.0f); 

            RenderEngine.ComputeStyleRecursive(parent);

            float revertValue = GetFloat(child.requiredStyle, ImGuiStyleVar.WindowRounding);
            ImTKAssert.AreEqual(7.7f, revertValue, "Revert value must match the global theme default, not 0.");

            ImTKTheme.GlobalTheme.borderRadius = originalRounding;
            ImTKTheme.GlobalTheme.ApplyToImGui();
        }

        private void TestThemeTokenResolution()
        {
            var element = new VisualElement();
            
            var tokenHash = new HashedString("--test-custom-color");
            ImTKTheme.GlobalTheme.SetColor(tokenHash, Color.Cyan);

            element.style.SetColor(VisualElement.StyleKey.BackgroundColor, "--test-custom-color");

            RenderEngine.ComputeStyleRecursive(element);

            ImTKAssert.IsTrue(HasColor(element.resolvedStyle, ImGuiCol.WindowBg), "WindowBg should be computed.");
            ImTKAssert.AreEqual(Color.Cyan.u32, GetColor(element.resolvedStyle, ImGuiCol.WindowBg), "Token should be correctly resolved to Cyan.");
        }

        private void TestFontInheritanceAndSizing()
        {
            var parent = new VisualElement();
            var pSheet = new StyleSheet();
            var pBlock = pSheet.AddBlock("font-parent");
            
            // Set font family in parent (using a fake token hash for testing)
            pBlock.Properties.Add(new StyleProperty { key = VisualElement.StyleKey.FontFamily.Hash, category = StyleCategory.HighLevelToken, dataType = StyleDataType.HashedString, tokenHash = 12345 });
            parent.localStyleSheet = pSheet;
            parent.classList.Add("font-parent");

            var child = new VisualElement();
            var cSheet = new StyleSheet();
            var cBlock = cSheet.AddBlock("font-child");
            
            // Set font size in child, but no font family
            cBlock.Properties.Add(new StyleProperty { key = VisualElement.StyleKey.FontSize.Hash, category = StyleCategory.HighLevelToken, dataType = StyleDataType.Float, floatValue = (float)FontSize.H1 });
            child.localStyleSheet = cSheet;
            child.classList.Add("font-child");
            parent.Add(child);

            RenderEngine.ComputeStyleRecursive(parent);

            bool HasFontFamily(ImGuiStyleHandler handler) => handler.GetActiveProperties().Any(p => p.key == ImGuiStyleHandler.s_fontFamilyImGuiKey.Hash);
            bool HasFontSize(ImGuiStyleHandler handler) => handler.GetActiveProperties().Any(p => p.key == ImGuiStyleHandler.s_fontSizeImGuiKey.Hash);

            // Verify parent has font family and font size (from GlobalTheme)
            ImTKAssert.IsTrue(HasFontFamily(parent.resolvedStyle), "Parent should have font family");
            ImTKAssert.IsTrue(HasFontSize(parent.resolvedStyle), "Parent should have font size from global theme");
            ImTKAssert.IsTrue(HasFontFamily(parent.requiredStyle), "Parent required style should push font family");

            // Verify child inherited font family, and has font size
            ImTKAssert.IsTrue(HasFontFamily(child.resolvedStyle), "Child should inherit font family");
            ImTKAssert.IsTrue(HasFontSize(child.resolvedStyle), "Child should have local font size");
            
            // The required style for child MUST push both if font size changed!
            ImTKAssert.IsTrue(HasFontFamily(child.requiredStyle), "Child required style MUST push font family to fetch correct font size!");
            ImTKAssert.IsTrue(HasFontSize(child.requiredStyle), "Child required style should push font size");
        }

        private void TestDpiScalingHierarchyConsistency()
        {
            float originalScale = RenderEngine.Context.CurrentDpiScale;
            try
            {
                RenderEngine.Context.CurrentDpiScale = 1.5f;

                var root = new VisualElement();
                var child1 = new VisualElement();
                var child2 = new VisualElement();
                var child3 = new VisualElement();
                root.Add(child1);
                child1.Add(child2);
                child2.Add(child3);

                RenderEngine.ComputeStyleRecursive(root);

                ImTKAssert.IsTrue(root.resolvedStyle.TryGetVector2((int)ImGuiStyleVar.FramePadding, out var rootPad), "Root must have FramePadding");
                ImTKAssert.IsTrue(child1.resolvedStyle.TryGetVector2((int)ImGuiStyleVar.FramePadding, out var child1Pad), "Child1 must have FramePadding");
                ImTKAssert.IsTrue(child2.resolvedStyle.TryGetVector2((int)ImGuiStyleVar.FramePadding, out var child2Pad), "Child2 must have FramePadding");
                ImTKAssert.IsTrue(child3.resolvedStyle.TryGetVector2((int)ImGuiStyleVar.FramePadding, out var child3Pad), "Child3 must have FramePadding");

                // Base theme padding scaled by 1.5x
                var basePadding = ImTKTheme.GlobalTheme.padding;
                ImTKAssert.AreEqual(basePadding.X * 1.5f, rootPad.X, "Root FramePadding.X must be scaled by 1.5x");
                ImTKAssert.AreEqual(basePadding.Y * 1.5f, rootPad.Y, "Root FramePadding.Y must be scaled by 1.5x");

                // Hierarchy must NOT exponentially compound DPI scaling (i.e. child3 == root, NOT root * 1.5^3)
                ImTKAssert.AreEqual(rootPad.X, child1Pad.X, "Child1 FramePadding.X must equal Root FramePadding.X");
                ImTKAssert.AreEqual(rootPad.Y, child1Pad.Y, "Child1 FramePadding.Y must equal Root FramePadding.Y");
                ImTKAssert.AreEqual(rootPad.X, child2Pad.X, "Child2 FramePadding.X must equal Root FramePadding.X");
                ImTKAssert.AreEqual(rootPad.Y, child2Pad.Y, "Child2 FramePadding.Y must equal Root FramePadding.Y");
                ImTKAssert.AreEqual(rootPad.X, child3Pad.X, "Child3 FramePadding.X must equal Root FramePadding.X");
                ImTKAssert.AreEqual(rootPad.Y, child3Pad.Y, "Child3 FramePadding.Y must equal Root FramePadding.Y");

                // Verify GetFrameHeight() consistency across hierarchy
                float rootFrameHeight = root.GetFrameHeight();
                float child3FrameHeight = child3.GetFrameHeight();
                ImTKAssert.AreEqual(rootFrameHeight, child3FrameHeight, "Root and Child3 GetFrameHeight() must be equal");
            }
            finally
            {
                RenderEngine.Context.CurrentDpiScale = originalScale;
            }
        }
    }
}
