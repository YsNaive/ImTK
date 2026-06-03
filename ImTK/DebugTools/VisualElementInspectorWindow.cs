using ImTK.UI;
using System.Collections.Generic;

namespace ImTK.DebugTools
{
    public class InspectorTreeView : TreeView<VisualElement>
    {
        protected override VisualElement MakeItem()
        {
            var label = new Label();
            label.useNativeLayout = true;
            return label;
        }

        protected override void BindItem(VisualElement ui, VisualElement item)
        {
            var label = (Label)ui;
            label.text = $"{item.GetType().Name} [ID:{item.m_elementId}]" + 
                         (string.IsNullOrEmpty(item.persistenceKey) ? "" : $" ({item.persistenceKey})");
        }

        protected override IEnumerable<VisualElement> FetchChildren(VisualElement item)
        {
            return item.hierarchy.Children();
        }

        protected override bool HasChildren(VisualElement item)
        {
            return item.hierarchy.childCount > 0;
        }
    }

    public class VisualElementInspectorWindow : Window
    {
        public const string WindowId = "ImTK.VisualElementInspector";

        private InspectorTreeView m_treeView;
        private Label m_idLabel;

        public VisualElementInspectorWindow() : base("UI Inspector", WindowId)
        {
            this.style.width = 600;
            this.style.height = 400;

            var splitView = new SplitView();
            splitView.persistenceKey = "InspectorSplitView";
            splitView.style.flexGrow = 1f;

            m_treeView = new InspectorTreeView();
            m_treeView.style.flexGrow = 1f;
            m_treeView.onSelectionChanged += OnSelectionChanged;

            var rightPanel = new VisualElement();
            rightPanel.style.padding = new Thickness(10);

            m_idLabel = new Label("Selected ID: None");
            rightPanel.Add(m_idLabel);

            splitView.Add(m_treeView);
            splitView.Add(rightPanel);

            this.Add(splitView);
        }

        private float m_scanTimer = 0f;

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshTree();
        }

        public override void OnRender()
        {
            base.OnRender();
            m_scanTimer += (float)ImTK.Core.Time.DeltaTime;
            if (m_scanTimer > 1.0f)
            {
                m_scanTimer = 0f;
                // 每秒重新指派一次 itemsSource，底層的 RebuildFlattenedList 會在保留狀態的前提下，
                // 高效地同步所有展開節點的最新子節點狀態 (無需重建物件池)。
                ImTK.Core.ImTKApplication.ScheduleDeferred(() => 
                {
                    RefreshTree();
                });
            }
        }

        private System.Collections.Generic.List<VisualElement> GetActiveRoots()
        {
            var activeRoots = new System.Collections.Generic.List<VisualElement>();
            foreach (var window in Window.activeWindows)
            {
                activeRoots.Add(window);
            }
            return activeRoots;
        }

        public void RefreshTree()
        {
            m_treeView.itemsSource = GetActiveRoots();
        }

        private void OnSelectionChanged(VisualElement element)
        {
            if (element != null)
            {
                m_idLabel.text = $"Selected ID: {element.m_elementId}";
            }
            else
            {
                m_idLabel.text = "Selected ID: None";
            }
        }
    }
}
