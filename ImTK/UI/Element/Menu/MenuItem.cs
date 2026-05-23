using System;
using ImGuiNET;
using ImTK.Log;

using ImTK.Core;

namespace ImTK.UI
{
    /// <summary>
    /// 代表選單中的末端可點擊項目。
    /// 不能包含子節點。
    /// </summary>
    public class MenuItem : VisualElement<MenuItem.Style>, IMenuElement
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString HoverColor = new HashedString("HoverColor");
            public static readonly HashedString ActiveColor = new HashedString("ActiveColor");
        }

        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            public StyleValue<Color>? hoverColor
            {
                get => GetOverrideColor(StyleKey.HoverColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.HoverColor, value.Value);
                    else Clear(StyleKey.HoverColor);
                }
            }

            public StyleValue<Color>? activeColor
            {
                get => GetOverrideColor(StyleKey.ActiveColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.ActiveColor, value.Value);
                    else Clear(StyleKey.ActiveColor);
                }
            }

            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);

                m_pushedColors = 0;

                // MenuItem Background maps to Header
                Color? bgColor = resolvedStyle.GetColor(VisualElement.StyleKey.BackgroundColor);
                if (bgColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, bgColor.Value.u32);
                    m_pushedColors++;
                }

                Color? hoverColor = resolvedStyle.GetColor(StyleKey.HoverColor);
                if (hoverColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, hoverColor.Value.u32);
                    m_pushedColors++;
                }

                Color? activeColor = resolvedStyle.GetColor(StyleKey.ActiveColor);
                if (activeColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.HeaderActive, activeColor.Value.u32);
                    m_pushedColors++;
                }
            }

            public override void PopFromImGui()
            {
                if (m_pushedColors > 0)
                {
                    ImGui.PopStyleColor(m_pushedColors);
                    m_pushedColors = 0;
                }
                base.PopFromImGui();
            }
        }

        public string name { get; set; }
        public int priority { get; set; }
        public bool isChecked { get; set; }

        public event Action<ClickEvent> onClicked
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public MenuItem(string name, Action<ClickEvent> onClicked = null, int priority = 0)
        {
            this.name = name;
            this.priority = priority;
            if (onClicked != null)
            {
                this.onClicked += onClicked;
            }
        }

        public override void OnRender()
        {
            // 根據 ImGui.MenuItem 的設計，若不傳入 selected 的 ref 值，則單純為點擊按鈕，不會有 Toggle 狀態
            // 若為單純的可點擊按鈕 (非 checkbox 形式)，則直接呼叫無 ref 的 overload
            bool clicked = false;

            // shortcut 相關屬性先移除，列入 TODO，目前傳入 string.Empty
            clicked = ImGui.MenuItem(name, string.Empty, false, true); // 暫時寫死 true 代表 enabled，false 代表不會傳回 selected 狀態

            // 如果我們想保留 isChecked 狀態，可能需要提供另一種建構參數來區分是否為 ToggleItem
            // 這裡根據 Code Review，暫時將所有 Item 當作一般按鈕處理，如果有需要 Toggle 狀態可以擴充。
            // (目前為符合 Code Review 回饋「避免每個都是 Toggle」，先簡化為非 Toggle 版本)

            if (clicked)
            {
                var evt = EventPool<ClickEvent>.Get();
                evt.source = this;
                EventDispatcher.Enqueue(evt);
            }
        }

        /// <summary>
        /// 為了防止從外部直接透過邏輯樹加入子物件，覆寫此屬性。
        /// 在 VisualElement 的預設設計中，Add 會取用 contentContainer。
        /// 這裡直接回傳自己但如果在 Add 中被呼叫，後續有需要可以配合其他約束。
        /// </summary>
        private static readonly LogContext s_log = new LogContext("MenuItem");

        /// <summary>
        /// 為了防止從外部直接透過邏輯樹加入子物件，覆寫此屬性。
        /// 若外部試圖存取 contentContainer（例如 VisualElement.Add 預設行為），則拋出例外。
        /// </summary>
        public override VisualElement contentContainer
        {
            get
            {
                throw new NotSupportedException($"MenuItem '{name}' is a terminal node and cannot act as a container for children.");
            }
        }
    }
}
