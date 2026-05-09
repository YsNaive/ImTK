# ImTK 框架開發文檔索引 (Documentation Index)

本目錄存放了 ImTK 框架的所有的技術規範、架構設計藍圖與開發文件。
為保持架構清晰，文檔根據「子系統 (Subsystem)」拆分至各個資料夾中，目錄結構與程式碼的 Namespace 大致對應。

## 子目錄導覽 (Subdirectories)

* **[`Project/`](Project/)**
  專案層級的管理規範，包含待辦事項 (`TODO.md`)、命名規範 (`NamingConventions.md`) 與開發規章。
* **[`Core/`](Core/)**
  框架的啟動基礎與全域雙層生命週期架構，包含 `ImTKApplication`, `ImTKModule`, `ImTKObject` 的設計決策。
* **[`UI/`](UI/)**
  視覺元件的核心底層，包含 `VisualElement` 的邏輯樹與物理樹設計、佈局系統與 UI 事件機制。
* **[`Database/`](Database/)**
  全域資源管理器 (`ImTKDatabase`) 與檔案/資源 I/O 的架構設計。
* **[`Log/`](Log/)**
  極早期初始化的日誌系統 (`ImTKLog`) 設計。
* **[`Event/`](Event/)**
  跨模組事件系統 (`EventBus`) 與主執行緒調度器 (`ImTKDispatcher`) 的設計。
