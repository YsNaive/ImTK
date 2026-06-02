# TODO / Task Tracker

這裡追蹤 ImTK 專案的功能開發與重構進度。
已完成的項目將記錄於根目錄的 `CHANGELOG.md` 中。

**AI Agent 注意**：嚴禁自動標記完成，必須與使用者確認後才能移動或刪除任務狀態。

## 🟢 進行中 (In Progress)
* （無）

## 🔵 待處理 (To Do)

### 🛠️ 除錯工具開發計畫 (Debug Tools)

* **[Debug/ImGuiTools] 偵錯/ImGui 內建工具**
  * 職責：暴露 ImGui 原生的除錯介面。
  * 基本功能：提供開關顯示 `ShowMetricsWindow` (繪製數據)、`ShowStyleEditor` (樣式編輯)、`ShowStackToolWindow` (ID 追蹤)。

* **[Debug/VisualElement] 偵錯/元件樹 (Inspector)**
  * 職責：檢視目前 UI 的層級結構與節點狀態。
  * 基本功能：顯示 VisualElement 樹狀圖、選中節點的詳細屬性 (Bounds, 狀態)、畫面高亮顯示選取元件。
* **[Debug/Database] 偵錯/資源** (延後/後續擴充)
  * 職責：監控 `ImTKDatabase` 載入的資源與記憶體狀態。
* **[Debug/Event] 偵錯/事件** (延後/後續擴充)
  * 職責：追蹤事件流與輸入焦點狀態。

## 🟡 技術債 (Tech Debt)
