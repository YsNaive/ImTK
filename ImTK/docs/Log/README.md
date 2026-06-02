# 日誌系統 (Log)

本模組提供了 ImTK 框架極早期初始化的全域日誌記錄功能。支援「破曉緩衝區 (Early Logs)」自動重播機制，以及達成 Zero-Allocation (零記憶體分配) 的條件式字串插值語法糖。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 基礎日誌寫入 (Standard Logging)

所有的日誌寫入皆透過 [`ImTKLog`](../../Log/ImTKLog.cs) 靜態類別。它會自動擷取呼叫端的檔案名稱 (`CallerFilePath`) 作為 ModuleName。

*   `ImTKLog.Trace(string message)`
*   `ImTKLog.Debug(string message)`
*   `ImTKLog.Info(string message)`
*   `ImTKLog.Warning(string message)`
*   `ImTKLog.Error(string message)` / `ImTKLog.Error(Exception ex, string message)`
*   `ImTKLog.Fatal(string message)` / `ImTKLog.Fatal(Exception ex, string message)`

### 2. 條件式日誌 (Zero-Allocation Logging)

**推薦使用**。透過 C# 的 `InterpolatedStringHandler` 機制，當布林條件不成立時，**字串的插值運算會被編譯器完全跳過**，徹底省下 GC 與 CPU 開銷。

*   `ImTKLog.TraceIf(bool condition, $"{複雜變數與字串插值}")`
*   `ImTKLog.DebugIf(...)`
*   `ImTKLog.InfoIf(...)`
*   `ImTKLog.WarningIf(...)`
*   `ImTKLog.ErrorIf(...)`
*   `ImTKLog.FatalIf(...)`

### 3. 日誌輸出端 (Log Sinks)

日誌系統支援多端輸出。如果在任何 Sink 被註冊「之前」就呼叫了 `ImTKLog.Info()`，這些日誌會被暫存，並在第一個 Sink 加入時自動完美回放。

*   **[`ILogSink`](../../Log/ILogSink.cs)**: 欲接收日誌的模組必須實作此介面。
*   **[`ConsoleSink`](../../Log/ConsoleSink.cs)**: 內建的預設輸出端，將帶有顏色的日誌打印至終端機。
*   `ImTKLog.AddSink(ILogSink sink)`: 動態加入輸出端。
*   `ImTKLog.RemoveSink(ILogSink sink)`: 動態移除輸出端。
*   `ImTKLog.SetSinkEnabled<T>(bool enabled)`: 透過泛型快速開關特定型別的 Sink。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討底層效能與設計模式的技術文件：

*   **[`Logging.md`](Logging.md)**：深入探討為何日誌系統能在 Phase 0 (極早期) 運作的秘密、破曉緩衝區 (`ConcurrentQueue`) 的防丟失機制，以及 `InterpolatedStringHandler` 如何實作出 C++ 巨集等級的效能。
