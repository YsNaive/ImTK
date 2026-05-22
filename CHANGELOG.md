# 更新日誌 (Changelog)

本專案的所有重要變更都將記錄於此文件中。

格式基於 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.1.0/)，
且本專案遵循 [語意化版本控制 (Semantic Versioning)](https://semver.org/lang/zh-TW/).

## [Unreleased]

### Added (新增)
- 實作了 Drawer 的絕對定位排版機制 (`overrideRenderRect`)，支援在 `Vector2Drawer`, `RectDrawer` 等複合 Drawer 中複用多個 `FloatDrawer` 並且完美維持單行顯示，同時改善了標籤字型的垂直對位置中。
- 實作了 `FoldoutDrawer<T>` 作為可折疊的內容抽屜基底類別，利用 ImDrawList 自定義繪製三角形圖示，並支援整行可點擊的 hover 視覺反饋。
- 將 `ObjectDrawer` 的繼承基底改為 `FoldoutDrawer<object>`，使得物件屬性面板能天然支援展開與折疊。
- 替 `FieldDrawer<T>` 新增預設圖示佔位空間 (`OnRenderIcon`)，以此取代繁瑣的 Indent Level 縮排邏輯，達成統一且天然的排版對齊 (`[icon][label][content]`)。
- 實作了基礎 UI 元件 `IntField` 與 `FloatField`，分別綁定 ImGui 的數值輸入並消除冗餘按鈕。
- 為 `FieldDrawer<T>` 新增 `RegisterValueChangedCallback` 與 `UnregisterValueChangedCallback` 語法糖。
- 為 `TextField` 實作自動適應高度的動態渲染，確保 `InputTextMultiline` 的流暢使用體驗。
- 實作了 `FontSource` 類別，封裝字型路徑與 `GlyphRanges`，支援自動偵測作業系統 (Windows, macOS, Linux) 預設字型目錄，並容錯檔名與副檔名的自動補全。
- `ImTKTheme` 中新增了 `globalFontScale` 全域字型縮放係數。該參數會在建置 Font Atlas 時將字型像素等比例放大，且在執行期自動換算，完美實現邏輯像素與物理像素的分離。
- `ImTKTheme` 新增了 `fontFamily` 參數，提供全域字型的統一切換入口，這修復了 ImGui DockSpace 分頁標籤 (Tabs) 無法正確吃到視窗自訂字型的 Bug。
- 在 `ImTKFontManager` 中新增了 `OverrideDefaultFamily` API，方便開發者直接覆寫底層的 `ImGuiDefault` 系統字型，而無需自訂 Theme。
- 實作了雙事件系統架構：建立全域的 `ImTKEventBus` 搭配 `ImTKDispatcher` 處理跨模組非同步廣播 (`OnXXXEvent`)。
- 在 `ImTKObject` 與 `ImTKModule` 實作了 `SubscribeEvent<T>`，利用 `OnDisable` 生命週期自動清理註冊，防止 Memory Leak。
- 新增 `EventBusTest` 整合測試，驗證全域事件發布、主執行緒調度及自動解綁邏輯。
- **ImTK.Sample**: Introduced new robust macro-architecture for sample scenarios.
- **ImTK.Sample**: Added `ScenarioListElement` and `ScenarioDetailElement` (both inheriting `VisualElement`) to cleanly decouple UI components from framework logic.
- **ImTK.Sample**: Expanded `ISampleScenario` with metadata for `Category`, `Order`, and `SeeAlso` (cross-referencing links) and implemented a `SampleScenarioBase` default implementation to reduce boilerplate.
- **ImTK.Sample**: Implemented split panel layout utilizing `Panel.RequireArea` during `OnInitializeDependencies` to allocate fixed screen regions for Lists (Left) and Details (Bottom-Right), pushing newly opened Demo `Window`s into the central right DockSpace automatically.
- 將 `ComputeStyle` 的靜態計算重構為每個元素實例持有的 `ResolvedStyle`。
- 實作了由上而下 (Top-Down) 的樣式繼承機制與 `ImGui` 推送防污染邏輯。
- 將 `StyleProperty` 結構體壓縮至完美的 16 Bytes 並統整 `StyleVarType` 與 `StyleKeyword` 為 `StylePropertyType`。
- 修正 `NamingConventions.md` 中對於 Instance Properties 和 Static Fields 的混淆規則。
- 實作了基礎 UI 元件 (`TextElement`, `CheckBox`, `TextField`)，封裝 ImGui 基礎操作並支援安全的值綁定與狀態事件。
- 重構了 `ImTKTheme` 主題系統，導入基於 `ColorFamily` 的高階語義群組 (如 `normalColor`, `successColor`)。
- 擴充並標準化了 `ImTKStyleKey`，移除不適用於 ImGui 的 CSS 屬性，新增支援 Layout (`Padding`, `ItemSpacing`) 與透明度 (`DisabledAlpha`) 的物理對應。
- 優化了 `ImTKTheme` 與 `ImTKStyleKey` 之間的 `HashedString` 快取，達成零分配讀取，避免每幀執行字串雜湊運算。
- 實作了高效的 CSS 子集樣式系統 (`StyleSystem`)，具備 Inline > Local > Global 的優先級層疊機制。
- 導入了 `HashedString` 與 `ImTKStyleKey` 高階列舉，徹底解耦樣式語義與底層 ImGui 變數，達成 O(1) 極速查表與零記憶體分配 (Zero GC Allocation)。
- 實作了基於字串 Token 的全新 `ImTKTheme` 設計系統，移除舊有死板的屬性，提供極大擴展性與 fallback 偵錯警告 (如遇到找不到的 Token 會回報警告並使用 Color.Magenta)。
- 引進 `StyleKeyword.Null` 搭配泛型 `StyleValue<T>` 結合隱式轉換，提供開發者流暢且兼具強型別安全與 CSS 習性的語法糖體驗。
- 實作 `StyleClass` 管理 `VisualElement` 的 `classList` (Add/Remove/Toggle)，並結合 `ComputeStyle.Overlay` 延遲快取，徹底解放每幀重新計算的效能負擔。
- 實作了 `FieldDrawer` 系統，支援動態綁定資料型別至 UI 元件 (類似 Unity PropertyDrawer)。
- 實作了雙向資料流與安全攔截機制 (`SetValueWithoutNotify`, `SetValueWithChanged`)，避免 DataBinding 循環。
- 新增了 `ValueChangedEvent<T>` (繼承自輕量級 `IValueChangedEvent`)，支援 `isInternalChange` 判斷，且預設不冒泡。
- 實作了 `FieldDrawerRegistry`，支援基於 `[CustomFieldDrawer]` 的型別映射、繼承遞迴回退與 `requiredModifier` 修飾器過濾。
- 實作了 `FieldDrawerFactory` 提供 Fluent API 動態建置 Drawer 與注入修飾器屬性。
- 實作了 `ObjectDrawer`，利用反射與 `IValueChangedEvent` 將 UI 的複雜嵌套修改雙向寫回原始物件。
- 在 `ImTKTheme` 引入 `DrawerLabelWidth`，並在 `FieldDrawer` 中實作 `Inline` / `Expand` 自動對齊排版。
- 實作 `ImTK.Color` 結構，支援 RGBA/HSV 色彩轉換，以及與 `Vector4` 和 `uint` (ImGui 原生格式) 的隱式/顯式轉換。
- 實作了記憶體最佳化的 `VisualElementStyle` (Style 系統)。透過 C# Union 結構 (`StyleEntry`) 與延遲初始化陣列，達成修改樣式時的零裝箱 (Zero Boxing) 與極低記憶體開銷。
- 實作了基於層疊與 Fallback 機制的 `ImTKTheme` (Theme 系統)。允許透過 `parent` 指標輕鬆建立繼承變體，並在 `Render()` 管線中自動處理 Override 與 Theme 之間的優先級競爭。
- 實作了 `ElementFlags<T>` 泛型位元運算基底，讓特定 UI 元件 (如 `Window`) 可以使用直覺的布林屬性語法糖 (`window.flags.noTitleBar`) 來封裝 ImGui 底層繁瑣的 Enum 操作。
- 為 `VisualElement`, `Window`, `Button` 實作了 `ApplyTheme` 虛擬方法，實現組件的預設主題映射。
- 於 `ImTK.Test` 專案中補齊了 `ColorTests`, `ElementFlagsTests`, `StyleThemeTests` 全面測試，並在報表視窗新增了即時切換明暗主題預覽的功能。
- 更新 `ImTK/docs/UI/VisualTree.md` 文件，將 Style 系統與 Theme 系統詳述整合其中。
- 實作了 ImGui 選單系統 (`MenuView`, `MenuItem`)，支援容器與可點擊末端節點的職責分離。
- `MenuView` 支援基於 `priority` 的自動排序機制，以及對於跨度大於 50 的項目自動插入 `ImGui.Separator()`。
- 提供路徑式 (Path-based) 選單建立語法糖 `AddItem` 與 `AddMenu`，並具備節點衝突的型別防護。
- 新增 `MainMenuModule`，透過 `Panel.RequireArea` 配置保留頂層選單空間，實現全域 `MenuBar`。
- 新增 `[MainMenu(string path, int priority)]` 屬性標籤，支援對靜態方法、欄位與屬性進行全域 Assembly 掃描與自動選單掛載。
- `ImTK.UI.Button` 元件，提供基本的按鈕渲染能力與建構子語法糖 (`text`, `onClicked`)。
- `ImTK.UI.ClickEvent` 延遲事件，允許按鈕點擊事件透過 `EventDispatcher` 推遲至安全的 `LogicUpdate` 階段執行。
- `ImTK/docs/UI/Element/` 文檔目錄，用於收納未來新增的各類 UI 元件規格。
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
- 新增嚴格的 `Project/NamingConventions.md` 命名規範，確立大小寫、前綴以及元件後綴的規則。

### Changed (變更)
- 重構了 FieldDrawer 的內部架構，包含 `IntDrawer`, `FloatDrawer`, `StringDrawer`, `BoolDrawer`，全面改用元件組合 (`Composition`) 取代原生的直接渲染。
- 於 `IntDrawer` 與 `FloatDrawer` 實作了類似 Unity Inspector 的 Hold-and-Drag 標籤拖曳互動來改變數值。
- 更新了基礎元素的 CSS 命名以符合標準 `kebab-case` (`text-field`, `check-box` 等)。
- 重新命名 `Drawer` 腳本檔案 (`IntField.cs` -> `IntDrawer.cs` 等) 消除與基礎 UI 元件的名稱衝突。
- 修改 `ImTKFontManager.ResolveFont()` 實作，針對字型建置的過程引入了 `Stopwatch` 時間追蹤日誌。
- 優化了 `FontSource` 的字型解析，改採副檔名字串嚴格比對 (如 `.ttf`, `.otf`)，避免路徑檔名內部含有 `.` (如 `font.v2`) 時造成的錯誤裁斷。
- 重構了 UI 區域事件 (`UIEventBase`) 的冒泡機制，利用虛擬屬性 `bubbles` 遵守開閉原則 (OCP)，移除原本 `is IValueChangedEvent` 的硬編碼判斷。
- 移除了 `ImTK.Test` 中 `TestRunnerModule` 內遺留的舊版主題切換功能。
- 實作了全新語意化的 14-token `ColorFamily` 系統，並重構 `ImTKTheme.ApplyToImGui` 映射。
- 替 `Window` 元件新增 `TitleBarColor`, `TitleBarActiveColor`, `TitleBarCollapsedColor` 等專屬樣式映射。
- 撰寫 `ThemeAndStyleMapping.md` 設計文檔定義樣式映射與色彩約束。
- 新增 `ImTKTheme.GlobalTheme` 全域主題管理系統，取代了以往各個 `Window` 獨立硬塞預設樣式表的邏輯。
- 在 `ImTKTheme` 的 `ColorFamily` 擴充了 `hoverBackground` 與 `selectedBackground`，實現更細緻的互動狀態著色。
- `ImTK.Sample` 中新增 `ThemeMenu`，支援透過上方選單列切換深色/淺色主題。
- 將 `DefaultDark` 更新為 VS Code 的 Modern Dark 風格。
- 將 `DefaultLight` 更新為 Unity Editor 的 Light 風格。
- **架構重構**：將 `ImTKTheme` 配置改為在 `Panel.OnLogicUpdate` 時統一透過 `ApplyToImGui()` 深度映射至 `ImGui.GetStyle()` 全域參數中。這顯著降低了 `VisualElement` 處理預設樣式的效能負擔，並解決了 Tab、TitleBg、MenuBar 等原生 UI 的漏色問題。
- **效能優化**：清除 `DefaultStyles.cs` 中的冗餘組態與過時的相容 Token，符合 YAGNI 原則並完全依賴於 ImGui 底層的高效繼承。
- 移除了 `ImTKTheme` 中的 `borderColor` 與 `checkMarkColor` 屬性，全面改用更一致的 `ColorFamily` 語義。
- 將 `VisualElement.Render()` 由 `internal` 修改為 `public`，確立其為驅動視覺樹渲染的公開鎖定入口，並徹底移除了內部為了規避權限而使用的 C# Reflection 渲染呼叫，大幅提升效能。
- 重構了現有 Database 相關測試，將其整合至新的 `IHeadlessTest` 框架中。
- 於 `AGENT.md` 補齊 `ImTKModule` 必須擁有「單一、無參數、非公開」建構函式的限制規範。
- 修正 `EventDispatcher` 的事件氣泡 (Bubbling) 路徑。事件現在嚴格沿著物理樹 (`hierarchy.parent`) 向上傳遞，修復了封裝內部元件 (Shadow DOM) 時事件斷層的問題。
- 將原有的 `Architecture/` 目錄下的架構設計文件重組並移至對應的子系統資料夾。
- 將 `AGENT.md` 移至專案根目錄，並擴充了 AI 代理的條列式探勘與開發指引。
- 修改 `DevelopmentWrapUp.md`，新增了「回歸測試與維護檢查 (Regression Testing & Maintenance)」SOP。
- 重構了 VisualElement 樣式系統，移除全局 `StyleMappingRegistry` 並引入 `VisualElement<TStyle>` 泛型，讓 `Button` 等特異元件能自行實作 ImGui 的 `PushToImGui` 與屬性擴充 (如 `Window.StyleKey.TitleBg`)。
- 將 `VisualElementStyle` 和 `ImTKStyleKey` 改為巢狀結構 (`VisualElement.Style` 與 `VisualElement.StyleKey`)，讓命名空間與架構更具層次性與擴展性。
- 將 `HoverColor`、`ActiveColor` 與 `CheckMarkColor` 從基底 `VisualElement.StyleKey` 中移除，改由各特化元件 (`Button`, `TextField`, `CheckBox`, `MenuItem`) 的 `<T>.StyleKey` 獨立實作，避免語意混淆。
- 擴充基底 `VisualElement.StyleKey`，新增 `SelectionColor` 與 `DisabledTextColor` 的 ImGui 映射支援。
- 更新 `TextField`、`CheckBox`、`MenuView` 及 `MenuItem` 的樣式映射實作，將背景色精準導向至 `FrameBg`、`PopupBg/MenuBarBg` 與 `Header`，完成全域與局部組件語義化覆寫的最終拼圖。

### Fixed (修復)
- 修復 ImGui `SetCursorScreenPos()` 無法擴展父視窗邊界的問題 (Assertion failed)。透過預先放置 `ImGui.Dummy` 分配複合 Drawers (`Vector2`, `Rect` 等) 所需的版面空間，確保安全計算並完美支援自訂 Absolute 排版。
- 引入了 `RenderingContext` 來追蹤與延遲管理依賴 ImGui 視窗狀態的操作，修復了在 `Window` 開啟時直接呼叫 `SetWindowFontScale` 導致 ImGui 出現 Debug 斷言視窗的問題。
- 修復了 `VisualElement` 在子元件單獨設定字型大小時，因無法取得父元件字型而錯誤退回預設字型的繼承失效問題（現透過 `RenderingContext.CurrentFontFamilyHash` 解決）。
- 修復了 `Panel` 在 `OnGuiRender` 期間直接註冊視窗 (`Window.Open`) 導致的 `[VisualElementHierarchy] Cannot modify VisualElement hierarchy during GuiRender state` 崩潰問題。現在 `RegisterWindow` 會把視窗推入 `s_windowsToAdd` 佇列，延遲至安全的 `OnLogicUpdate` 階段加入。
- 修復了 Theme 初始化過早導致 `NullReferenceException` 的崩潰問題（將 ImGui 指標的映射延後並由 `isGlobalThemeDirty` 旗標保護）。
- 修復了 `SampleOverviewModule` 的左右面板吃不到樣式的問題，將其封裝進 `OverviewHostElement` 並納入正規的渲染管線。
- 修正 Window 右上角關閉按鈕點擊後未能觸發 `Close()` 的問題（隔離 ImGui 內部狀態修改與元件本身的 `m_isOpen` 狀態）。
- 將 `AssetManager.GetOrCreateAsset<T>` 異常捕捉範圍放寬至泛型 `Exception`，防止 IO 異常導致程式崩潰。
- 優化 `ResolvedStyle` 樣式計算的內部結構，改為使用 `List<StyleProperty>`，消除字典查表與清理時的 GC 記憶體配置壓力。
- 擴充 `Button.Style`，實作 `Width` 與 `Height` 參數自訂功能。
- 優化 `EventDispatcher` 階層髒標記處理邏輯，透過雙緩衝區 (Double-Buffering) 徹底消除每幀陣列複製造成的 GC 配置。
- 修復了無頭測試中全域 `EventDispatcher` 駐列污染的架構漏洞，實作 `ClearQueue` 強制在每次測試前後隔離環境。
- 修正 `MainMenuModule` 在繪製 MenuBar 容器時，由於 ImGui 預設 `WindowMinSize` 大於我們給定的高度，導致產生透明 Hit-box 擋住下方 Docking 標題列滑鼠拖曳事件的問題（透過推送 `ImGuiStyleVar.WindowMinSize` 至 `0,0` 解決）。
- 修正 `TestRunnerModule` 內的 `TestReportWindow` 統計狀態異常問題（修正 Pending 顯示判斷與顏色標示）。
- 修正 `EventBubbleTest` 測試失敗問題：將原有的 `ImGui.Button` 替換為 `ImTK.UI.Button` 以解決在 `GuiRender` 階段因直接修改視覺樹而觸發的生命週期保護機制 (CheckSafeState)。
