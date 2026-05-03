# TODO / Task Tracker

這裡追蹤 ImTK 專案的功能開發與重構進度。
**AI Agent 注意**：嚴禁自動標記完成，必須與使用者確認後才能移動任務狀態。

## 🟢 進行中 (In Progress)
* （無）

## 🔵 待處理 (To Do)
* 實作全新的雙層生命週期架構 (`ImTKModule` & `ImTKObject`) 與階段性 Hooks (OnInitializeSelf, OnGraphicsSetup 等)。
* 實作 `ImTKDatabase` 執行期資源快取管理器與 `IAsset` 介面。
* 實作 `ImTKLog` 日誌系統（包含 `LogContext`, `ILogSink` 介面與 `MemoryRingBufferSink`）。
* 實作 `ImTKDispatcher` 執行緒調度與 `ImTKEventBus` 隱式生命週期解綁事件系統。
* 實作 `VisualElement` 的邏輯/物理樹雙軌指標 (`parent` vs `physicalParent`)，與事件冒泡機制 (`UIEventBase`, `RegisterCallback`)。

## 🔴 已完成 (Done)
* 完善開發文檔規章與架構深度探勘（建立次世代架構藍圖：00~05 全系列架構設計文檔）。
* 重構 `VisualElement`，確保 `onHierarchyChanged` 事件在延遲隊列觸發時，其物理與邏輯狀態已一致。
* 重構命名規範，將 `Horizontal` 更名為 `HorizontalView`，將 `WindowView` 更名為 `Window`。
* 擴充 `Window` 以支援多實例操作 (`instance.Open()`) 與動態名稱唯一性檢查。
* 擴充 `Window` 序列化機制 (`SerializeState`, `DeserializeState`) 以支援自訂資料持久化。
