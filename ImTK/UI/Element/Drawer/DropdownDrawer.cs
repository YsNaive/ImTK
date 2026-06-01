using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;

namespace ImTK.UI
{
    public abstract class DropdownDrawer<TValue> : FieldDrawer<TValue>
    {
        private IEnumerable<TValue> m_options;

        public IEnumerable<TValue> options
        {
            get => m_options;
            set
            {
                m_options = value;
                RebuildOptions();
            }
        }

        /// <summary>
        /// 提供選項顯示字串的轉換函式。用於 ComboContainer.GetPreviewText()
        /// 以及 RebuildOptions 中每個 ItemElement 的 displayString 格式化。
        /// </summary>
        public Func<TValue, string> formatOption { get; set; }

        /// <summary>
        /// 是否開啟搜尋功能
        /// </summary>
        public bool searchable { get; set; } = false;

        private ComboContainer m_comboContainer;
        protected string m_cachedPreviewText = "-";
        protected bool m_previewDirty = true;

        protected DropdownDrawer()
        {
            m_comboContainer = CreateComboContainer();
            m_comboContainer.style.flexGrow = 1;
            m_contentContainer.Add(m_comboContainer);
        }

        /// <summary>
        /// 建立外層下拉選單容器。子類別可覆寫以替換整個 Combo 外框行為
        /// （例如：SearchableDropdownDrawer 可回傳帶有搜尋欄的 SearchableComboContainer）。
        /// </summary>
        protected virtual ComboContainer CreateComboContainer() => new ComboContainer(this);

        /// <summary>
        /// 為指定的值建立對應的選項元件。子類別可覆寫以提供自訂選項外觀或互動邏輯
        /// （例如：FlagsEnumDropdownDrawer 可回傳帶有 Checkbox 的選項）。
        /// </summary>
        protected virtual ItemElement CreateItemElement(TValue value) => new ItemElement(this, value);

        /// <summary>
        /// 保留 API：建立 Combo 按鈕區的預覽元件，供未來擴充複雜預覽使用。
        /// 當前 ComboContainer 以 GetPreviewText() 搭配 ImDrawList 直接渲染，此方法不被自動呼叫。
        /// 子類別覆寫 CreateComboContainer() 後可在自訂容器內使用。
        /// </summary>
        protected virtual VisualElement CreatePreviewElement() => null;

