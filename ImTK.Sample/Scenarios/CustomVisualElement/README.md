# 自訂 VisualElement 與樣式擴充 SOP

在 ImTK 架構中，當你需要建立一個具有特殊 ImGui 樣式屬性（例如自訂的標籤背景色、特殊的進度條顏色等）的客製化元件時，請嚴格遵守以下 SOP。

我們將原本共用的樣式映射機制，解耦為利用泛型與巢狀類別（`VisualElement<TStyle>`）的強型別架構。

## 1. 宣告客製化元件與其巢狀樣式類別

自訂元件應繼承自 `VisualElement<自訂元件.Style>`。並在元件內部定義 `StyleKey` 與 `Style` 類別，繼承自基底的 `VisualElement.StyleKey` 與 `VisualElement.Style`。

```csharp
using ImGuiNET;
using ImTK.Core;
using ImTK.UI;
using ImTK.UI.Style;

namespace ImTK.Sample.Scenarios.CustomVisualElement
{
    // 1. 繼承泛型基底
    public class Badge : VisualElement<Badge.Style>
    {
        // 2. 擴充專屬的 StyleKey (必須宣告為 public new class)
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString BadgeColor = new HashedString("BadgeColor");
        }

        // 3. 實作專屬的 Style 容器 (必須宣告為 public new class)
        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            // 覆寫 PushToImGui，把專屬的 Key 推送到指定的 ImGuiCol
            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);
                m_pushedColors = 0;

                // 透過自定義的 StyleKey 取得樣式值
                Color? badgeColor = resolvedStyle.GetColor(StyleKey.BadgeColor);
                if (badgeColor.HasValue)
                {
                    // 對應到你希望的 ImGui 特殊變數
                    ImGui.PushStyleColor(ImGuiCol.Button, badgeColor.Value.u32);
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

            // (可選) 提供 Syntax Sugar 屬性方便直接賦值
            public StyleValue<Color>? badgeColor
            {
                get => GetOverrideColor(StyleKey.BadgeColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.BadgeColor, value.Value);
                    else Clear(StyleKey.BadgeColor);
                }
            }
        }

        public string text { get; set; }

        public Badge(string text)
        {
            this.text = text;
            classList.Add("Badge"); // 掛載預設 CSS Class
        }

        // 4. 實作 ImGui 渲染邏輯
        protected override void OnRenderSelf()
        {
            // 此時 ImGuiCol.Button 已經被你的 Style 覆寫了
            ImGui.SmallButton(text);
        }
    }
}
```

## 2. 如何使用與套用樣式

因為我們使用了強型別的 `Style` 類別，你可以在程式碼中直接賦予值，且編譯器會提供 IntelliSense。

```csharp
var badge = new Badge("New");

// Inline 賦值
badge.style.badgeColor = Color.Red;
// 或綁定 Theme Token
badge.style.badgeColor = "--danger-color";
```

若要在全域 `StyleSheet` 中設定，你可以透過 `SetColor` 方法傳入自定義的 `HashedString` (不一定要提供 Syntax Sugar 方法)。

```csharp
var block = StyleSheet.Global.AddBlock("Badge");
// 使用剛剛定義的 HashedString 作為鍵值
block.SetColor(Badge.StyleKey.BadgeColor, Color.Yellow);
```

## 總結
1. 客製元件若需特殊 ImGui 樣式，請繼承 `VisualElement<MyComponent.Style>`。
2. 將擴充的鍵值放入 `public new class StyleKey : VisualElement.StyleKey`。
3. 將推播邏輯 (`PushToImGui`/`PopFromImGui`) 實作在 `public new class Style : VisualElement.Style` 中。
4. 預設的版面屬性 (如 BackgroundColor, Padding) 會由 `base.PushToImGui` 自動處理，不需重複實作。
