# 基礎元件 (Basic Elements)

在 `ImTK/UI/Element` 目錄下，我們提供了一系列封裝 ImGui 基礎操作的元件。這些元件皆繼承自 `VisualElement`，支援 ImTK 的事件系統與佈景主題樣式設定。

## TextElement
`TextElement` 是一個輕量級的文字顯示區塊元件。它底層呼叫了 `ImGui.TextUnformatted` 並預設開啟換行 (`enableWordWrap = true`) 與 `Overflow.Visible`，適合用來展示多行敘述或長篇內容。
* **主要屬性**：`text` (string), `enableWordWrap` (bool)

## Label
`Label` 繼承自 `TextElement`，專為「單行標籤」情境設計。它預設關閉換行 (`enableWordWrap = false`) 並啟用裁切 (`style.overflow = Overflow.Hidden`)，確保在嚴格的排版容器內不會將排版撐破或將文字擠至下一行。
* **主要屬性**：`text` (string)

## CheckBox
`CheckBox` 是一個布林值開關元件。
* **主要屬性**：`label` (顯示在核取方塊旁邊的文字)、`value` (布林值)。
* **事件綁定**：可透過 `onValueChanged` 訂閱數值變更的事件。

## TextField
`TextField` 是一個單行文字輸入框。
* **主要屬性**：`label` (輸入框前方的標籤)、`value` (輸入的字串)、`maxLength` (字串長度上限，預設 1024)。
* **事件綁定**：可透過 `onValueChanged` 訂閱使用者輸入變更。
