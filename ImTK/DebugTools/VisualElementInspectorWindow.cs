using ImTK.UI;
using System.Collections.Generic;

namespace ImTK.DebugTools
{
    public class VisualElementInspectorWindow : Window
    {
        public const string WindowId = "ImTK.VisualElementInspector";

        private TreeView<VisualElementTreeNode> m_treeView;
        private Label m_idLabel;

        public VisualElementInspectorWindow() : base("UI Inspector", WindowId)
        {
            this.style.width = 600;
            this.style.height = 400;

            var splitView = new SplitView();
            splitView.persistenceKey = "InspectorSplitView";
            splitView.style.flexGrow = 1f;

            m_treeView = new TreeView<VisualElementTreeNode>();
            m_treeView.onSelectionChanged += OnSelectionChanged;

            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1f;
            scrollView.Add(m_treeView);

            var rightPanel = new VisualElement();
            rightPanel.style.padding = new Thickness(10);

            m_idLabel = new Label("Selected ID: None");
            rightPanel.Add(m_idLabel);

            splitView.Add(scrollView);
            splitView.Add(rightPanel);

            this.Add(splitView);
        }

        private HashSet<VisualElement> m_knownRoots = new HashSet<VisualElement>();
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
                CheckRoots();
            }
        }

        private void CheckRoots()
        {
            var currentRoots = GetActiveRoots();
            bool changed = false;
            int count = 0;

            foreach (var root in currentRoots)
            {
                count++;
                if (!m_knownRoots.Contains(root))
                {
                    changed = true;
                    break;
                }
            }

            if (!changed && count != m_knownRoots.Count)
            {
                changed = true;
            }

            if (changed)
            {
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
                if (window is VisualElementInspectorWindow)
                    continue;
                    
                activeRoots.Add(window);
            }
            return activeRoots;
        }

        public void RefreshTree()
        {
            m_treeView.Clear();
            m_knownRoots.Clear();
            var roots = GetActiveRoots();
            foreach (var root in roots)
            {
                m_knownRoots.Add(root);
                m_treeView.Add(new VisualElementTreeNode(root));
            }
        }

        private void OnSelectionChanged(VisualElementTreeNode node)
        {
            if (node != null && node.targetElement != null)
            {
                m_idLabel.text = $"Selected ID: {node.targetElement.m_elementId}";
            }
            else
            {
                m_idLabel.text = "Selected ID: None";
            }
        }
    }
}
