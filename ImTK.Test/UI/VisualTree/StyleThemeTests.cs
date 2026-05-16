using System;
using System.Collections;
using System.Reflection;
using ImGuiNET;
using ImTK.UI;
using ImTK.Test.Framework;

namespace ImTK.Test.UI
{
    public class StyleThemeTests : IHeadlessTest
    {
        public void Run()
        {
            TestStyleOverrides();
            TestThemeFallback();
            TestSetThemePropagation();
        }

        private void TestStyleOverrides()
        {
            VisualElement element = new VisualElement();
            var style = element.style;

            // Initially null
            ImTKAssert.IsTrue(style.textColor == null, "Text color should be null initially");
            ImTKAssert.IsTrue(style.backgroundColor == null, "Background color should be null initially");

            // Set override
            style.textColor = Color.Red;
            ImTKAssert.IsTrue(style.textColor.HasValue && style.textColor.Value.Value == Color.Red, "Text color should be Red");

            style.backgroundColor = Color.Blue;
            ImTKAssert.IsTrue(style.backgroundColor.HasValue && style.backgroundColor.Value.Value == Color.Blue, "Background color should be Blue");

            // Clear override
            style.textColor = null;
            ImTKAssert.IsTrue(style.textColor == null, "Text color should be null after clearing");

            style.backgroundColor = null;
            ImTKAssert.IsTrue(style.backgroundColor == null, "Background color should be null after clearing");
        }

        private void TestThemeFallback()
        {
            ImTKTheme parentTheme = new ImTKTheme();
            parentTheme.SetColorToken("--background-1", Color.Blue);
            parentTheme.SetColorToken("--text-primary", Color.Green);

            ImTKTheme childTheme = new ImTKTheme { parent = parentTheme };
            childTheme.SetColorToken("--text-primary", Color.Red); // Override parent's text color

            // Fallback to parent
            childTheme.TryGetColorToken(new ImTK.Core.HashedString("--background-1").Hash, out var bg1);
            ImTKAssert.AreEqual(Color.Blue, bg1, "Should fallback to parent's Background1");

            // Override parent
            childTheme.TryGetColorToken(new ImTK.Core.HashedString("--text-primary").Hash, out var textP);
            ImTKAssert.AreEqual(Color.Red, textP, "Should override parent's TextPrimary");
        }

        private class ThemeTestElement : VisualElement
        {
            public ThemeTestElement()
            {
                style.backgroundColor = "--background-1";
            }
        }

        private void TestSetThemePropagation()
        {
            ThemeTestElement root = new ThemeTestElement();
            ThemeTestElement child1 = new ThemeTestElement();
            ThemeTestElement child2 = new ThemeTestElement();

            root.Add(child1);
            root.Add(child2);

            ImTKTheme customTheme = new ImTKTheme();
            customTheme.SetColorToken("--background-1", Color.Magenta);

            root.theme = customTheme;

            // Trigger computation to verify cascading
            root.Render();
            child1.Render();
            child2.Render();

            ImTKAssert.AreEqual(customTheme, root.theme, "Root theme should be updated.");
            ImTKAssert.AreEqual(customTheme, child1.theme, "Child1 theme should inherit root theme.");
        }
    }
}
