using ImTK.Core;
using ImTK.UI;
using ImTK.Sample.Framework;
using System.Collections.Generic;

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

    public class StringNodeData
    {
        public string Text;
        public List<StringNodeData> Children = new List<StringNodeData>();
    }

    public class StringTreeView : TreeView<StringNodeData>
    {
        protected override VisualElement MakeItem()
        {
            var label = new Label();
            label.useNativeLayout = true;
            return label;
        }

        protected override void BindItem(VisualElement ui, StringNodeData item)
        {
            var label = (Label)ui;
            label.text = item.Text;
        }

        protected override IEnumerable<StringNodeData> FetchChildren(StringNodeData item)
        {
            return item.Children;
        }

        protected override bool HasChildren(StringNodeData item)
        {
            return item.Children != null && item.Children.Count > 0;
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

            var treeView = new StringTreeView();
            treeView.allowMultiSelect = false;
            treeView.style.flexGrow = 1f;

            // Root Node 1
            var root1 = new StringNodeData { Text = "Root Node 1" };

            var child1_1 = new StringNodeData { Text = "Child 1.1" };
            var child1_2 = new StringNodeData { Text = "Child 1.2" };
            var child1_2_1 = new StringNodeData { Text = "Child 1.2.1" };
            
            child1_2.Children.Add(child1_2_1);
            root1.Children.Add(child1_1);
            root1.Children.Add(child1_2);

            // Root Node 2
            var root2 = new StringNodeData { Text = "Root Node 2" };
            root2.Children.Add(new StringNodeData { Text = "Child 2.1" });
            root2.Children.Add(new StringNodeData { Text = "Child 2.2" });

            treeView.itemsSource = new List<StringNodeData> { root1, root2 };

            var infoText = new Label("Selected: None");
            infoText.style.margin = new Thickness(0, 0, 0, 10);
            treeContainer.Add(infoText);
            treeContainer.Add(treeView);

            treeView.onSelectionChanged += (node) => 
            {
                infoText.text = $"Selected: {(node != null ? node.Text : "None")}";
            };

            var rightPanel = new VisualElement();
            rightPanel.style.flexGrow = 1;
            rightPanel.style.alignItems = AlignItems.Center;
            rightPanel.style.justifyContent = JustifyContent.Center;
            rightPanel.Add(new Label("Right Panel Content"));

            split.Add(treeContainer);
            split.Add(rightPanel);

            this.hierarchy.Add(split);
        }
    }
}
