# 事件與調度器 (Event)

本目錄包含 ImTK 跨模組溝通以及執行緒調度的架構設計。

## 包含的文件：

* **[`EventBus.md`](EventBus.md)**：詳述全域事件系統 `EventBus`，包含透過基底類別代理訂閱，實現無痛的生命週期綁定與自動解除訂閱機制，以及 `ImTKDispatcher` 的跨執行緒調度設計。
