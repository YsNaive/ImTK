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


            public StyleColor? hoverColor
            {
                get => GetPropertyColor(StyleKey.HoverColor);
                set => SetPropertyColor(StyleKey.HoverColor, value);
            }

            public StyleColor? activeColor
            {
                get => GetPropertyColor(StyleKey.ActiveColor);
                set => SetPropertyColor(StyleKey.ActiveColor, value);
            }





            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken)
                {
                    if (prop.key == VisualElement.StyleKey.ColorFamily.Hash)
                    {
                        string prefix = "--normal";
                        if (prop.enumValue == (int)ThemeColorFamily.Success) prefix = "--success";
                        else if (prop.enumValue == (int)ThemeColorFamily.Info) prefix = "--info";
                        else if (prop.enumValue == (int)ThemeColorFamily.Warning) prefix = "--warning";
                        else if (prop.enumValue == (int)ThemeColorFamily.Danger) prefix = "--danger";

                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.Header, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component").Hash });
                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.HeaderHovered, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-hover").Hash });
                        output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.HeaderActive, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-active").Hash });
                    }
                    else if (prop.key == VisualElement.StyleKey.BackgroundColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.Header;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.HoverColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.HeaderHovered;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.ActiveColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.HeaderActive;
                        output.Add(prop);
                        return;
                    }
                }
                base.ComputeHighlevelToken(prop, output);
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
            m_useNativeLayout = true;
            if (onClicked != null)
            {
                this.onClicked += onClicked;
            }
        }

                protected internal override bool CheckHoverState()
        {
            return ImGuiNET.ImGui.IsItemHovered(ImGuiNET.ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
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


        /// <summary>
        /// 為了防止從外部直接透過邏輯樹加入子物件，覆寫此屬性。
        /// 若外部試圖存取 contentContainer（例如 VisualElement.Add 預設行為），則拋出例外。
        /// </summary>
        public override VisualElement contentContainer
        {
            get
            {
                ImTKLog.Error($"MenuItem '{name}' is a terminal node and cannot act as a container for children.");
                return this;
            }
        }
    }
}
