# AI Agent 開發指南與專案導覽

本專案是一個基於 C++11 開發的 VEX Robotics (V5) 競賽機器人框架。
本文件 (`AGENT.md`) 為 AI Agent 參與開發與維護時的**最高指導原則**與**專案導覽**。

---

## 1. 核心開發規範

在修改或重構任何程式碼之前，你**必須**遵守以下開發規範：

### 1.1 任務追蹤與文檔維護原則
* **優先查看 `TODO.md`**：當收到新任務或指令不明確時，**必須**優先查看 `./TODO.md`，特別是「🟢 進行中 (In Progress)」區塊，以確認當前的開發焦點。
* **手動確認任務進度**：當完成了一項子任務或準備提交時，**絕對禁止自動將任務標記為完成 (`- [x]`)**。你必須利用 `request_user_input` 詢問使用者是否同意標記完成並移動任務區塊。
* **閱讀規格為先**：在修改任何模組的程式碼之前，必須先尋找並閱讀該模組相關的 Markdown 規格文件。
* **確認需求邊界**：如果使用者的要求涉及修改規格書「未提及」的部分，或是超出當前規格的範疇，必須先利用 `request_user_input` 與使用者確認。
* **純粹的事實紀錄**：更新規格文件時，只記錄事實與現狀（Established facts）。絕對不要在文件中加入修改日誌、更新歷史或 changelog。

### 1.2 生命週期與 Application 註冊架構 (NEW)
本專案採用**基於 Event-Bus/Delegate 的動態生命週期註冊架構**。所有的程式進入點皆由 `gcvex::Application` 統一管理，支援靜態函數或動態物件實例的註冊。

* **`gcvex::Application` (生命週期總管)**：接管 `main.cpp`，負責綁定 `vex::competition` 並驅動主迴圈。它提供以下靈活的註冊 API (會回傳唯一 ID 用於卸載)：
  * `registerInit(cb)`：於 `pre_auton` 階段觸發。**禁止**存取外部動態變數以防 Static Initialization Order Fiasco。
  * `registerStart(cb)`：於每次進入 `autonomous` 或 `usercontrol` 時觸發，適合進行依賴注入。
  * `registerLoop(cb, interval)`：於主迴圈中依據 `interval` 定期觸發 (`cb(time, dt)`)。
  * `registerExit(cb)`：於進入 Disable 或退出模式時觸發，負責重置與關閉。
  * `registerSubSystem(...)`：捷徑，一次性註冊上述四個階段。

* **單執行緒與非阻塞原則**：所有的 Callback 都被設計為在單一主執行緒迴圈中被動呼叫。**絕對禁止在 `loop()` 中呼叫 `vex::wait()` 或是使用任何形式的無窮迴圈阻塞主執行緒。**

* **`Application::auto_op` 與 `Application::teleop_op`**：
  這是取代複雜 `OpMode` 繼承體系的變數。使用者可將競技階段的主邏輯包裝為 `std::function<void()>` 並指派給這兩個變數，`Application` 會在主迴圈 `loop()` 派發完畢後呼叫它們。

### 1.3 C++ 與架構設計原則
* **明確的物理單位標示**：變數名稱必須明確附帶單位字尾（例如 `_deg`, `_rad`, `_ms`, `_rpm`）。內部計算統一使用弧度，外部 API 使用角度。
* **無鎖單執行緒設計**：狀態機架構 (`StateMachine`) 與生命週期架構 (`Application`) 皆被設計為無鎖單執行緒。盡可能避免自己建立背景 `vex::task` 或 Mutex。
* **依賴注入與生命週期管理**：利用 `registerStart` 進行依賴注入。若將硬體物件注入控制器 (如 `Chassis`)，應使用參考 (`&`) 或原始指標 (`*`)。

### 1.4 狀態機 (`StateMachine`) 執行語意
* `StateMachine` 是 `auto_op` (Autonomous) 中的「一項工具」，用於編寫依序執行的複雜腳本，而非控制整個機器人的唯一主控台。
* `done()` 代表狀態正常完成，並觸發後續狀態；`interrupt()` 代表強制中斷，不觸發後續。

### 1.5 競賽特定邏輯
* 與特定賽季遊戲規則相關的機器人邏輯（例如 Intake、Skill Auto）**必須**放置於 `./teamcode/include/` 和 `./teamcode/src/` 目錄中。請利用 `Application::register...` 模式或 `ISubSystem` 介面來將它們掛載到核心。

---

## 2. 專案子模組導覽

目前專案的核心邏輯被劃分為以下幾個主要的模組：

### 2.1 生命週期與系統框架 (Framework)
* **相關路徑**：`include/framework/Application.h`, `src/framework/Application.cpp`
* **核心邏輯**：基於 Delegate/Event-bus 的生命週期與註冊器，接管整個 VEX 的競爭模式。

### 2.2 里程計與定位 (Odometry / Locator)
* **強烈建議**封裝為動態註冊於 `Application` 的子系統，讓其 `loop()` 在背景穩定更新座標，並供全局讀取。

### 2.3 底盤與傳動 (Chassis & Drivetrain)
* **IDrivetrain**：維持單純的硬體抽象介面 (不需註冊)。
* **Chassis**：建議註冊於 `Application` 中，透過其 `loop()` 提供背景的 PID 保持、路徑追蹤等功能，無論在 Auto 還是 Teleop 都能自動運作。

### 2.4 自動程序與狀態機 (Auto Flow & State Machine)
* **相關路徑**：`include/state_machine/`
* **核心邏輯**：基於 Fluent API 的平行狀態機架構（如 `add()`, `then()`, `branch()`）。用於撰寫非阻塞的自動階段任務腳本。

### 2.5 偵錯與儀表板系統 (Debug & Dashboard)
* **相關路徑**：`include/framework/Debug.h`, `include/framework/Dashboard.h`, `include/gc_config.h`, `DEBUG.md`, `DASHBOARD.md`
* **核心邏輯**：
  * 編譯期開關：由 `gc_config.h` 的 `DEVELOPMENT_MODE` 巨集統一控制，可達到正式競賽零開銷。
  * `Debug` 模組：採用 `LogProvider` 介面，統一管理多達 50 筆的 Key-Value 日誌，支援不同設備（Brain、Controller）各自定義繪製限制與頻率。統一使用 `gcvex::Debug::log` API。
  * `Dashboard` 子系統：獨立模組，負責管理 `SerialLogger`，透過 `gcvex::Dashboard::send(cmd)` 對接外部 C# / Web Socket 通訊。
