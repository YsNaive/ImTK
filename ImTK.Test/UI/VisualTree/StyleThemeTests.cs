using System;
using System.Reflection;
using System.Collections;
using ImTK.UI;
using ImTK.Test.Framework;
using ImGuiNET;

namespace ImTK.Test.UI.VisualTree
{
    public class StyleThemeTests : IHeadlessTest
    {
        public void Run()
        {
            TestLazyInitialization();
            TestOverrideStyle();
            TestThemeFallback();
            TestSetThemePropagation();
        }

        private void TestLazyInitialization()
        {
            VisualElement element = new VisualElement();
            VisualElementStyle style = element.style;

            // Use reflection to access internal lists to ensure they are null initially
            var overrideField = typeof(VisualElementStyle).GetField("m_overrideStyles", BindingFlags.Instance | BindingFlags.NonPublic);
            var themeField = typeof(VisualElementStyle).GetField("m_themeStyles", BindingFlags.Instance | BindingFlags.NonPublic);

            ImTKAssert.IsTrue(overrideField.GetValue(style) == null, "Override list should be null initially to save memory");
            ImTKAssert.IsTrue(themeField.GetValue(style) == null, "Theme list should be null initially to save memory");

            // Accessing nullable property should return null and not initialize
            ImTKAssert.IsFalse(style.textColor.HasValue, "textColor should be null");
            ImTKAssert.IsTrue(overrideField.GetValue(style) == null, "Override list should still be null after reading property");
        }

        private void TestOverrideStyle()
        {
            VisualElement element = new VisualElement();
            VisualElementStyle style = element.style;

            // Set override
            style.textColor = Color.Red;
            ImTKAssert.IsTrue(style.textColor.HasValue, "textColor should have value");
            ImTKAssert.AreEqual(Color.Red, style.textColor.Value, "textColor mismatch");

            // Clear override
            style.textColor = null;
            ImTKAssert.IsFalse(style.textColor.HasValue, "textColor should be null after clearing");

            // The list should be initialized but empty (or entry removed)
            var overrideField = typeof(VisualElementStyle).GetField("m_overrideStyles", BindingFlags.Instance | BindingFlags.NonPublic);
            var list = overrideField.GetValue(style) as IList;
            ImTKAssert.NotNull(list, "List should be initialized");
            ImTKAssert.AreEqual(0, list.Count, "List should be empty after clearing all styles");
        }

        private void TestThemeFallback()
        {
            ImTKTheme parentTheme = new ImTKTheme();
            parentTheme.Background1 = Color.Blue;
            parentTheme.TextPrimary = Color.Green;

            ImTKTheme childTheme = new ImTKTheme { parent = parentTheme };
            childTheme.TextPrimary = Color.Red; // Override parent's text color

            // Fallback to parent
            ImTKAssert.AreEqual(Color.Blue, childTheme.Background1, "Should fallback to parent's Background1");

            // Override parent
            ImTKAssert.AreEqual(Color.Red, childTheme.TextPrimary, "Should override parent's TextPrimary");

            // Fallback to absolute default when parent doesn't have it either (not set)
            ImTKAssert.AreEqual(new Color(0.15f, 0.15f, 0.15f, 1f), childTheme.Background2, "Should fallback to absolute default Background2");
        }

        private class ThemeTestElement : VisualElement
        {
            public int ApplyThemeCallCount = 0;
            protected override void ApplyTheme(ImTKTheme theme)
            {
                base.ApplyTheme(theme);
                ApplyThemeCallCount++;
                style.backgroundColor = theme.Background1; // Manual mapping for test
            }
        }

        private void TestSetThemePropagation()
        {
            ThemeTestElement root = new ThemeTestElement();
            ThemeTestElement child1 = new ThemeTestElement();
            ThemeTestElement child2 = new ThemeTestElement();

            root.Add(child1);
            root.Add(child2);

            ImTKTheme theme = new ImTKTheme();
            theme.TextPrimary = Color.Magenta;
            theme.Background1 = Color.Cyan;

            root.SetTheme(theme);

            // Verify call counts
            ImTKAssert.AreEqual(1, root.ApplyThemeCallCount, "Root ApplyTheme should be called once");
            ImTKAssert.AreEqual(1, child1.ApplyThemeCallCount, "Child1 ApplyTheme should be called once");
            ImTKAssert.AreEqual(1, child2.ApplyThemeCallCount, "Child2 ApplyTheme should be called once");

            // Verify theme list population (internal via base method mapping)
            var themeField = typeof(VisualElementStyle).GetField("m_themeStyles", BindingFlags.Instance | BindingFlags.NonPublic);
            var rootThemeList = themeField.GetValue(root.style) as IList;
            ImTKAssert.NotNull(rootThemeList, "Root theme list should be initialized");
            ImTKAssert.IsTrue(rootThemeList.Count > 0, "Root theme list should have entries");

            // Verify explicit mapping in our derived class
            ImTKAssert.AreEqual(Color.Cyan, root.style.backgroundColor.Value, "Root background should map to Theme's Background1");
            ImTKAssert.AreEqual(Color.Cyan, child1.style.backgroundColor.Value, "Child background should map to Theme's Background1");
        }
    }
}
