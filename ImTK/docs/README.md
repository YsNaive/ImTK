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

---

# ImTK 專案架構總覽 (Project Overview)

本段落摘要了 ImTK 框架當前已實作的核心功能、子系統組件，以及關鍵的技術細節。

## 1. 核心生命週期系統 (Core Lifecycle)

*   **雙層架構設計**：
    *   **`ImTKApplication`**：全域的應用程式進入點與狀態機引擎。
    *   **`ImTKModule`**：系統級別的單例模組（如 `Panel`、`ImTKFontManager`），嚴格透過反射在啟動時實例化。
    *   **`ImTKObject`**：動態建立的邏輯執行實例（類似 Unity 的 GameObject），生命週期受框架自動管控。
*   **嚴謹的狀態機 (ApplicationState)**：
    *   強制規定各個階段的執行順序：`Uninitialized` -> `InitializeSelf` -> `InitializeDependencies` -> `Enable` -> `AwaitingGraphicsSetup` -> `GraphicsSetup` -> `Idle` -> `LogicUpdate` -> `GuiRender` -> `GizmoRender` -> `LateUpdate`。
    *   內建 `EnforceFrameOrder` 防禦機制，防止邏輯重入 (Re-entrancy) 與順序違規。
*   **時間與驅動**：
    *   透過 `ImTKSilk.Run()` 整合 Silk.NET，驅動 ImGui，支援 Viewports (多視窗佈局)。
    *   全域 `Time` 管理系統提供精準的 DeltaTime 與 StartupTime。

## 2. 佈局與渲染引擎 (UI Layout & Render Engine)

*   **四階段生命週期**：
    將 UI 渲染拆分為 `Measure` (測量) -> `Arrange` (佈局) -> `Style Compute` (樣式計算) -> `RenderFlat` (扁平化渲染)。
*   **1-Pass 輕量級 Flexbox 排版**：
    *   完整支援 `FlexDirection` (Row/Column)、`FlexGrow`、`AlignItems`。
    *   支援精確的間距 (`gap`/`ItemSpacing`) 自動扣除與對齊。
    *   支援 `Width`, `Height`, `MinMax` 的嚴格尺寸夾擠 (Clamp)。
*   **雙層視覺樹 (VisualTree)**：
    *   區分「邏輯樹 (`logicHierarchy`)」與「物理樹 (`hierarchy`)」，自動同步與脫離，消滅幽靈節點問題。
    *   基於 `IRenderRoot` 與 `RenderListCache` 的獨立渲染清單，確保動態顯示隱藏 (`display: none`) 時享有 O(1) 的超高過濾效能，無需每幀重建清單。
    *   內建安全防重入的 `RenderEngine.RenderFlat` 迴圈機制 (`Stack<List<RenderOp>>`)。
*   **高階意圖與樣式系統 (Style & Theme)**：
    *   **零裝箱 16-Bytes `StyleProperty`**：使用強型別取代傳統字典，達成 Zero GC Allocation。
    *   實作 Inline > Local > Global 的 CSS 層疊優先級。
    *   支援 `HighLevelToken`（高階意圖，例如 `ColorFamily`）自動繼承與解析。
    *   提供極為流暢的 DX 語法糖 (如 `style.padding = 20;`, `style.textColor = "#FFFF00";`)。
    *   **全域 / 局部 Theme 切換**：支援即時熱切換淺色/深色主題，並能在單一 Element 上注入局部獨立的 `VisualElement.theme`。

## 3. UI 基礎設施與封裝 (UI Infrastructure & Elements)

*   **基礎控制項 (Basic Elements)**：
    *   `TextElement` (支援自動折行), `Button`, `TextField` (動態適應多行高度), `CheckBox`。
    *   `IntField`, `FloatField`，消除冗餘按鈕的乾淨 ImGui 控制項。
*   **抽屜與綁定系統 (Drawer System)**：
    *   類似 Unity Inspector 的 `FieldDrawer<T>`，支援雙向綁定與攔截 (`SetValueWithoutNotify`)。
    *   內建 `Hold-and-Drag` 拖曳改變數值 (SliderInt/Float)。
    *   **開放式泛型註冊表**：透過 `[CustomFieldDrawer]` 自動註冊，並能遞迴尋找父類別或介面。
    *   實作 `ObjectDrawer` (基於 `FoldoutDrawer`) 支援巢狀物件的反射生成與展開。
*   **視窗與選單 (Windows & Menus)**：
    *   支援無接縫的自動工作區持久化 (Workspace Restoration) 與啟動復原。
    *   `Window` 透過 `Panel` 統一集中管控，支援生命週期防呆（如尚未繪製前不呼叫 Focus）。
    *   以 `[MainMenu("Path/To/Item")]` 屬性驅動的全域自動選單系統。

## 4. 資料與資源系統 (Database & Assets)

*   **唯一基底 `ImTKAsset`**：
    系統中萬物皆為純粹資料載體 (POCO Container)。
*   **隔離雙軌資源存取**：
    *   `Resource.Load<T>()`：全域唯讀設定檔/資源，強制鎖定 `MarkDirty()` 防竄改。
    *   `ImTKDatabase.Load<T>()`：本地可讀寫快取與資料庫。
*   **Importer / Exporter 註冊表機制**：
    *   支援型別映射，將 I/O 徹底從資料邏輯中解耦。
    *   內建 `JsonAsset<T>`，負責透明且高效的序列化轉換。

## 5. 事件與派發器 (Event & Dispatcher)

*   **`ImTKEventBus` 雙軌事件系統**：
    *   提供全域的跨模組非同步廣播。
    *   模組生命週期掛鉤：於 `InternalOnEnable` 自動註冊、於 `InternalOnDisable` 自動卸載，徹底防範 Memory Leak。
*   **`ImTKDispatcher` 主執行緒調度**：
    *   支援多執行緒調用，將耗時回呼或 UI 操作延遲至安全的 `LateUpdate` 階段執行。
*   **UI 階層事件 (Hierarchy Events)**：
    *   `UIEventBase` 具備氣泡 (Bubbling) 上傳機制。
    *   延遲派發 `HierarchyChangedEvent` 避免走訪時修改集合導致的例外。

## 6. 日誌系統 (Logging)

*   **`ImTKLog` 極早期初始化**：
    *   於 Phase 0 反射掛載所有的 `ILogSink`。
    *   內建 `ConcurrentQueue` 作為「破曉緩衝區」，確保任何在 Sink 註冊前發出的日誌都能完美回放。
*   **Zero-Allocation 效能設計**：
    *   利用 C# 的 `InterpolatedStringHandler` (`xxxIf`) 語法糖，當等級不足時直接跳過字串插值運算，節省 CPU 與記憶體。

## 7. 範例與測試框架 (Testing & Samples)

*   **自動化測試 (`ImTK.Test`)**：
    *   區分無頭測試 (`IHeadlessTest`) 與整合測試 (`IIntegrationTest`)。
    *   內建 `TestRunnerModule` 視覺化報表與即時主題切換功能。
*   **範例場景架構 (`ImTK.Sample`)**：
    *   基於 `ISampleScenario` 介面的屬性驅動註冊，動態生成左側分類清單與右側詳細面板。
