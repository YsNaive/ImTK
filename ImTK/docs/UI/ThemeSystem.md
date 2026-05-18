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
