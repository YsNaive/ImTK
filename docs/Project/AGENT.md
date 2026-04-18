# AI Agent 開發指南與專案導覽

本專案是一個基於 C# / .NET 開發的 ImTK (ImGui Wrapper) 框架。
本文件 (`AGENT.md`) 為 AI Agent 參與開發與維護時的**最高指導原則**與**專案導覽**。

---

## 1. 核心開發規範

在修改或重構任何程式碼之前，你**必須**遵守以下開發規範：

### 1.1 任務追蹤與文檔維護原則
* **優先查看 `TODO.md`**：當收到新任務或指令不明確時，**必須**優先查看 `TODO.md`，特別是「進行中」區塊，以確認當前的開發焦點。
* **閱讀規格為先**：在修改任何模組的程式碼之前，必須先尋找並閱讀該模組相關的 Markdown 規格文件。
* **確認需求邊界**：如果使用者的要求涉及修改規格書「未提及」的部分，或是超出當前規格的範疇，必須先與使用者確認。
* **純粹的事實紀錄**：更新規格文件時，只記錄事實與當下的架構現狀。絕對不要在 `README.md` 中加入修改日誌、更新歷史或 changelog。

### 1.2 ImGui.NET 指標記憶體安全規範
因為 ImTK 封裝了 C++ 底層的 ImGui，在處理需要指標 (Pointers) 的操作時，**必須絕對避免因 C# 垃圾回收 (GC) 導致的懸空指標問題**。
* **規範**：當 AI 代理或開發者在呼叫 ImGui.NET 中任何需要寫入 `byte*` (字串指標) 的底層 API 時，**嚴禁使用 `fixed` 語句直接綁定 C# 字串陣列**。
* **解法**：**必須**使用 `Marshal.StringToCoTaskMemUTF8(string)` 將字串分配到不受 GC 管理的記憶體區塊中，然後強轉為指標傳給 ImGui。並有責任在物件生命週期結束（如 `OnClose`）時，呼叫 `Marshal.FreeCoTaskMem` 釋放該指標以防 Memory Leak。

### 1.3 C# 開發與 API 封裝習慣
* **Property Over Field**：影響功能狀態的變數（如 `enableXXX`、`isResizable`），必須實作為 C# Property，以便未來攔截 getter/setter 或綁定事件。
* **ImGui Flags 封裝**：不要讓最終開發者直接操作 `ImGuiWindowFlags` 等底層位元遮罩。應將其封裝為直覺的布林屬性 (如 `enableDocking`)，並在內部自動進行位元運算映射。

---

## 2. 專案子模組導覽

目前專案的核心邏輯被劃分為以下幾個主要的模組，詳細說明請見各模組的 `README.md`：

### 2.1 系統框架與生命週期 (ImTK & ImTK_Silk)
* **`ImTKModule`**：全自動的依賴與生命週期管理。系統會透過反射自動尋找繼承此類別的私有嵌套模組並初始化。
* **`ImTKSilk`**：程式進入點，負責橋接 Silk.NET 視窗，綁定 ImGuiContext，並驅動 `ImTKModule` 的 `Update`/`Render` 主迴圈。

### 2.2 UI 樹狀結構 (VisualElement)
* 核心基底類別。它放棄了通用的 `Hierarchy<T>`，轉而採用「邏輯樹」與「物理樹」雙層設計（透過 `contentContainer` 屬性重定向）。
* 擁有獨特的防連鎖崩潰走訪機制：在算圖或更新迭代 (`BeginIteration`) 期間，任何對節點的增刪改都會被延遲到迭代結束後執行，確保事件觸發時的狀態一致性。

### 2.3 視窗與狀態管理 (Window)
* 將 ImGui Window 封裝為 Retained Mode 物件。
* 支援兩種開啟模式：單例工具面板 (`Window.Open<T>()`) 與動態多實例 (`instance.Open()`)。
* 具備防止佈局崩潰的「唯一命名檢查表」，並支援將視窗開啟狀態與客製化資料持久化儲存於 `window_state.json`。
