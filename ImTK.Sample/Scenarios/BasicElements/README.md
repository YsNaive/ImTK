# 基礎元件 (Basic Elements)

本範例展示了 ImTK 中最基礎的 UI 元件：

* **`TextElement`**：用於顯示純文字，無法互動。
* **`CheckBox`**：布林值開關，可切換 true/false，並可訂閱 `onValueChanged` 事件。
* **`TextField`**：字串輸入框，可設定字串長度上限，並可訂閱 `onValueChanged` 事件。

這三個元件是最常使用的表單與顯示元件，透過設定 `value` 屬性即可輕易地修改其內部狀態。