        public override TValue value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_previewDirty = true;
            }
        }

        public override void SetValueWithoutNotify(TValue newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_previewDirty = true;
        }

        public override void SetValueWithChanged(TValue newValue)
        {
            base.SetValueWithChanged(newValue);
            m_previewDirty = true;
        }

        private void RebuildOptions()
        {
            m_comboContainer.Clear();
            if (m_options == null) return;

            foreach (var opt in m_options)
            {
                var item = CreateItemElement(opt);
                item.displayString = formatOption != null
                    ? formatOption(opt)
                    : (opt != null ? opt.ToString() : "null");
                m_comboContainer.Add(item);
            }
        }

        // ============================================================
        // Nested Types
        // ============================================================

        /// <summary>
        /// 下拉選單的外框容器。
        /// 使用 ImGui.InvisibleButton 精確佔用 layoutRect 配置的空間，
        /// 搭配 ImDrawList 渲染外框與 Preview 文字，並以 ImGui Popup 管理選單。
        /// </summary>
        protected class ComboContainer : VisualElement
        {
            protected readonly DropdownDrawer<TValue> m_drawer;
            private bool m_popupOpen;
            
            // 搜尋與排序相關狀態
            private string m_searchText = string.Empty;
            private List<VisualElement> m_originalOrder = new List<VisualElement>();

            public ComboContainer(DropdownDrawer<TValue> drawer)
            {
                m_drawer = drawer;
                this.style.flexGrow = 1;
            }

            /// <summary>
            /// 渲染 Preview 按鈕並管理 Popup 生命週期。
            /// InvisibleButton 解決兩個核心問題：
            /// (1) 大小精確對應 layoutRect，避免 BeginGroup/EndGroup 造成的高度衝突。
            /// (2) 透過 ImGui 輸入系統處理點擊，Popup 開啟時父視窗被遮蔽，
            ///     點擊 Popup 內的選項不會觸發此按鈕（解決點擊穿透問題）。
            /// OpenPopup 與 BeginPopup 使用相同字串 ID 且在同一 PushID 作用域下，
            /// 確保 ImGui 雜湊結果一致。
            /// </summary>
            public override bool OnBeginRender()
            {
                var drawList  = ImGui.GetWindowDrawList();
                var padding   = ImGui.GetStyle().FramePadding;
                float rounding = ImGui.GetStyle().FrameRounding;

                float width  = this.layoutRect.width;
                float height = this.layoutRect.height > 0f
                    ? this.layoutRect.height
                    : ImGui.GetFrameHeight();

                if (width <= 0f || height <= 0f) return false;

                // --- 1. InvisibleButton：精確佔用空間，透過 ImGui 輸入系統偵測點擊 ---
                // clicked 由 ImGui 正規 item 邏輯決定，Popup 開啟時父視窗輸入被遮蔽，
                // 不會因 Popup 內的選項點擊而誤觸。
                bool clicked = ImGui.InvisibleButton("##combo_btn", new Vector2(width, height));
                bool hovered = ImGui.IsItemHovered();

                var min = ImGui.GetItemRectMin();
                var max = ImGui.GetItemRectMax();

                // --- 2. 繪製背景外框 ---
                uint bgColor = hovered
                    ? ImGui.GetColorU32(ImGuiCol.FrameBgHovered)
                    : ImGui.GetColorU32(ImGuiCol.FrameBg);
                drawList.AddRectFilled(min, max, bgColor, rounding);
                drawList.AddRect(min, max, ImGui.GetColorU32(ImGuiCol.Border), rounding);

                // --- 3. 繪製 Preview 文字（限制在箭頭左側的內容區）---
                float arrowWidth = height;
                string previewText = GetPreviewText();
                var contentMax = new Vector2(max.X - arrowWidth, max.Y);
                var textPos = new Vector2(
                    min.X + padding.X,
                    min.Y + (height - ImGui.GetFontSize()) * 0.5f
                );
                drawList.PushClipRect(min, contentMax, true);
                drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), previewText);
                drawList.PopClipRect();

                // --- 4. 繪製右側箭頭區塊 ---
                var arrowMin = new Vector2(max.X - arrowWidth, min.Y);
                drawList.AddRectFilled(arrowMin, max, ImGui.GetColorU32(ImGuiCol.Button), rounding);
                var ac = new Vector2(arrowMin.X + arrowWidth * 0.5f, min.Y + height * 0.5f);
                drawList.AddTriangleFilled(
                    new Vector2(ac.X - 4f, ac.Y - 2f),
                    new Vector2(ac.X + 4f, ac.Y - 2f),
                    new Vector2(ac.X,      ac.Y + 3f),
                    ImGui.GetColorU32(ImGuiCol.Text)
                );

                // --- 5. 點擊時開啟 Popup ---
                if (clicked)
                    ImGui.OpenPopup("##popup");

                // --- 6. 定位並嘗試開啟 Popup 視窗 ---
                ImGui.SetNextWindowPos(new Vector2(min.X, max.Y));
                ImGui.SetNextWindowSizeConstraints(
                    new Vector2(width, 0f),
                    new Vector2(float.MaxValue, 300f)
                );
                m_popupOpen = ImGui.BeginPopup("##popup");

                if (m_popupOpen && m_drawer.searchable)
                {
                    RenderSearchBar();
                }

                // Popup 開啟時回傳 true，RenderEngine 在 Popup 視窗內走訪子節點（ItemElement）
                return m_popupOpen;
            }

            private void RenderSearchBar()
            {
                bool needsRebuildOrder = m_originalOrder.Count != this.hierarchy.childCount;
                if (!needsRebuildOrder && this.hierarchy.childCount > 0)
                {
                    if (!m_originalOrder.Contains(this.hierarchy.ChildAt(0)))
                    {
                        needsRebuildOrder = true;
                    }
                }

                if (needsRebuildOrder)
                {
                    m_originalOrder.Clear();
                    for (int i = 0; i < this.hierarchy.childCount; i++)
                        m_originalOrder.Add(this.hierarchy.ChildAt(i));
                }

                bool changed = false;
                if (ImGui.IsWindowAppearing())
                {
                    ImGui.SetKeyboardFocusHere();
                    m_searchText = string.Empty;
                    changed = true; // 強制在開啟時套用過濾
                }

                ImGui.SetNextItemWidth(-float.Epsilon);
                
                string currentSearch = m_searchText;
                
                ImGui.PushID("search_bar");
                if (ImGui.InputTextWithHint("##search", "Search...", ref currentSearch, 256))
                {
                    changed = true;
                    m_searchText = currentSearch;
                }
                ImGui.PopID();

                // 若文字變更，或選項陣列被重建，必須重新套用過濾與排序
                if (changed || needsRebuildOrder)
                {
                    ApplyFilter();
                }

                ImGui.Separator();
            }

            private void ApplyFilter()
            {
                string query = m_searchText ?? string.Empty;
                bool isEmpty = string.IsNullOrEmpty(query);
                
                var scores = new Dictionary<VisualElement, int>();
                int childCount = this.hierarchy.childCount;
                
                for (int i = 0; i < childCount; i++)
                {
                    var child = this.hierarchy.ChildAt(i) as ItemElement;
                    if (child == null) continue;
                    
                    if (isEmpty)
                    {
                        scores[child] = 0;
                        child.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        int dist = ComputeMatchDistance(query, child.displayString ?? string.Empty);
                        scores[child] = dist;
                        
                        // 距離過大時隱藏。容許的修改次數為長度的一半。
                        int threshold = query.Length / 2;
                        if (dist <= threshold || dist == 0)
                        {
                            child.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            child.style.display = DisplayStyle.None;
                        }
                    }
                }
                
                // 根據距離（修改次數）進行排序，距離越小越前面。若距離相同則保留原順序
                this.hierarchy.SortChildren((a, b) =>
                {
                    if (isEmpty)
                    {
                        return m_originalOrder.IndexOf(a).CompareTo(m_originalOrder.IndexOf(b));
                    }
                    
                    int scoreA = scores.TryGetValue(a, out int sa) ? sa : int.MaxValue;
                    int scoreB = scores.TryGetValue(b, out int sb) ? sb : int.MaxValue;
                    
                    int cmp = scoreA.CompareTo(scoreB);
                    if (cmp == 0)
                    {
                        // 分數相同時，依照原始順序排列
                        return m_originalOrder.IndexOf(a).CompareTo(m_originalOrder.IndexOf(b));
                    }
                    return cmp;
                }, notify: false);
                
                RenderEngine.MarkRenderDirty(this);
            }

            /// <summary>
            /// 計算子字串模糊匹配的 Levenshtein 距離
            /// </summary>
            private static int ComputeMatchDistance(string query, string text)
            {
                query = query.ToLowerInvariant();
                text = text.ToLowerInvariant();
                
                if (text.Contains(query)) return 0;
                
                int n = query.Length;
                int m = text.Length;
                if (n == 0) return 0;
                if (m == 0) return n;
                
                int[,] d = new int[n + 1, m + 1];
                
                for (int i = 0; i <= n; i++) d[i, 0] = i;
                for (int j = 0; j <= m; j++) d[0, j] = 0; 
                
                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (query[i - 1] == text[j - 1]) ? 0 : 1;
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1,    // Deletion from query
                                     d[i, j - 1] + 1),   // Insertion into query (skipping char in text)
                            d[i - 1, j - 1] + cost);     // Substitution
                    }
                }
                
                int minDistance = n;
                for (int j = 1; j <= m; j++)
                {
                    if (d[n, j] < minDistance)
                        minDistance = d[n, j];
                }
                
                return minDistance;
            }

            public override void OnEndRender()
            {
                if (m_popupOpen)
                    ImGui.EndPopup();
            }

            /// <summary>
            /// 取得 Preview 按鈕應顯示的文字。子類別可覆寫以提供自訂的預覽字串。
            /// </summary>
            protected virtual string GetPreviewText()
            {
                if (m_drawer.m_previewDirty)
                {
                    m_drawer.m_cachedPreviewText = m_drawer.formatOption != null
                        ? m_drawer.formatOption(m_drawer.value)
                        : (m_drawer.value != null ? m_drawer.value.ToString() : "-");
                    m_drawer.m_previewDirty = false;
                }
                return m_drawer.m_cachedPreviewText;
            }
        }

        /// <summary>
        /// 下拉選單中的單一選項元件。
        /// 子類別可覆寫 ComputeIsSelected 與 OnClicked 以自訂選擇邏輯，
        /// 例如：FlagsItemElement 可實作位元遮罩切換。
        /// </summary>
        protected class ItemElement : VisualElement
        {
            protected readonly DropdownDrawer<TValue> m_drawer;

            public TValue value { get; internal set; }
            public string displayString { get; internal set; }

            public ItemElement(DropdownDrawer<TValue> drawer, TValue value)
            {
                m_drawer = drawer;
                this.value = value;
                this.useNativeLayout = true;
                this.style.positionType = PositionType.Absolute;
            }

            /// <summary>
            /// 決定此選項是否為當前選中狀態。
            /// 預設以 EqualityComparer 進行等值比較（單選）。
            /// 子類別可覆寫以實作 [Flags] 位元包含判斷等多選邏輯。
            /// </summary>
            protected virtual bool ComputeIsSelected()
                => EqualityComparer<TValue>.Default.Equals(value, m_drawer.value);

            /// <summary>
            /// 選項被點擊時的處理邏輯。
            /// 預設呼叫 SetValueWithChanged（觸發 ValueChangedEvent&lt;T&gt;）。
            /// 子類別可覆寫以實作位元切換（[Flags]）或多選行為。
            /// </summary>
            protected virtual void OnClicked()
                => m_drawer.SetValueWithChanged(value);

            public override void OnRender()
            {
                bool selected = ComputeIsSelected();
                if (ImGui.Selectable(displayString ?? string.Empty, selected))
                    OnClicked();
                if (selected)
                    ImGui.SetItemDefaultFocus();
            }
        }
    }
}
