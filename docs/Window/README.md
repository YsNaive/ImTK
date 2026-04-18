# Window 視窗系統

`Window` 類別繼承自 `VisualElement`，它將 ImGui 原生的 Window 概念封裝為 Retained Mode 的 C# 物件，並具備強大的狀態持久化能力與生命週期管理。

## 1. 雙軌啟動模式 (Dual Open Modes)

針對不同的 UI 需求，`Window` 系統支援兩種不同的啟動與管理模式：

### 模式 A：單例工具面板 (Singleton Mode)
* **API**：`Window.Open<T>()`
* **適用場景**：工具列、屬性面板、日誌主控台。在應用程式中「同時間只能存在一個」的視窗。
* **特性**：系統會自動透過靜態字典 `windowsTable` 追蹤它。若視窗已開啟，再次呼叫只會使其取得焦點。

### 模式 B：多實例動態視窗 (Multiple Instances Mode)
* **API**：先 `new MyWindow()` 建立實例，然後呼叫 `instance.Open()`。
* **適用場景**：文件編輯器、多重圖表。使用者可以開很多個相同型別的視窗來編輯不同的資料。
* **特性**：它**不會**被加入 `windowsTable`，生命週期由開發者自己管理或關閉。

## 2. 唯一命名限制 (Unique Name Registry)

**架構挑戰**：ImGui 底層的佈局系統極度依賴 `Window Title`（在 C# 中對應 `displayName`）作為 ID。如果兩個視窗的 `displayName` 完全相同，ImGui 會發生嚴重佈局衝突，導致視窗大小、位置互相覆蓋。

**ImTK 防呆機制**：
* 系統內部維護了一個靜態的 `s_usedWindowNames` 雜湊表。
* 當呼叫 `Open()` (無論單例或多實例) 時，系統會嚴格檢查 `displayName` 是否已被使用。
* **強制報錯**：若名稱重複，系統會直接拋出 `InvalidOperationException`。
* **開發者責任**：在開發多實例動態視窗時，開發者**必須**動態給予實例不同的名稱（如 `Doc - A.txt` 與 `Doc - B.txt`），或是利用 ImGui 的隱藏 ID 語法（例如 `"文件視窗###Doc_A"`）來通過唯一性檢查。

## 3. 生命週期與狀態持久化 (Serialization & State Restoring)

ImTK 的視窗系統不僅記錄「誰被打開了」，還能深度還原「它之前的狀態是什麼」。

* **佈局持久化 (由 ImGui 負責)**：視窗的位置 (Position) 與大小 (Size) 是由 ImGui 底層的 `imgui.ini` 自動儲存與還原的。
* **功能狀態持久化 (由 ImTK `window_state.json` 負責)**：
  1. 每當有**單例工具面板**被開啟或關閉時，系統會更新 `window_state.json`，記錄當前被開啟的型別。
  2. **深度資料序列化 (Custom Data)**：`Window` 提供兩個虛擬方法：`virtual string SerializeState()` 與 `virtual void DeserializeState(string json)`。
  3. 開發者可以覆寫這兩個方法。當系統關閉儲存時，會把 `SerializeState` 吐出來的 JSON 字串與 Type 綁在一起；下次程式啟動時，會自動透過 Reflection 生成該視窗，並把 JSON 字串傳給 `DeserializeState` 讓視窗還原先前的業務邏輯狀態。

> **注意**：目前 `window_state.json` 僅針對「單例工具面板 (`Open<T>`)」進行自動持久化追蹤。動態多實例的持久化不在本框架自動管理的範圍內，需由開發者在自身的模組邏輯中處理。

## 4. ImGui 屬性映射 (Flag Encapsulation)

為了保持 C# 物件導向的直覺性，開發者不需要直接對 `ImGuiWindowFlags` 進行位元運算。
`Window` 將常用的行為封裝為布林屬性 (Property)，如 `enableDocking`, `isResizable`, `isMovable`, `isCollapsible`, `showTitleBar`。這些屬性在背後會自動對 `windowFlags` 進行正確的位元設定。
