# 開發者除錯工具 (DebugTools)

本模組提供了 ImTK 框架內建的視覺化除錯工具集，幫助開發者在執行期即時分析 UI 效能、檢視日誌與剖析視覺樹結構。

---

## ⚡ 快速速查表 (Quick Reference)

這些工具通常會自動註冊到上方主選單 (MainMenu) 的 `Window/Debug` 類別中，您可以隨時點擊開啟：

### 1. 視覺樹檢視器 (Visual Element Inspector)

*   **[`VisualElementInspectorWindow`](../../DebugTools/VisualElementInspectorWindow.cs)**:
    *   提供類似瀏覽器開發者工具 (F12) 或 Unity UI Toolkit Debugger 的功能。
    *   **核心功能**：即時顯示目前畫面上所有 `VisualElement` 的階層關係，並允許在執行期動態檢視與修改 UI 元素的樣式 (Style) 與排版屬性。

### 2. 即時日誌視窗 (Log Viewer)

*   **[`LogViewerWindow`](../../DebugTools/LogViewerWindow.cs)**:
    *   內建的視覺化日誌接收端 (`ILogSink` 實作)。
    *   **核心功能**：提供過濾 (Trace/Info/Warning/Error) 與文字搜尋功能，並能點擊展開詳細的 Exception StackTrace。

### 3. 效能與記憶體監控 (Performance Monitor)

*   **[`PerformanceMonitorWindow`](../../DebugTools/PerformanceMonitorWindow.cs)**:
    *   結合 `ImTKProfiler` 的視覺化前端。
    *   **核心功能**：提供即時的 FPS (Frames Per Second) 走勢圖、垃圾回收 (GC) 記憶體分配量，以及各個 Pipeline 階段 (如 Measure, Arrange, Render) 的 CPU 耗時長條圖。

---

## 🔧 使用與擴充

本模組作為獨立的 `ImTKModule` 運作，因此所有的除錯視窗都不會干擾正式專案的核心邏輯。
開發者若需擴充新的除錯工具，可參考 `VisualElementInspectorWindow` 的架構，結合 `ImTKDatabase` 或 `ImTKLog` 的事件掛載來完成資料蒐集與呈現。
