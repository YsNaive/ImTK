# 核心生命週期 (Core)

本目錄包含 ImTK 框架最底層的啟動、驅動與物件生命週期設計架構。

## 包含的文件：

* **[`Global_Architecture.md`](Global_Architecture.md)**：從 OS 啟動執行檔到完全退出的全域巨觀生命週期藍圖。
* **[`Lifecycle.md`](Lifecycle.md)**：雙層架構設計的微觀說明。詳述了 `ImTKApplication` 狀態機防護機制，以及 `ImTKModule` (系統級單例) 與 `ImTKObject` (動態邏輯物件) 的介面與階段性 Hooks。
