# ImTK 核心生命週期架構藍圖 (Core Lifecycle Architecture)

## 1. 摘要與背景 (Abstract & Background)

ImTK 的核心價值在於提供一個基於 C# 與 ImGui 的 Retained Mode UI 框架，旨在快速開發小型應用工具。
在先前的架構中，`ImTKModule` 使用全域反射掃描進行自動註冊，並提供了 `OnLoad`, `Update`, `Render`, `OnClose` 等基本的生命週期鉤子。

然而，隨著框架擴展（例如準備引入資源管理系統 `ImTKDatabase`），舊有架構暴露了以下痛點：
*   **缺乏初始化順序保障**：全域反射抓取的順序是未知的，導致模組間的依賴注入（Dependency Injection）充滿風險（例如 ModuleA 依賴尚未初始化的 ModuleB）。
*   **渲染與邏輯邊界模糊**：ImGui 要求嚴格的繪圖指令順序（必須在 `NewFrame` 之後）。舊架構未嚴格分離純資料邏輯與 UI 渲染。
*   **缺乏動態實例支援**：`ImTKModule` 是強制單例，無法滿足執行期（Runtime）動態建立與銷毀臨時邏輯物件（如一個短暫的下載任務控制器）的需求。

為此，ImTK 將採用全新的 **雙層架構 (Dual-Tier Architecture)** 與 **語意化生命週期 (Semantic Lifecycle)**，以「階段分離 (Phase Separation)」取代硬性排序 (Hard Priority)，提供安全、極簡且具備高度擴展性的底層基礎。

---

## 2. 為什麼捨棄 Priority 排序？ (Why Phase Separation over Priority)

在許多框架中，為了解決模組載入順序問題，會要求開發者設定優先級（例如：`ModuleA.Priority = 100`）。但這會導致「Priority Hell（優先級地獄）」，開發者必須不斷猜測與調整魔術數字以避免衝突。

ImTK 借鏡了 Unity 的 `Awake` / `Start` 哲學，採用**階段分離 (Phase Separation)**。
我們不規定模組之間的執行先後順序，而是將**「初始化行為」切割為兩個嚴格的階段**。只要遵循「內部事務在第一階段處理，外部連結在第二階段處理」，依賴順序問題就不攻自破。這大幅降低了開發者的心智負擔。

---

## 3. 雙層架構設計 (The Dual-Tier Architecture)

系統將分為兩個清晰的層級，各自承擔不同的職責與註冊方式：

### 3.1 第一層：`ImTKModule` (系統級單例模組)
*   **定位**：框架或大型子系統的骨幹（例如：`ImTKDatabase` 資源系統、`WindowManager`、`InputManager`）。
*   **註冊方式**：**絕對單例 (Absolute Singleton)**。
    *   僅透過應用程式啟動時的反射全域掃描自動註冊。
    *   **不接受**開發者在執行期手動多實例註冊。
*   **生命週期特徵**：擁有最完整、涵蓋初始化階段（含依賴注入與 GPU 準備）的健全生命週期。

### 3.2 第二層：`ImTKObject` (動態邏輯物件)
*   **定位**：臨時性、半持久性的業務邏輯或 UI 控制器（例如：進度追蹤器、動態對話框控制器）。
*   **註冊方式**：**動態多實例 (Dynamic Multi-instance)**。
    *   完全由開發者在執行期 (Runtime 主迴圈) 手動 `new` 出來。
    *   透過 `ImTKApplication.RegisterObject()` 與 `UnregisterObject()` 明確管理。
*   **生命週期特徵**：極簡化。因為它們建立於主迴圈運行期間，故省略早期的啟動階段，僅參與運行時的 Update/Render 迴圈。

*(註：`VisualElement` 作為純粹的視圖節點，不屬於 `ImTKObject`，其渲染驅動由其父節點或 `Window` 管理，以維護 ImGui 巢狀樹的安全。此部分細節將於後續的 VisualTree 文件詳述。)*

---

## 4. 語意化生命週期 (Semantic Lifecycle)

為解決模組間未知的載入順序問題，我們引入「階段式初始化」概念，並使用自我描述性極強的命名。

### 4.1 啟動與初始化階段 (Initialization Phase)
*此階段僅 `ImTKModule` 參與，由框架在主迴圈開始前嚴格依序驅動。*

