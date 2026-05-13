# 更新日誌 (Changelog)

本專案的所有重要變更都將記錄於此文件中。

格式基於 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.1.0/)，
且本專案遵循 [語意化版本控制 (Semantic Versioning)](https://semver.org/lang/zh-TW/).

## [Unreleased]
### Added (新增)
- 實作了 ImGui 選單系統 (`MenuView`, `MenuItem`)，支援容器與可點擊末端節點的職責分離。
- `MenuView` 支援基於 `priority` 的自動排序機制，以及對於跨度大於 50 的項目自動插入 `ImGui.Separator()`。
- 提供路徑式 (Path-based) 選單建立語法糖 `AddItem` 與 `AddMenu`，並具備節點衝突的型別防護。
- 新增 `MainMenuModule`，透過 `Panel.RequireArea` 配置保留頂層選單空間，實現全域 `MenuBar`。
- 新增 `[MainMenu(string path, int priority)]` 屬性標籤，支援對靜態方法、欄位與屬性進行全域 Assembly 掃描與自動選單掛載。
- `ImTK.UI.Button` 元件，提供基本的按鈕渲染能力與建構子語法糖 (`text`, `onClicked`)。
- `ImTK.UI.ClickEvent` 延遲事件，允許按鈕點擊事件透過 `EventDispatcher` 推遲至安全的 `LogicUpdate` 階段執行。
- `ImTK/docs/UI/Element/` 文檔目錄，用於收納未來新增的各類 UI 元件規格。

### Fixed (修復)
- 修正 `MainMenuModule` 在繪製 MenuBar 容器時，由於 ImGui 預設 `WindowMinSize` 大於我們給定的高度，導致產生透明 Hit-box 擋住下方 Docking 標題列滑鼠拖曳事件的問題（透過推送 `ImGuiStyleVar.WindowMinSize` 至 `0,0` 解決）。
- 修正 `TestRunnerModule` 內的 `TestReportWindow` 統計狀態異常問題（修正 Pending 顯示判斷與顏色標示）。
- 修正 `EventBubbleTest` 測試失敗問題：將原有的 `ImGui.Button` 替換為 `ImTK.UI.Button` 以解決在 `GuiRender` 階段因直接修改視覺樹而觸發的生命週期保護機制 (CheckSafeState)。


### Added (新增)
- 實作了標準化且輕量的測試框架 (`ImTK.Test`)，區分 `IHeadlessTest` 與 `IIntegrationTest`。
- 實作了可於 UI 顯示 Headless 與 Integration 測試報表的 `TestRunnerModule` 儀表板，並引入輕量斷言庫 `ImTKAssert`。
- 實作了標準化範例框架 (`ImTK.Sample`)，引入 `ISampleScenario` 以支援自動註冊與展示。
- 新增 `SampleOverviewModule`，自動生成範例總覽面板與文檔連結。
- 實作了強大且安全的雙軌資源系統 (`Asset System`)，將唯讀的全域資源存取 (`ImTK.Database.Resource`) 與可讀寫的本地資料庫 (`ImTK.Database.ImTKDatabase`) 完全分離，避免了路徑與權限錯誤。
- 引入了 `ImTKEnvironment` 靜態環境管理器，自動處理跨平台的資源根目錄 (`GlobalAssetPath` 與 `LocalDataPath`) 解析。
- 實作了泛型導向的資源反序列化架構 (`ImTKAsset` 與 `ImTKSaveableAsset`)，取代傳統龐大的 Loader 系統。
- 實作了防禦性的 `AssetManager`，並針對路徑穿越 (Directory Traversal) 漏洞進行了深度防禦。
- 內建提供 `JsonAsset<T>`，簡化了基於 `System.Text.Json` 的 POCO 設定檔存取與存檔開發流程。
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
- 將 `VisualElement.Render()` 由 `internal` 修改為 `public`，確立其為驅動視覺樹渲染的公開鎖定入口，並徹底移除了內部為了規避權限而使用的 C# Reflection 渲染呼叫，大幅提升效能。
- 重構了現有 Database 相關測試，將其整合至新的 `IHeadlessTest` 框架中。
- 於 `AGENT.md` 補齊 `ImTKModule` 必須擁有「單一、無參數、非公開」建構函式的限制規範。
- 修正 `EventDispatcher` 的事件氣泡 (Bubbling) 路徑。事件現在嚴格沿著物理樹 (`hierarchy.parent`) 向上傳遞，修復了封裝內部元件 (Shadow DOM) 時事件斷層的問題。
- 將原有的 `Architecture/` 目錄下的架構設計文件重組並移至對應的子系統資料夾。
- 將 `AGENT.md` 移至專案根目錄，並擴充了 AI 代理的條列式探勘與開發指引。
- 修改 `DevelopmentWrapUp.md`，新增了「回歸測試與維護檢查 (Regression Testing & Maintenance)」SOP。
