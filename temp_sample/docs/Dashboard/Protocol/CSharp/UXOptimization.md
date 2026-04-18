# C# 接收端 UX 優化與快取機制 (UX Optimization)

在開發或比賽過程中，VEX 機器人可能會頻繁重新啟動，從而觸發 `[0x00] Reset` 指令。
如果只是單純地清空並銷毀所有 UI 視窗，使用者辛苦排版的 ImGui 視窗位置與大小將會遺失。為了提供平滑的使用體驗，C# 接收端實作了以下的快取與懶惰刪除 (Lazy Delete) 機制。

## 1. 統一快取路徑 (Unified Cache Path)

所有的 C# 端狀態快取檔案，包含本系統的 Group 狀態與 ImTK 的視窗設定檔 (`window_state.json`)，皆統一路徑至系統標準的 Local Application Data 目錄。

* **路徑**：`%LOCALAPPDATA%/gcvex_dashboard/`
* **快捷功能**：透過 ImTK 頂端選單的 `[MainMenu("設定/開啟快取資料夾")]`，可一鍵開啟該目錄，方便開發者手動清除或檢查快取。

## 2. 視窗的平滑重置與懶惰刪除 (Lazy Delete Mechanism)

`Registry` 使用兩個 `HashSet<string>` 來管理 Group 視窗的生命週期：
* `used_group`：歷史上曾經出現過的 Group。此清單會被序列化存入快取檔案 (`used_groups.json`)。
* `using_group`：在本次連線週期內（即兩次 Reset 之間）實際有收到 `CreateEntity` 的活躍 Group。

### 生命週期與流程：

1. **啟動時 (Startup)**：
   從快取讀取 `used_group`。針對清單中的每個 Group 名稱，系統會「預先建立」空的 `DashEntityWindow`。這能確保 ImGui 的 Window ID 不變，從而讓 ImTK 成功載入歷史視窗的位置與尺寸配置。
2. **新增實體 (CreateEntity)**：
   當收到新建實體的指令時，將其加入對應的 Group 視窗中，並將該 Group 名稱加入 `using_group`。如果這是一個全新的 Group (不在 `used_group` 中)，則同時加入 `used_group` 並觸發快取存檔。
3. **重置時 (Reset)**：
   * 當收到 `[0x00] Reset` 時，**不直接關閉視窗**。
   * 遍歷所有的 `DashEntityWindow`，呼叫 `Clear()` 清除視窗內所有的 Entity 控制項 (UI 元素)。
   * **比對與清除 (Lazy Delete)**：檢查 `used_group` 中的每一個 Group。如果它不在 `using_group` 中，代表在上一輪連線中，這個 Group 已經被 VEX 程式徹底移除。這時才呼叫視窗的 `Close()` 將其銷毀，並從 `used_group` 中剔除。
   * 最後，將當前的 `using_group` 覆寫到 `used_group`，並清空 `using_group`，準備迎接新一輪的變數宣告。並更新快取存檔。

透過這個機制，就算機器人重開機，使用者在畫面上排列好的 Dashboard 視窗也不會閃爍或跑位，達到無縫重連的完美體驗。