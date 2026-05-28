using System;
using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;
using ImTK.Log;

using ImTK.Core;

namespace ImTK.UI
{
    public class MenuView : VisualElement<MenuView.Style>, IMenuElement, IRenderRoot
    {
        public new class Style : VisualElement.Style
        {






            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken)
                {
                    if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.PopupBg;
                        output.Add(prop);
                        return;
                    }
                }
                base.ComputeHighlevelToken(prop, output);
            }
}

        public RenderListCache RenderCache { get; } = new RenderListCache();
        public string name { get; set; }
        public int priority { get; set; }
        public bool isMenuBar { get; set; } = false;

        private List<IMenuElement> m_sortedMenuElements = new List<IMenuElement>();

        private const int SEPARATOR_THRESHOLD = 50;

        public MenuView(string name, int priority = 0)
        {
            this.name = name;
            this.priority = priority;
            m_useNativeLayout = true;
            RegisterCallback<HierarchyChangedEvent>(OnHierarchyChanged);
        }

        private bool m_isRebuildingHierarchy = false;

        private void OnHierarchyChanged(HierarchyChangedEvent evt)
        {
            if (m_isRebuildingHierarchy) return;
            
            var validElements = new List<IMenuElement>();
            foreach (var child in hierarchy.Children())
            {
                if (child is IMenuElement me && !(child is MenuSeparatorElement))
                {
                    validElements.Add(me);
                }
            }
            validElements.Sort((a, b) => a.priority.CompareTo(b.priority));

            // 檢查是否需要重建
            bool needsRebuild = false;
            int expectedIndex = 0;
            int prevPriority = int.MinValue;
            bool first = true;

            foreach(var menuElement in validElements)
            {
                if (!first && (menuElement.priority - prevPriority) >= SEPARATOR_THRESHOLD)
                {
                    if (expectedIndex >= hierarchy.childCount || !(hierarchy.ChildAt(expectedIndex) is MenuSeparatorElement))
                    {
                        needsRebuild = true; break;
                    }
                    expectedIndex++;
                }

                if (expectedIndex >= hierarchy.childCount || hierarchy.ChildAt(expectedIndex) != menuElement)
                {
                    needsRebuild = true; break;
                }
                expectedIndex++;
                prevPriority = menuElement.priority;
                first = false;
            }

            if (expectedIndex != hierarchy.childCount) needsRebuild = true;

            if (!needsRebuild) return;

            m_sortedMenuElements.Clear();
            foreach(var v in validElements) m_sortedMenuElements.Add(v);

            // 如果已經不需要重建，且沒有漏掉 Separator (需要更精細檢查的話，目前簡化處理：如果數量與狀態一致則略過)
            // 這裡簡單防護：清空物理樹再重新 Insert，並靜默處理避免再次觸發 HierarchyChanged
            m_isRebuildingHierarchy = true;
            try
            {
                // 先安全移除所有舊有的子節點
                var oldChildren = new List<VisualElement>(hierarchy.Children());
                foreach(var child in oldChildren)
                {
                    hierarchy.Remove(child, notify: false);
                }

                int previousPriority = int.MinValue;
                bool isFirst = true;

                foreach (var menuElement in m_sortedMenuElements)
                {
                    if (!isFirst && (menuElement.priority - previousPriority) >= SEPARATOR_THRESHOLD)
                    {
                        hierarchy.Insert(hierarchy.childCount, new MenuSeparatorElement(), notify: false);
                    }

                    if (menuElement is VisualElement ve)
                    {
                        hierarchy.Insert(hierarchy.childCount, ve, notify: false);
                    }

                    previousPriority = menuElement.priority;
                    isFirst = false;
                }

                // 最後統一標記 dirty，這樣 RenderList 才會被重建
                EventDispatcher.MarkHierarchyDirty(this);
                this.GetRenderRoot()?.RenderCache.MarkDirty();
            }
            finally
            {
                m_isRebuildingHierarchy = false;
            }
        }

        // 為了確保只有 IMenuElement 能夠被加入，我們覆寫/隱藏 Add 與 AddRange
        public new void Add(VisualElement child)
        {
            if (!(child is IMenuElement))
            {
                ImTKLog.Error($"Cannot add '{child.GetType().Name}' to MenuView '{name}'. Only objects implementing IMenuElement can be added.");
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

        private bool m_menuOpenedCache;

        public override bool OnBeginRender()
        {
            m_menuOpenedCache = false;

            if (isMenuBar)
            {
                m_menuOpenedCache = ImGui.BeginMenuBar();
            }
            else
            {
                m_menuOpenedCache = ImGui.BeginMenu(name);
            }

            if (m_menuOpenedCache)
            {
                // We return true here to allow RenderEngine.RenderFlat to traverse and render the children naturally!
                return true;
            }

            return false;
        }

        public override void OnEndRender()
        {
            if (m_menuOpenedCache)
            {
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
                    ImTKLog.Error($"Path conflict at '{partName}': Expected a MenuView, but found {(existingNode is MenuItem ? "MenuItem" : "unknown type")}. Path: {path}");
                    return null;
                }
            }

            // 最後一個部分，應為 MenuItem
            string finalPartName = parts[parts.Length - 1];
            var existingFinalNode = currentView.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == finalPartName);

            if (existingFinalNode != null)
            {
                ImTKLog.Warning($"Item '{finalPartName}' already exists at path '{path}'. It will not be replaced.");
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
                    ImTKLog.Error($"Cannot add menu '{view.name}' at root because an element with the same name already exists.");
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
                    ImTKLog.Error($"Path conflict at '{partName}': Expected a MenuView, but found {(existingNode is MenuItem ? "MenuItem" : "unknown type")}. Cannot attach menu '{view.name}'.");
                    return;
                }
            }

            // 檢查最終父節點下是否已有同名節點
            var existingFinalNode = currentView.hierarchy.Children().FirstOrDefault(c => (c as IMenuElement)?.name == view.name);
            if (existingFinalNode != null)
            {
                ImTKLog.Error($"Cannot add menu '{view.name}' at path '{parentPath}' because an element with the same name already exists.");
                return;
            }

            currentView.Add(view);
        }
    }
}
