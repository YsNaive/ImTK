# ImTK 全域架構與真實生命週期 (Global Architecture & Real Lifecycle Map)

## 1. 摘要 (Abstract)

要精準地開發與除錯 ImTK 應用程式，僅了解 `ImTKModule` 的內部運作是不夠的。我們必須從「作業系統啟動執行檔」到「程式完全退出」的宏觀視角，理解整個應用程式的**全域真實生命週期 (Global Real Lifecycle)**。

這份藍圖明確界定了基礎設施（如日誌系統 `ImTKLog`）、核心框架 (`ImTK`)、圖形橋接層 (`ImTK.Silk` 等) 在整個程式生命週期中的介入時機與執行順序。

---

## 2. 全域真實生命週期調用表 (Global Real Lifecycle Map)

### Phase 1: 預備與基礎設施啟動 (Pre-Bootstrap)
*在此階段，沒有圖形介面，也沒有複雜的反射與依賴。系統必須建立最基礎的防護網與監控機制。*

1.  **`Main()` 進入點**：作業系統啟動應用程式。
2.  **`ImTKLog.Initialize()`**：
    *   **職責**：配置最底層的日誌系統（Console Sink, File Sink）。
    *   **狀態**：從這一刻起，所有的例外與錯誤皆可被安全記錄。這確保了後續哪怕在反射或 OpenGL 初始化時崩潰，開發者也能留下追蹤線索。
3.  **`ImTKApplication.Configure()`**：讀取啟動參數或全域設定檔（如決定視窗大小、預設根目錄）。

### Phase 2: 系統級組件掃描與建立 (System Bootstrap)
*由 `ImTK` 核心負責，建立所有單例的骨幹系統。*

4.  **反射掃描與實例化 (`ScanAndInstantiateAll`)**：
    *   **狀態**：尋找所有繼承自 `ImTKModule` 的類別，透過無參數建構子建立實例。此時尚未呼叫任何自訂初始化邏輯。
5.  **`OnInitializeSelf()`**：
    *   所有 `ImTKModule` 依序執行。僅限處理內部記憶體配置與私有設定。
6.  **`OnInitializeDependencies()`**：
    *   所有 `ImTKModule` 依序執行。進行模組間的依賴注入（如 `ImTKDatabase` 註冊自訂載入器）。

### Phase 3: 圖形環境初始化 (Graphics Context Bootstrap)
*這層職責交由橋接器 (如 `ImTK.Silk`) 驅動，牽涉到與作業系統圖形 API 的溝通。*

7.  **申請系統視窗**：呼叫如 `Silk.NET.Window.Create()`，建立作業系統原生視窗並獲取 OpenGL Context。
8.  **ImGui 啟動**：呼叫 `ImGui.CreateContext()` 準備底層資料結構。
9.  **`OnGraphicsSetup()`**：
    *   **關鍵時機**：這是 ImTK 框架首次可以安全接觸 GPU 的時間點。模組可在此載入自訂字型、註冊預設 Texture、編譯 Shader。
10. **字型推送 (Font Build)**：由橋接層呼叫 `ImGui.GetIO().Fonts.Build()`，將字型資料上傳至 GPU。

### Phase 4: 運行時主迴圈 (The Runtime Loop)
*程式佔用絕大部分時間的循環，通常以 60 FPS 以上的頻率執行。*

11. **新幀啟動 (`NewFrame`)**：橋接層呼叫 `ImGui.NewFrame()` 宣告新的一幀開始。
12. **`OnLogicUpdate(dt)`**：
    *   依序呼叫所有 `ImTKModule`，接著呼叫所有動態註冊的 `ImTKObject`。
    *   **規範**：專注於純邏輯與資料更新，嚴禁產生任何 ImGui 繪製指令。
13. **`OnGuiRender()`**：
    *   生成 ImGui 繪製指令。若有內建的 UI Console 模組，此時會向 `ImTKLog` 拉取最新日誌並渲染為視窗。
14. **繪製與交換 (`Render` & `SwapBuffers`)**：
    *   由橋接層收集 DrawData 並送往 GPU，完成畫面呈現。
15. **`OnLateUpdate(dt)`**：
    *   處理幀末的延遲操作（如處理 `ImTKObject` 的註冊/註銷佇列、統一執行存檔 `SaveAssets`），確保集合操作的安全性。

### Phase 5: 關閉與清洗 (Teardown & Cleanup)
*當使用者點擊關閉視窗，或程式發出結束訊號時觸發。*

16. **`OnDisable()`**：所有 `ImTKModule` 與 `ImTKObject` 停止作用，解除事件訂閱。
17. **`OnClose()`**：
    *   所有 `ImTKModule` 執行最終清洗。
    *   **關鍵時機**：必須在此時呼叫 OpenGL 指令釋放自訂的 Texture 或 Buffer，因為下一步圖形 Context 將不復存在。
18. **圖形環境銷毀**：呼叫 `ImGui.DestroyContext()` 並關閉系統視窗。
19. **`ImTKLog.FlushAndClose()`**：將記憶體緩衝中最後的日誌寫入磁碟，程式優雅退出。

---

## 3. 架構邊界與除錯價值 (Boundaries & Debugging Value)

透過此表，我們可以明確劃分以下職責邊界，並提升除錯效率：

*   **橋接層獨立性**：`ImTK.Silk` 僅負責 Phase 3（準備環境）、Phase 4 的 11 & 14（驅動迴圈）、以及 Phase 5 的 18。它不涉及任何具體的業務邏輯。若未來想更換為 WebGL 或 DirectX 橋接，只需替換這些步驟即可。
*   **除錯定位器 (Debug Locator)**：若日誌顯示「字型載入失敗：OpenGL Context 尚未建立」，開發者便可立即對照此表，發現自己錯誤地將載入字型的邏輯寫在了 Phase 2 的 `OnInitializeDependencies` 中，而正確的時機應為 Phase 3 的 `OnGraphicsSetup`。