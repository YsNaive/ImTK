# Styling Developer Experience (DX)

本範例展示了 ImTK 樣式系統中，為提升開發體驗而設計的各項「語法糖 (Syntactic Sugar)」與「自動映射 (Auto-Mapping)」機制。

## 展示重點

1. **`colorFamily` 自動映射**
   - 展示如何透過一行 `style.colorFamily = ThemeColorFamily.Danger;` 將按鈕的各個互動狀態（預設、懸停、按下）以及文字顏色一次性對齊主題色。
   - 基礎 `VisualElement` 設定 `colorFamily` 時，會自動成為該主題色的 Container，讓內部子物件繼承對應的文字顏色。

2. **`StyleThickness` (Padding / Margin)**
   - 支援隱式轉換：
     - 單一 `float`：代表四邊等寬（例如 `style.padding = 20;`）。
     - `Vector2`：代表水平與垂直等距（例如 `style.padding = new Vector2(20, 10);`）。

3. **`StyleSpacing` (間距設定)**
   - 支援隱式轉換單一 `float` 為 `Vector2`，使得設定等距間隔更加簡潔（例如 `style.itemSpacing = 15;`）。

4. **`StyleFontSize` (字體縮放)**
   - 完美兼容列舉與絕對像素大小：
     - `style.fontSize = FontSize.H2;` (套用主題預設標題大小)
     - `style.fontSize = 18;` (直接指定 18px，底層自動推入對應的 FontScale)

5. **`StyleColor` (顏色支援)**
   - 支援多種直覺的顏色賦值方式：
     - Hex 字串 (`style.textColor = "#FFFF00";`)
     - `uint` 數值 (`style.backgroundColor = 0xFF333333;`)
     - ImTK `Color` 結構。

這些設計不僅大幅減少了冗餘的 UI 建置程式碼，也完美契合了全新 C# 物件導向架構的流暢度。
