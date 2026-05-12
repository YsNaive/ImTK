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

## 5. MainMenuAttribute 自動掛載

為了提升開發體驗，系統提供了 `[MainMenu(string path, int priority)]` 屬性。
`MainMenuModule` 會在初始化依賴階段 (`OnInitializeDependencies`) 掃描全域的組件：
*   標註在 **靜態無參數 (或單一 ClickEvent 參數) 方法** 上：框架會自動將其轉為委派，並透過 `MainMenuModule.AddItem` 掛載為 `MenuItem`。
*   標註在 **靜態 MenuView 欄位/屬性** 上：框架會提取該實例，並透過 `AddMenu` API 將其掛載到指定的父節點下，這對於開發動態生成的下拉選單非常方便。

---

## 6. 渲染入口與架構設計微調

*   **`VisualElement.Render()` 存取修飾詞**：為了避免在使用 C# Reflection 時產生的高昂效能開銷與迂迴設計，`VisualElement.Render()` 已被提升為 `public` 方法。它現在正式作為驅動視覺樹渲染的公開鎖定入口（處理 `PushID`, `PopID`, 事件派發等防護層），而子類別應實作受保護的 `OnRenderLayout()` 或 `OnRenderSelf()`。
*   **與 PanelLayout 整合**：全域主選單列由 `MainMenuModule` 管理。為了避免 ImGui `WindowMinSize` 的限制導致無邊框容器的 hit-box 往下覆蓋並阻擋 DockSpace 的標題列拖曳，`MainMenuModule` 會在渲染容器視窗前，推送 `ImGuiStyleVar.WindowMinSize` 設為 `0,0`。它精準佔據了 `RequireMenuArea` 所切出的空間，不干擾下方佈局。
