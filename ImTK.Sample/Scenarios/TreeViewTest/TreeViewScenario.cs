using ImTK.Core;
using ImTK.UI;
using ImTK.Sample.Framework;

namespace ImTK.Sample.Scenarios.TreeViewTest
{
    public class TreeViewScenario : SampleScenarioBase
    {
        public override string Category => "UI Components";
        public override string Description => "Demonstrates the custom TreeView and TreeNode components.";

        public override Window Open()
        {
            return Window.Open<TreeViewWindow>();
        }
    }

    public class TreeViewWindow : Window
    {
        public TreeViewWindow() : base("TreeView Test")
        {
            var split = new SplitView();
            split.style.flexGrow = 1;

            var treeContainer = new VisualElement();
            treeContainer.style.flexGrow = 1;
            treeContainer.style.padding = new Thickness(10);

            var treeView = new TreeView<TreeNode>();
            treeView.allowMultiSelect = false;

            // Root Node 1
            var root1 = new TreeNode("Root Node 1");

            var child1_1 = new TreeNode("Child 1.1");
            var child1_2 = new TreeNode("Child 1.2");
            var child1_2_1 = new TreeNode("Child 1.2.1");
            
            child1_2.Add(child1_2_1);
            root1.Add(child1_1);
            root1.Add(child1_2);

            // Root Node 2
            var root2 = new TreeNode("Root Node 2");
            root2.Add(new TreeNode("Child 2.1"));
            root2.Add(new TreeNode("Child 2.2"));

            treeView.Add(root1);
            treeView.Add(root2);

            var infoText = new TextElement("Selected: None");
            infoText.style.margin = new Thickness(0, 0, 0, 10);
            treeContainer.Add(infoText);
            treeContainer.Add(treeView);

            treeView.onSelectionChanged += (node) => 
            {
                infoText.text = $"Selected: {(node != null ? node.text : "None")}";
            };

            var rightPanel = new VisualElement();
            rightPanel.style.flexGrow = 1;
            rightPanel.style.alignItems = AlignItems.Center;
            rightPanel.style.justifyContent = JustifyContent.Center;
            rightPanel.Add(new TextElement("Right Panel Content"));

            split.Add(treeContainer);
            split.Add(rightPanel);

            this.hierarchy.Add(split);
        }
    }
}
