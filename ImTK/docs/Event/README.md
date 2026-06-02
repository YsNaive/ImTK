# 事件與調度器 (Event)

本模組提供了 ImTK 框架跨模組溝通的**全域事件系統 (`ImTKEventBus`)** 以及跨執行緒的安全調度機制 **(`ImTKDispatcher`)**。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 全域事件匯流排 (Event Bus)

負責跨模組的鬆耦合通訊。所有事件的處理程序皆會被自動派發至主執行緒執行。

*   **[`ImTKEventBus`](../../Event/ImTKEventBus.cs)**: 全域事件派發核心。
    *   `Publish<T>(T evt)`: 發布事件給所有訂閱者。
    *   `GlobalSubscribe<T>(Action<T> handler)`: (不建議直接使用) 註冊事件，一般應透過 `ImTKObject.SubscribeEvent<T>` 來達成生命週期自動解綁。

### 2. 執行緒調度器 (Dispatcher)

確保需要操作 UI 或 OpenGL 資源的背景任務，能夠安全地在主執行緒 (LateUpdate 階段) 執行。

*   **[`ImTKDispatcher`](../../Event/ImTKDispatcher.cs)**: 執行緒防護與任務佇列。
    *   `IsMainThread`: (Property) 檢查當前是否為主執行緒，常用於 `Assert` 防呆。
    *   `Enqueue(Action action)`: 將一段函式推入佇列，等待主執行緒空閒時安全執行。若當下已是主執行緒，則會立即同步執行。

### 3. 內建全域事件列表 (Available Events)

所有全域事件必須實作 **[`IImTKEvent`](../../Event/IImTKEvent.cs)** 介面。

*   **[`OnFontChangedEvent`](../../Event/OnFontChangedEvent.cs)**: 當系統全域字體變更時觸發，通知各模組或 UI 重新計算排版與字體配置。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討底層設計的技術文件：

*   **[`EventBus.md`](EventBus.md)**：深入探討為何需要型別安全事件、生命週期自動解綁 (Implicit Unsubscription) 如何解決 Memory Leak 問題，以及與 UI 區域事件 (UI Event) 的職責差異。
