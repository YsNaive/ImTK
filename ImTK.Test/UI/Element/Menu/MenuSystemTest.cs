using System;
using System.Linq;
using ImTK.Test.Framework;
using ImTK.UI;
using ImTK.Log;

namespace ImTK.Test.UI.Element.Menu
{
    public class MenuSystemTest : IHeadlessTest
    {
        public string TestName => "Menu System Logic Test";

        public void Run()
        {
            TestPathParsing();
            TestMenuConstraint();
            TestMenuPrioritySorting();
            TestAddMenuStatic();
        }

        private void TestPathParsing()
        {
            var menu = new MenuView("Root");
            var item = menu.AddItem("File/Recent/ProjectA", null, 10);

            ImTKAssert.NotNull(item, "Item should be created.");
            ImTKAssert.AreEqual("ProjectA", item.name, "Terminal node name should be ProjectA");

            // Verify hierarchy
            var children = menu.hierarchy.Children().ToList();
            ImTKAssert.AreEqual(1, children.Count, "Root should have 1 child");
            ImTKAssert.IsTrue(children[0] is MenuView, "First child should be a MenuView");
            var fileMenu = children[0] as MenuView;
            ImTKAssert.AreEqual("File", fileMenu.name, "Node name should be File");

            var fileChildren = fileMenu.hierarchy.Children().ToList();
            ImTKAssert.AreEqual(1, fileChildren.Count, "File should have 1 child");
            var recentMenu = fileChildren[0] as MenuView;
            ImTKAssert.AreEqual("Recent", recentMenu.name, "Node name should be Recent");

            var recentChildren = recentMenu.hierarchy.Children().ToList();
            ImTKAssert.AreEqual(1, recentChildren.Count, "Recent should have 1 child");
            ImTKAssert.IsTrue(recentChildren[0] is MenuItem, "Leaf node should be MenuItem");
            ImTKAssert.AreEqual(item, recentChildren[0], "Leaf node should match the returned item");

            // Test conflict
            var conflictItem = menu.AddItem("File/Recent", null, 0);
            ImTKAssert.IsTrue(conflictItem == null, "Adding an item to a path that is already a View should return null due to conflict.");
        }

        private void TestMenuConstraint()
        {
            var item = new MenuItem("Terminal");
            bool exceptionThrown = false;
            try
            {
                // This triggers the VisualElement Add logic which accesses contentContainer
                var dummy = new VisualElement();
                item.Add(dummy);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            ImTKAssert.IsFalse(exceptionThrown, "Adding a child to MenuItem should no longer throw an exception, but logs an error instead.");
        }

        private void TestMenuPrioritySorting()
        {
            var menu = new MenuView("Root");
            menu.AddItem("Item C", null, 30);
            menu.AddItem("Item A", null, 10);
            menu.AddItem("Item B", null, 20);

            // Trigger hierarchy update event
            var evt = EventPool<HierarchyChangedEvent>.Get();
            menu.HandleEvent(evt);
            evt.Dispose();

            // We need to access m_sortedMenuElements, but it's private.
            // As an integration test, we can trust the render output visually, or use reflection to verify the sorted order.
            var fieldInfo = typeof(MenuView).GetField("m_sortedMenuElements", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var sortedList = fieldInfo.GetValue(menu) as System.Collections.Generic.List<IMenuElement>;

            ImTKAssert.NotNull(sortedList, "Sorted list should not be null.");
            ImTKAssert.AreEqual(3, sortedList.Count, "Should have 3 sorted elements.");
            ImTKAssert.AreEqual("Item A", sortedList[0].name, "First item should be A");
            ImTKAssert.AreEqual("Item B", sortedList[1].name, "Second item should be B");
            ImTKAssert.AreEqual("Item C", sortedList[2].name, "Third item should be C");
        }

        private void TestAddMenuStatic()
        {
            var root = new MenuView("Root");
            var dynamicView = new MenuView("DynamicTools");
            dynamicView.AddItem("Tool 1", null, 0);

            root.AddMenu("Window/Tools", dynamicView, 50);

            var windowMenu = root.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == "Window") as MenuView;
            ImTKAssert.NotNull(windowMenu, "Window menu should be created.");

            var toolsMenu = windowMenu.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == "Tools") as MenuView;
            ImTKAssert.NotNull(toolsMenu, "Tools menu should be created.");

            var attachedView = toolsMenu.hierarchy.Children().FirstOrDefault() as MenuView;
            ImTKAssert.NotNull(attachedView, "Dynamic menu should be attached.");
            ImTKAssert.AreEqual("DynamicTools", attachedView.name, "Attached menu name mismatch.");
            ImTKAssert.AreEqual(50, attachedView.priority, "Priority should be applied.");
        }
    }
}
