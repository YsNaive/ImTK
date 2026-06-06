using ImTK.Log;
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

        protected override IEnumerable<VisualElement> GetItemChildren(VisualElement item)
        {
            return item.hierarchy.Children();
        }

        protected override VisualElement GetItemParent(VisualElement item)
        {
            return item.hierarchy.parent;
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
        private VisualElementGizmoContext m_pickingGizmoContext;
        private VisualElementGizmoContext m_windowBlockerGizmoContext;
        
        private Button m_pickingModeBtn;
        private bool m_isPickingMode = false;
        private VisualElement m_currentHoveredPick = null;
        private VisualElement m_lastFrameHoveredPick = null;
        private int m_lastFrameCount = -1;

        public VisualElementInspectorWindow() : base("UI Inspector", WindowId)
        {
            this.style.width = 600;
            this.style.height = 400;

            var splitView = new SplitView();
            splitView.persistenceKey = "InspectorSplitView";
            splitView.style.flexGrow = 1f;

            var leftPanel = new VisualElement();
            leftPanel.style.flexGrow = 1f;
            leftPanel.style.flexDirection = ImTK.UI.FlexDirection.Column;

            m_pickingModeBtn = new Button("Picking Mode");
            m_pickingModeBtn.style.margin = new ImTK.UI.Thickness(5f);
            m_pickingModeBtn.onClicked += (evt) => {
                m_isPickingMode = !m_isPickingMode;
                m_pickingModeBtn.style.colorFamily = m_isPickingMode ? ImTK.UI.ThemeColorFamily.Warning : ImTK.UI.ThemeColorFamily.Normal;
                ImTK.Log.ImTKLog.Trace($"Picking Mode toggled: {m_isPickingMode}");
            };

            m_treeView = new InspectorTreeView();
            m_treeView.style.flexGrow = 1f;
            m_treeView.onSelectionChanged += OnSelectionChanged;

            leftPanel.Add(m_pickingModeBtn);
            leftPanel.Add(m_treeView);

            m_propertiesPanel = new InspectorPropertiesPanel();

            splitView.Add(leftPanel);
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
                    if (m_isPickingMode) return node == m_lastFrameHoveredPick;
                    
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

            m_pickingGizmoContext = new VisualElementGizmoContext
            {
                filter = (node) => m_isPickingMode,
                action = (node) => 
                {
                    int currentFrame = Hexa.NET.ImGui.ImGui.GetFrameCount();
                    if (currentFrame != m_lastFrameCount)
                    {
                        m_lastFrameHoveredPick = m_currentHoveredPick;
                        m_currentHoveredPick = null;
                        m_lastFrameCount = currentFrame;
                    }

                    var rect = node.layoutRect;
                    var offset = ImTK.UI.RenderEngine.Context.CurrentRenderOffset;
                    System.Numerics.Vector2 min = rect.position - offset;
                    System.Numerics.Vector2 size = new System.Numerics.Vector2(rect.width, rect.height);
                    
                    if (size.X <= 0 || size.Y <= 0) return;

                    if (Hexa.NET.ImGui.ImGui.IsMouseHoveringRect(min, min + size, true))
                    {
                        m_currentHoveredPick = node;
                    }
                }
            };

            m_windowBlockerGizmoContext = new VisualElementGizmoContext
            {
                filter = (node) => m_isPickingMode && node is ImTK.UI.Window,
                action = (node) =>
                {
                    Hexa.NET.ImGui.ImGui.SetCursorPos(new System.Numerics.Vector2(0, 0));
                    
                    var size = Hexa.NET.ImGui.ImGui.GetContentRegionAvail();
                    Hexa.NET.ImGui.ImGui.PushID(node.GetHashCode());
                    Hexa.NET.ImGui.ImGui.InvisibleButton("##blocker", size);
                    Hexa.NET.ImGui.ImGui.PopID();
                }
            };

            ImTK.UI.RenderEngine.RegisterGizmoContext(m_pickingGizmoContext);
            ImTK.UI.RenderEngine.RegisterGizmoContext(m_windowBlockerGizmoContext);
            
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
            if (m_pickingGizmoContext != null)
            {
                ImTK.UI.RenderEngine.UnregisterGizmoContext(m_pickingGizmoContext);
                m_pickingGizmoContext = null;
            }
            if (m_windowBlockerGizmoContext != null)
            {
                ImTK.UI.RenderEngine.UnregisterGizmoContext(m_windowBlockerGizmoContext);
                m_windowBlockerGizmoContext = null;
            }
        }

        public override void OnRender()
        {
            base.OnRender();

            int currentFrame = Hexa.NET.ImGui.ImGui.GetFrameCount();
            if (currentFrame != m_lastFrameCount)
            {
                m_lastFrameHoveredPick = m_currentHoveredPick;
                m_currentHoveredPick = null;
                m_lastFrameCount = currentFrame;
            }

            if (m_isPickingMode)
            {
                Hexa.NET.ImGui.ImGui.SetMouseCursor(Hexa.NET.ImGui.ImGuiMouseCursor.Hand);
                m_treeView.externalHoveredItem = m_lastFrameHoveredPick;
                
                if (Hexa.NET.ImGui.ImGui.IsMouseClicked(Hexa.NET.ImGui.ImGuiMouseButton.Left))
                {
                    m_isPickingMode = false;
                    m_pickingModeBtn.style.colorFamily = ImTK.UI.ThemeColorFamily.Normal;
                    ImTK.Log.ImTKLog.Trace("Picking Mode disabled (element selected).");
                    
                    var pick = m_lastFrameHoveredPick;
                    m_currentHoveredPick = null;
                    m_lastFrameHoveredPick = null;
                    m_treeView.externalHoveredItem = null;

                    if (pick != null)
                    {
                        ImTK.Core.ImTKApplication.ScheduleDeferred(() => 
                        {
                            m_treeView.Reveal(pick);
                        });
                    }
                }
            }
            else
            {
                m_currentHoveredPick = null;
                m_lastFrameHoveredPick = null;
                m_treeView.externalHoveredItem = null;
            }

            m_scanTimer += (float)ImTK.Core.Time.DeltaTime;
            if (m_scanTimer > 1.0f)
            {
                m_scanTimer = 0f;
                ImTK.Core.ImTKApplication.ScheduleDeferred(() => 
                {
                    RefreshTree();
                });
            }
        }

        private bool IsInsideInspector(VisualElement element)
        {
            VisualElement current = element;
            while (current != null)
            {
                if (current == this) return true;
                current = current.hierarchy.parent;
            }
            return false;
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
            var offset = ImTK.UI.RenderEngine.Context.CurrentRenderOffset;
            System.Numerics.Vector2 min = rect.position - offset;
            
            // 使用元件本身的 contentSize 來包覆被裁切或超出範圍的內容 (包含 ScrollView 等滾動容器的完整大小)
            System.Numerics.Vector2 targetSize = node.contentSize;

            float drawWidth = Math.Max(rect.width, targetSize.X);
            float drawHeight = Math.Max(rect.height, targetSize.Y);
            
            System.Numerics.Vector2 max = min + new System.Numerics.Vector2(drawWidth, drawHeight);


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
            else if (node == m_treeView.hoveredItem || (m_isPickingMode && node == m_currentHoveredPick))
            {
                uint blueTranslucent = new ImTK.Color(0f, 0.5f, 1f, 0.1f).ToUInt32();
                drawList.AddRectFilled(min, max, blueTranslucent);
                
                uint blueSolid = new ImTK.Color(0f, 0.5f, 1f, 1f).ToUInt32();
                drawList.AddRect(min, max, blueSolid, 0f, Hexa.NET.ImGui.ImDrawFlags.None, 1f);
            }
        }
    }
}
