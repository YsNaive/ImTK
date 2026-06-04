using ImTK.UI;
using System.Collections.Generic;

namespace ImTK.DebugTools
{
    public class InspectorTreeView : TreeView<VisualElement>
    {
        protected override VisualElement MakeItem()
        {
            var container = new VisualElement();
            container.useNativeLayout = true;
            
            // Temporary hack: Since TreeNode wraps us in an ImGui.BeginGroup and calls SameLine,
            // we should just use TextElement for the placeholder and text, and we will format it as "[Icon] TypeName" 
            // instead of a complex nested container which would break TreeNode's native layout expectations.
            var label = new Label();
            label.useNativeLayout = true;
            return label;
        }

        protected override void BindItem(VisualElement ui, VisualElement item)
        {
            var label = (Label)ui;
            label.text = $"[ ] {item.GetType().Name}"; // Placeholder icon space
            
            if (item.resolvedLayoutState.display == ImTK.UI.DisplayStyle.None)
            {
                label.style.textColor = new ImTK.Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                label.style.textColor = ImTK.UI.StyleKeyword.Null;
            }
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
        private InspectorPropertiesPanel m_propertiesPanel;
        private VisualElement m_selectedElement;
        private VisualElementGizmoContext m_gizmoContext;

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

            m_propertiesPanel = new InspectorPropertiesPanel();

            splitView.Add(m_treeView);
            splitView.Add(m_propertiesPanel);

            this.Add(splitView);
        }

        private float m_scanTimer = 0f;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_gizmoContext = new VisualElementGizmoContext
            {
                filter = (node) => 
                {
                    if (node == m_treeView.hoveredItem) return true;
                    var current = m_selectedElement;
                    while (current != null)
                    {
                        if (current == node) return true;
                        current = current.hierarchy.parent;
                    }
                    return false;
                },
                action = DrawBoxModel
            };
            ImTK.UI.RenderEngine.RegisterGizmoContext(m_gizmoContext);
            
            RefreshTree();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_gizmoContext != null)
            {
                ImTK.UI.RenderEngine.UnregisterGizmoContext(m_gizmoContext);
                m_gizmoContext = null;
            }
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
            m_selectedElement = element;
            m_propertiesPanel.SetTarget(m_selectedElement);
        }

        private int GetElementDepth(VisualElement element)
        {
            int depth = 0;
            var current = element;
            while (current.hierarchy.parent != null)
            {
                depth++;
                current = current.hierarchy.parent;
            }
            return depth;
        }

        private void DrawBoxModel(VisualElement node)
        {
            var viewport = Hexa.NET.ImGui.ImGui.GetWindowViewport();
            var drawList = Hexa.NET.ImGui.ImGui.GetForegroundDrawList(viewport);
            
            var rect = node.layoutRect;
            System.Numerics.Vector2 min = new System.Numerics.Vector2(rect.x, rect.y);
            System.Numerics.Vector2 max = new System.Numerics.Vector2(rect.x + rect.width, rect.y + rect.height);

            bool isSelectedOrParent = false;
            var current = m_selectedElement;
            while (current != null)
            {
                if (current == node)
                {
                    isSelectedOrParent = true;
                    break;
                }
                current = current.hierarchy.parent;
            }

            if (isSelectedOrParent)
            {
                int depth = GetElementDepth(node);
                float hue = ((depth * 15f) % 360f) / 360f;
                float r = 0, g = 0, b = 0;
                Hexa.NET.ImGui.ImGui.ColorConvertHSVtoRGB(hue, 1f, 1f, ref r, ref g, ref b);
                
                uint color = new ImTK.Color(r, g, b, 1f).ToUInt32();
                drawList.AddRect(min, max, color, 0f, Hexa.NET.ImGui.ImDrawFlags.None, 1f);
            }
            else if (node == m_treeView.hoveredItem)
            {
                uint blueTranslucent = new ImTK.Color(0f, 0.5f, 1f, 0.1f).ToUInt32();
                drawList.AddRectFilled(min, max, blueTranslucent);
                
                uint blueSolid = new ImTK.Color(0f, 0.5f, 1f, 1f).ToUInt32();
                drawList.AddRect(min, max, blueSolid, 0f, Hexa.NET.ImGui.ImDrawFlags.None, 1f);
            }
        }
    }
}
