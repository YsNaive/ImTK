# TODO / Task Tracker

這裡追蹤 ImTK 專案的功能開發與重構進度。
已完成的項目將記錄於根目錄的 `CHANGELOG.md` 中。

**AI Agent 注意**：嚴禁自動標記完成，必須與使用者確認後才能移動或刪除任務狀態。

## 🟢 進行中 (In Progress)
* （無）

## 🔵 待處理 (To Do)
* 實作 `ImTKDatabase` 執行期資源快取管理器與 `IAsset` 介面。
* 實作 `ImTKLog` 日誌系統（包含 `LogContext`, `ILogSink` 介面與 `MemoryRingBufferSink`）。
* 實作 `ImTKDispatcher` 執行緒調度與 `ImTKEventBus` 隱式生命週期解綁事件系統。
* 實作 `VisualElement` 的邏輯/物理樹雙軌指標 (`parent` vs `physicalParent`)，與事件冒泡機制 (`UIEventBase`, `RegisterCallback`)。
