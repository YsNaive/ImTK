# TODO / Task Tracker

這裡追蹤 ImTK 專案的功能開發與重構進度。
**AI Agent 注意**：嚴禁自動標記完成，必須與使用者確認後才能移動任務狀態。

## 🟢 進行中 (In Progress)
* 完善開發文檔規章與架構深度探勘 (目前階段)。

## 🔵 待處理 (To Do)
* （目前暫無未分配之需求）

## 🔴 已完成 (Done)
* 重構 `VisualElement`，確保 `onHierarchyChanged` 事件在延遲隊列觸發時，其物理與邏輯狀態已一致。
* 重構命名規範，將 `Horizontal` 更名為 `HorizontalView`，將 `WindowView` 更名為 `Window`。
* 擴充 `Window` 以支援多實例操作 (`instance.Open()`) 與動態名稱唯一性檢查。
* 擴充 `Window` 序列化機制 (`SerializeState`, `DeserializeState`) 以支援自訂資料持久化。
