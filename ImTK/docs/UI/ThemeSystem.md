> Warning: This document has been updated to reflect the new Style System architecture involving RenderEngine Pipeline.


# 主題與樣式系統 (Theme & Style System)

ImTK 的主題與樣式系統分為兩層架構：

1. **元件層級 (`VisualElement.Style` & `VisualElement.StyleKey`)**
   這是底層物理層的映射，用來直接控制 `ImGui.PushStyleColor` 與 `ImGui.PushStyleVar`。此處的 Key 都是明確與 ImGui API 綁定的，例如 `BackgroundColor`, `Padding`, `ItemSpacing`。

2. **全域主題層級 (`ImTKTheme`)**
   這是一個高階的、帶有「語義」的設計系統。它利用 `HashedString` 來作為鍵值快取，避免執行時期的字串雜湊開銷。

## ColorFamily (色彩家族)
在 `ImTKTheme` 中，我們不再只是單獨提供一個主色或背景色，而是定義了多個 `ColorFamily`：
* `normalColor`
* `successColor`
* `infoColor`
* `warningColor`
* `dangerColor`

每個 `ColorFamily` 都統一包含了以下屬性：
* `background`, `subBackground`
* `foreground`, `subForeground`
* `text`, `subText`, `disabledText`

這種設計使得元件（例如 `Button` 或 `Badge`）可以輕易地綁定整套狀態色彩，而不用逐一設定。

## 共用版面參數
`ImTKTheme` 也提供了全域的版面預設值：
* `padding` (Vector2)
* `itemSpacing` (Vector2)
* `itemInnerSpacing` (Vector2)
* `borderWidth` (float)
* `borderRadius` (float)
* `disabledAlpha` (float)

這確保了專案在不同元件之間能擁有一致的留白與外觀風格。

## 字型系統 (Font System)

ImTK 的字型系統被設計為獨立且可動態重載的模組 (`ImTKFontManager`)，這解決了 ImGui 建立字型圖集 (Font Atlas) 時缺乏彈性的問題：

1. **多重字型註冊 (FontFamily)**：
   開發者可以透過 `ImTKFontManager.RegisterFamily("MyFamily", new string[] { "Primary.ttf", "Fallback.ttf" })` 來註冊字型。系統會自動利用 ImGui 的 `MergeMode` 來把這些字型檔案合併成單一個 FontFamily，當遇到第一套字型缺少的字符 (Glyph) 時，自動使用後面的字型進行渲染。最後永遠會加入 ImGui 內建的預設點陣字型作為保底。

2. **混合式字型大小策略 (Hybrid Font Size Strategy)**：
   * **預先載入**：全域的 `ImTKTheme.GlobalTheme` 會定義各個 `FontSize` (Small, Normal, H3, H2, H1) 所對應的真實像素大小。當字型圖集建立時，會根據已註冊的 FontFamily 與這些全域大小組合，產生對應的 `ImFontPtr`。
   * **彈性縮放**：當元件在局部設定 `element.style.fontSize = 20;` 時，系統並不會即時引發耗時的字型圖集重構。相反地，它會就近尋找大於或等於 20 的預先載入字型（例如 `H3 = 24`），然後利用 `ImGui.SetWindowFontScale` 進行縮小（以保持字型銳利度）。

3. **字型狀態繼承與渲染上下文 (RenderingContext)**：
   為了解決 ImGui 狀態堆疊機制的限制，系統引入了 `RenderingContext`。這確保了在執行 `VisualElement.Render()` 時，字型操作 (特別是 FontScale 變化與 FontFamily 的狀態追蹤) 能在物件樹游離的情況下正確繼承：
   * 當子元件僅設定 `FontSize` 而沒有 `FontFamily` 時，會自動回退 (Fallback) 至 `RenderingContext.CurrentFontFamilyHash` 取得當前有效的 FontFamily 指標。
   * 會引起 ImGui 斷言衝突的指令（如需要在 Window 內執行的 `ImGui.SetWindowFontScale`），會利用 `RenderingContext.EnqueueWindowCommand` 暫存，直到 `Window.Begin` 完成後才被正確執行，避免出現錯誤的 Debug 視窗。

4. **安全熱重載 (Safe Hot-Reload)**：
   若系統觸發了圖集更新 (`ImTKFontManager.MarkFontDirty()`)，核心會在下一幀 `LogicUpdate` 的最開頭重建圖集，確保與其他執行緒和 ImGui 內部狀態不衝突。完成後發送全域事件 `OnFontChangedEvent`，由底層的橋接層 (如 Silk.NET) 捕獲並通知 GPU 重建 Texture。

## 4. 預設樣式表 (DefaultStyles)
`DefaultStyles` 曾經用於註冊全域預設樣式。目前多數元件預設樣式已交由 `ImTKTheme` 與 C++ 底層直接映射處理，此類別現保留用作未來擴展或套用自訂全域 CSS-like `StyleBlock` 的進入點。
