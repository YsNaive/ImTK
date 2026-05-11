# 更新日誌 (Changelog)

本專案的所有重要變更都將記錄於此文件中。

格式基於 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.1.0/)，
且本專案遵循 [語意化版本控制 (Semantic Versioning)](https://semver.org/lang/zh-TW/).

## [Unreleased]
### Added (新增)
- 實作 `Window` 視窗系統，作為 `VisualElement` 的根節點。支援單例 (`Window.Open<T>()`) 與多實例，並整合至 `Panel` 的全域生命週期 (`OnEnable`, `OnDisable`, `Update`) 進行集中控管。
- 實作了雙層視覺樹 (Logical vs Physical) 的自動同步 (Auto-Sync) 機制。當節點被移動時，會根據其 `NodeType` 自動從舊的邏輯或物理父節點乾淨脫離，徹底解決幽靈節點問題。
- 新增了延遲且整併 (Deferred & Debounced) 的 `HierarchyChangedEvent`，利用 `EventDispatcher` 與 `HashSet` 在 `OnLogicUpdate` 統一派發，消除了修改 UI 時的 `Collection was modified` 例外。
- 在 `VisualElement` 中引入 `m_useAutoId` 控制旗標。`Window` 預設停用此旗標以避開動態 ID，利用 ImGui 的 `###` 語法結合 `displayName` 與 `windowId` 達成穩定定位，修復了 Docking 版面無法記憶的問題。
- 新增 `AddRange` 批次操作方法至 `VisualElement` 系統，受惠於延遲事件機制，現在可以高效地建立大量子節點。
- 實作了完整的 `ImTKLog` 日誌與偵錯系統。支援零分配 (Zero-Allocation) 的條件式內插字串語法糖 (`xxxIf`)，並具備執行緒安全的 `ImmutableArray` 派發機制。
- 新增 `ILogSink` 架構與 `LogSinkBase`，並實作了 `ConsoleSink` 終端輸出。
- 於 `ImTKApplication.Initialize` 實作「階段零 (Phase 0) 雙重反射」，自動掃描並註冊所有的 `ILogSink` 實作。
- 實作 `LogFormatterBuilder` 提供鏈式調用 (Fluent API)，幫助文字終端客製化日誌格式 (包含置中的層級標籤與啟動時間戳記)。
- 於 `ImTKApplication` 的生命週期迴圈中全面整合 `try-catch` 例外捕捉與日誌記錄，提升框架除錯體驗並防止單點崩潰。
- 在 `ImTK.Core.Time` 模組中新增全域 `StartupTime`。
- 實作了核心的雙層生命週期架構 (`ImTKApplication`, `ImTKModule`, `ImTKObject`)。
- 引入了嚴格的 `ApplicationState` 狀態機，以防止主迴圈邏輯重入或順序錯誤。
- 新增 `Time` 靜態工具類別，自動管理全域的真實與縮放延遲時間 (Delta Time)。
- 新增 `ImTKSilk.Run()` 驅動程式，用於初始化 Silk.NET 並將 ImGui 控制器與 ImTK 生命週期完美整合。
- 透過 `ImTKSilkConstant` 啟用 ImGui Viewports，原生支援多視窗佈局。
- 於 `docs/` 目錄下建立基礎文檔結構，依子系統劃分 (`Core`, `UI`, `Database`, `Log`, `Event`, `Project`)。
- 新增嚴格的 `NamingConventions.md` 命名規範，確立大小寫、前綴以及元件後綴的規則。

### Changed (變更)
- 修正 `EventDispatcher` 的事件氣泡 (Bubbling) 路徑。事件現在嚴格沿著物理樹 (`hierarchy.parent`) 向上傳遞，修復了封裝內部元件 (Shadow DOM) 時事件斷層的問題。
- 將原有的 `Architecture/` 目錄下的架構設計文件重組並移至對應的子系統資料夾。
- 將 `AGENT.md` 移至專案根目錄，並擴充了 AI 代理的條列式探勘與開發指引。
- 修改 `DevelopmentWrapUp.md`，制定了合併程式碼前必須遵守的五步驟最終檢查清單 (SOP)。
