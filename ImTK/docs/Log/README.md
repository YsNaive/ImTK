# 日誌系統 (Log)

本目錄包含 ImTK 內部的日誌收集與分發架構設計。

## 包含的文件：

* **[`Logging.md`](Logging.md)**：詳述 `ImTKLog` 的設計。為了解決極早期的錯誤捕捉，該系統被設計為獨立的靜態架構，並支援 `ILogSink` (如 `MemoryRingBufferSink`) 以在 UI 介面上展示。
