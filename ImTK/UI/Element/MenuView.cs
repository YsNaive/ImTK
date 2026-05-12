using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using ImTK.Log;

namespace ImTK.UI
{
    public class MenuView : VisualElement, IMenuElement
    {
        public string name { get; set; }
        public int priority { get; set; }
        public bool isMenuBar { get; set; } = false;

        private List<IMenuElement> m_sortedMenuElements = new List<IMenuElement>();
        private static readonly LogContext s_log = new LogContext("MenuView");
        private const int SEPARATOR_THRESHOLD = 50;

        // 靜態快取 Render 方法以避免每幀的反射負擔
        private static readonly System.Reflection.MethodInfo s_renderMethod = typeof(VisualElement).GetMethod("Render", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public MenuView(string name, int priority = 0)
        {
            this.name = name;
            this.priority = priority;
            RegisterCallback<HierarchyChangedEvent>(OnHierarchyChanged);
        }

        private void OnHierarchyChanged(HierarchyChangedEvent evt)
        {
            // 當子節點變動時，收集所有的 IMenuElement 並根據 priority 排序
            m_sortedMenuElements.Clear();
            foreach (var child in hierarchy.Children())
            {
                if (child is IMenuElement menuElement)
                {
                    m_sortedMenuElements.Add(menuElement);
                }
            }
            m_sortedMenuElements.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        // 為了確保只有 IMenuElement 能夠被加入，我們覆寫/隱藏 Add 與 AddRange
        public new void Add(VisualElement child)
        {
            if (!(child is IMenuElement))
            {
                s_log.Error($"Cannot add '{child.GetType().Name}' to MenuView '{name}'. Only objects implementing IMenuElement can be added.");
                return;
            }
            base.Add(child);
        }

        public new void AddRange(IEnumerable<VisualElement> children)
        {
            foreach (var child in children)
            {
                Add(child);
            }
        }

        protected override void OnRenderLayout()
        {
            bool menuOpened = false;

            if (isMenuBar)
            {
                menuOpened = ImGui.BeginMenuBar();
            }
            else
            {
                menuOpened = ImGui.BeginMenu(name);
            }

            if (menuOpened)
            {
                int previousPriority = int.MinValue;
                bool isFirst = true;

                foreach (var menuElement in m_sortedMenuElements)
                {
                    if (!isFirst && (menuElement.priority - previousPriority) >= SEPARATOR_THRESHOLD)
                    {
                        ImGui.Separator();
                    }

                    if (menuElement is VisualElement visualChild)
                    {
                        // 呼叫內部防護層 Render，包含 PushID, PopID
                        if (s_renderMethod != null)
                        {
                            s_renderMethod.Invoke(visualChild, null);
                        }
                    }

                    previousPriority = menuElement.priority;
                    isFirst = false;
                }

                if (isMenuBar)
                {
                    ImGui.EndMenuBar();
                }
                else
                {
                    ImGui.EndMenu();
                }
            }
        }

        /// <summary>
        /// 提供透過字串路徑快速建立/尋找節點的語法糖。
        /// 路徑如 "File/Recent/Project A"。
        /// </summary>
        public MenuItem AddItem(string path, Action<ClickEvent> onClicked, int priority = 0)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            MenuView currentView = this;

            // 遍歷除了最後一個以外的所有節點，這些應該要是 MenuView
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string partName = parts[i];
                var existingNode = currentView.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == partName);

                if (existingNode == null)
                {
                    // 建立新的 MenuView 作為中繼節點
                    var newView = new MenuView(partName);
                    currentView.Add(newView);
                    currentView = newView;
                }
                else if (existingNode is MenuView existingView)
                {
                    currentView = existingView;
                }
                else
                {
                    s_log.Error($"Path conflict at '{partName}': Expected a MenuView, but found {(existingNode is MenuItem ? "MenuItem" : "unknown type")}. Path: {path}");
                    return null;
                }
            }

            // 最後一個部分，應為 MenuItem
            string finalPartName = parts[parts.Length - 1];
            var existingFinalNode = currentView.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == finalPartName);

            if (existingFinalNode != null)
            {
                s_log.Warning($"Item '{finalPartName}' already exists at path '{path}'. It will not be replaced.");
                return existingFinalNode as MenuItem; // 假設它是 MenuItem
            }

            var newItem = new MenuItem(finalPartName, onClicked, priority);
            currentView.Add(newItem);
            return newItem;
        }

        /// <summary>
        /// 提供將動態 MenuView 實例掛載到指定路徑下的語法糖。
        /// 路徑為要掛載的父節點路徑 (例如 "Window/Layouts")。
        /// </summary>
        public void AddMenu(string parentPath, MenuView view, int priority = 0)
        {
            if (view == null) return;
            view.priority = priority;

            if (string.IsNullOrEmpty(parentPath))
            {
                // 若路徑為空，則直接加到自己底下
                var existingRootNode = this.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == view.name);
                if (existingRootNode != null)
                {
                    s_log.Error($"Cannot add menu '{view.name}' at root because an element with the same name already exists.");
                    return;
                }
                this.Add(view);
                return;
            }

            string[] parts = parentPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            MenuView currentView = this;

            // 確保 parentPath 上的所有節點都存在且為 MenuView
            foreach (var partName in parts)
            {
                var existingNode = currentView.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == partName);

                if (existingNode == null)
                {
                    var newView = new MenuView(partName);
                    currentView.Add(newView);
                    currentView = newView;
                }
                else if (existingNode is MenuView existingView)
                {
                    currentView = existingView;
                }
                else
                {
                    s_log.Error($"Path conflict at '{partName}': Expected a MenuView, but found {(existingNode is MenuItem ? "MenuItem" : "unknown type")}. Cannot attach menu '{view.name}'.");
                    return;
                }
            }

            // 檢查最終父節點下是否已有同名節點
            var existingFinalNode = currentView.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == view.name);
            if (existingFinalNode != null)
            {
                s_log.Error($"Cannot add menu '{view.name}' at path '{parentPath}' because an element with the same name already exists.");
                return;
            }

            currentView.Add(view);
        }
    }
}