1.  **`OnInitializeSelf()`**
    *   **職責**：模組自身的基礎準備。
    *   **允許事項**：分配記憶體、讀取屬於該模組的私有設定檔、實例化內部的私有變數。
    *   **強制禁止**：**絕對不可**在此階段呼叫或依賴其他 `ImTKModule`，因為其他模組可能尚未執行 `OnInitializeSelf`。
    *   *範例*：`m_cacheDict = new Dictionary<string, object>();`
2.  **`OnInitializeDependencies()`**
    *   **職責**：外部依賴注入與跨模組連結。
    *   **狀態保證**：框架保證此時所有模組的 `OnInitializeSelf` 皆已完成。
    *   **允許事項**：可安全地獲取其他模組實例、向其他系統註冊回呼。
    *   *範例*：`ImTKModule.Get<ImTKDatabase>().RegisterLoader(new MyCustomLoader());`
3.  **`OnGraphicsSetup()`**
    *   **職責**：圖形與渲染環境的最後準備。
    *   **狀態保證**：圖形 Context (如 OpenGL/Silk.NET) 已就緒，且尚未繪製第一幀。這是載入自訂字型、註冊初始 GPU Texture 的唯一安全時機。

### 4.2 運行時迴圈階段 (Runtime Loop Phase)
*此階段 `ImTKModule` 與 `ImTKObject` 皆參與，每幀循環呼叫。*

4.  **`OnLogicUpdate(double deltaTime)`**
    *   **職責**：純資料與業務邏輯更新。
    *   **強制規範**：執行於 `ImGui.NewFrame()` 之前，**嚴禁**包含任何 ImGui 渲染指令。
5.  **`OnGuiRender()`**
    *   **職責**：介面建構與渲染指令生成。
    *   **狀態保證**：已被包裝在 `ImGui.NewFrame()` 與 `ImGui.Render()` 之間。專用於建構 UI 樹與呼叫 ImGui API。
6.  **`OnLateUpdate(double deltaTime)`**
    *   **職責**：幀末延遲處理。
    *   **狀態保證**：所有模組的 UI 皆已建構完畢。
    *   **應用場景**：清理被標記為銷毀的物件、執行統一存檔 (`SaveAssets`)、以及處理全域集合的增刪（解決 `Collection modified` 例外）。

### 4.3 停用與拆卸階段 (Teardown Phase)

7.  **`OnDisable()`**
    *   **職責**：當模組/物件的 `IsActive` 屬性被設為 `false`，或動態物件被卸載時觸發。用於暫停事件訂閱或釋放暫時資源。
8.  **`OnClose()`** (僅 `ImTKModule`)
    *   **職責**：應用程式即將關閉時觸發。
    *   **強制規範**：必須在此徹底清洗所有非託管資源（如呼叫 OpenGL 刪除 Texture），因為隨後圖形 Context 將被銷毀。

---

## 5. 集合安全與動態註冊 (Collection Safety & Pending Queue)

針對 `ImTKObject` 的動態註冊特性，如果在 `OnLogicUpdate` 迴圈迭代中直接修改全域清單，會引發 `Collection was modified` 崩潰。因此必須實作 **待處理佇列 (Pending Queue)** 機制：

1.  **註冊請求**：當開發者呼叫 `ImTKApplication.RegisterObject(obj)` 時，物件不會立刻進入主迴圈清單，而是被加入 `m_pendingAdd` 佇列。
2.  **註銷請求**：呼叫 `UnregisterObject(obj)` 時，物件被加入 `m_pendingRemove` 佇列，同時框架可提早呼叫其 `OnDisable()` 暫停其作用。
3.  **安全執行點**：在每幀的 `OnLateUpdate` 階段末尾（所有遍歷皆已結束），框架統一將 `m_pendingAdd` 內的物件加入主清單，並將 `m_pendingRemove` 內的物件從主清單移除。

---

## 6. 架構邊界：VisualElement 的定位

`VisualElement` 是 ImTK 負責呈現 UI 與處理佈局的基石，但 **它不繼承自 `ImTKObject`**。

*   **原因 1：階層衝突**。`VisualElement` 依賴父子樹狀結構（Logical & Physical Tree）來維護 ImGui 的 `BeginChild/EndChild` 巢狀邏輯。如果將它攤平放入全域的 `ImTKObject` 清單中，會破壞渲染順序。
*   **原因 2：效能**。全域迴圈不應承受成百上千個微小 UI 節點的遍歷負擔。
*   **結論**：`VisualElement` 的生命週期必須由其父節點或頂層容器（如繼承自 `ImTKModule` 的 `Window`）驅動，作為全域生命週期進入 UI 樹狀結構的單一橋樑。