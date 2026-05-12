# ImTK 選單系統設計 (Menu System Architecture)

## 1. 摘要

ImTK 選單系統旨在封裝 ImGui 的 `BeginMenuBar`、`BeginMenu` 與 `MenuItem` 等 API，提供一套物件導向、符合 VisualElement 架構的選單構建機制。同時，支援類似 Unity Editor 的自動排序與分隔線功能。

---

## 2. 元件拆分與職責

為了避免 ImGui 中「節點一旦擁有子節點就會自動變成不可點擊的容器」所帶來的狀態混亂，選單元件被嚴格拆分為兩種職責：

### 2.1 `MenuView` (選單容器)
* **職責**：專門作為容器。無論內部是否有子節點，都使用 `ImGui.BeginMenu` 渲染（若標記為全域則使用 `ImGui.BeginMenuBar`）。不提供點擊事件。
* **特性**：提供 `AddItem` 路徑語法糖，並且內部維護一個根據優先權 (`priority`) 排序的子節點清單。

### 2.2 `MenuItem` (末端節點)
* **職責**：專門作為可點擊的末端項目。使用 `ImGui.MenuItem` 渲染。支援 `isChecked` 與 `onClicked` 事件。
* **約束**：嚴格禁止加入子節點。若試圖呼叫 `Add` 則會透過 `ImTKLog.Error` 進行攔截。

---

## 3. 排序機制與自動分隔線

### 3.1 `IMenuElement` 介面
所有參與選單排序的元素（包含 `MenuView` 與 `MenuItem`）都必須實作 `IMenuElement` 介面，該介面強制規範了 `name` 與 `priority` 屬性。

### 3.2 排序實作
*   為了避免操作底層 `VisualElement.hierarchy` 觸發無限遞迴的 `HierarchyChangedEvent`，`MenuView` 內部維護了一個獨立的 `List<IMenuElement> m_sortedMenuElements`。
*   當觸發 `HierarchyChangedEvent` 時，`MenuView` 僅提取是 `IMenuElement` 的子節點進行排序。
*   **隱性約束**：若傳入了非 `IMenuElement` 的子節點，它將不會被加入到排序列表中，進而不會被渲染。

### 3.3 自動分隔線 (Auto Separator)
在 `MenuView` 渲染其子項目時，會計算相鄰兩個項目的 `priority` 差值。若差值 `>= 50`，則會在渲染當前項目之前自動插入一條 `ImGui.Separator()`。

---

## 4. 路徑語法糖與衝突防護

為了便於建立深層選單（例如 `"File/Recent/ProjectA"`），`MenuView` 提供了 `AddItem(string path, Action onClick, int priority)` 方法。

### 路徑衝突攔截
在建立或尋找路徑節點時，系統會進行型別驗證。
*   若預期某個節點應為 `MenuView` 容器（如 `"A/B"` 中的 `"A"`），但實際上已存在且為 `MenuItem`，系統會觸發 `ImTKLog.Error` 並放棄操作。
*   這種防護確保了「可點擊的末端節點無法被降級為容器」的堅固語義。

---

## 5. MainMenuModule 與 PanelLayout 整合

全域的主選單列由 `MainMenuModule` 管理。它遵循 `Panel Layout` 規範：
1.  在初始化階段 (`OnInitializeSelf`) 呼叫 `Panel.RequireArea`，切出 `ImGui.GetFrameHeight() * 1.25f` 的頂部預留區域。
2.  在渲染階段 (`OnGuiRender`)，於該區域使用無邊框的 `ImGui.Begin` 視窗。
3.  內部持有一個根層級的 `MenuView`（`isMenuBar = true`），並對其發起渲染呼叫。
4.  提供全域捷徑 API（如 `MainMenuModule.AddItem`），讓其他模組可以極度便捷地掛載工具列按鈕。
