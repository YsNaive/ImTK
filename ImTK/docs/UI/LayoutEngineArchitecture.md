# Layout Engine Architecture: 排版與渲染引擎架構設計

本文件記錄了 ImTK 下一代排版引擎的架構藍圖。該引擎旨在 Immediate Mode (ImGui) 的底層基礎上，構建一套支援微型 Flexbox (Yoga-lite) 的 Retained Mode 排版系統。

此架構徹底分離了「排版空間計算」與「繪製渲染」，解決了流式排版在相對縮放 (Flex-Grow)、對齊 (Align) 與文字換行上的耦合痛點。

---

## 1. 全新生命週期：四階段 Pipeline (The Four-Phase Pipeline)

在新的架構下，畫面更新不再是單一的遞迴走訪，而是被嚴格拆分為以下四個明確的階段：

1. **Build Phase (建構快取階段)**
   - **職責**：當視覺樹結構改變（如新增、刪除節點）時，建立或更新一維的走訪快取清單。
   - **目的**：避免高頻率的遞迴走訪開銷，提升效能。

2. **Layout Phase (排版階段：整合 Measure 與 Arrange)**
   - **職責**：精準計算出每個元件的絕對與相對座標 (`layoutRect`)。
   - **流程**：從 `IWindow` 根節點發起的 **Top-Down 遞迴** (`CalculateLayout`)。
   - **整合邏輯**：為了處理文字自動換行這種深度耦合的問題，Measure (算大小) 和 Arrange (排位置) 必須在此次遞迴中整合。父節點準備空間約束 -> 呼叫子節點的 `MeasureContent` -> 子節點回報所需尺寸 -> 父節點計算出 `layoutRect`。

3. **Render Phase (渲染階段)**
   - **職責**：將算好的 `layoutRect` 對應到螢幕上並呼叫 ImGui 進行純繪圖。
   - **規範**：**絕對禁止**在此階段呼叫任何影響原游標位置的排版指令 (如 `ImGui.SameLine`)。系統純粹利用絕對定位 (`ImGui.SetCursorScreenPos`) 將元件安置，隨後呼叫元件本體的 `OnBeginRender / OnRender / OnEndRender`。

---

## 2. 空間約束機制 (Layout Constraints & MeasureMode)

為了解決子節點依賴父節點可用空間才能決定自身大小的問題，系統引入了由上向下傳遞的約束機制。

* **`LayoutConstraint` 結構**：封裝 `AvailableWidth` 與 `AvailableHeight`。
* **`MeasureMode` 三種模式**：
  - **`Exactly` (精確)**：父節點強制子節點必須是指定大小 (例如 `style.width = 200`)。
  - **`AtMost` (最大限制)**：子節點根據自身內容(如文字長度)決定大小，但不可超過給定值 (這是處理換行的關鍵)。
  - **`Undefined` (無限制)**：沒有邊界限制 (常出現於 ScrollView 內部)，子節點回報其完全伸展所需的龐大尺寸。

---

## 3. ImGui 交握與佔位策略 (Absolute Positioning & Dummy)

為確保在 ImGui 上使用絕對座標繪製時不會破壞原有的生態系統，我們採用以下策略：

1. **全面接管佈局**：所有 ImTK 的元件佈局改由自定義的 Flexbox 引擎接管，不再依賴 ImGui 內建的流式排版。
2. **根節點 Dummy 佔位**：為了讓 ImGui 知道我們繪製的總範圍 (影響 Bounding Box 和外部的 ScrollView)，我們在渲染開始時，必須利用 `ImGui.Dummy` 畫出一個總尺寸的透明佔位區塊。
3. **允許重疊 (AllowOverlap)**：由於我們使用絕對座標疊加元件，在繪製背景或佔位區塊時，需呼叫 `ImGui.SetNextItemAllowOverlap()`，確保被蓋在上面的按鈕能正確被 ImGui 的 `IsItemHovered()` 偵測到。

---

## 4. 安全剪裁與 ScrollView 支援 (Clipping)

絕對定位容易造成渲染溢出。為了限縮元件只能在分配到的 `layoutRect` 內渲染，我們依賴 ImGui 的硬體剪裁：

* 當遇到 `ScrollView` 等容器時，在 Render Phase 的 `OnBeginRender` 呼叫 `ImGui.BeginChild(layoutRect.Size, ...)`，開啟 Scissor Clip。
* **游標原點修正**：進入 Child Window 後，呼叫 Dummy 撐開所需的捲軸空間，然後將游標拉回包含捲軸偏移量的內容原點 (`contentOrigin`)。內部的子節點再依據這個原點疊加自身的絕對座標進行安全繪製。

---

## 5. 隔離機制與樣式效能優化 (Critical Optimizations)

### 5.1 `IWindow` 排版根節點隔離
* **問題**：「誰來啟動排版？」以及避免視窗被折疊時的不必要計算。
* **解法**：引入 `IWindow` 作為標記介面。`RenderEngine` 全域走訪時，只對實作 `IWindow` (具有 `Begin()/End()`) 的節點發起獨立的 Layout Phase。
* **強硬規範**：Window 的任意祖先節點不能是另一個 Window。這確保了排版樹不會發生重疊與死結。

### 5.2 `StyleProperty.LayoutAffecting` Flag 優化
* **問題**：在 Layout Phase 呼叫 `MeasureContent` 算字體長度時，如果推入所有的樣式 (包含顏色、背景) 會浪費效能，甚至在沒有 Context 的情況下引發錯誤。
* **解法**：在 `StyleProperty` 結構的 Flag 中新增 `LayoutAffecting` 標記。像 `FontSize`, `Padding`, `ItemSpacing` 擁有此標記。排版引擎在測量前呼叫 `resolvedStyle.Push(layoutMatterOnly: true)`，精準推送會影響佈局的屬性。

---

## 6. 對現有 API 的影響與遷移指南 (Migration Guide)

導入此架構後，現有程式碼需要進行以下重構與調整：

### 6.1 `RenderEngine.cs`
* 將被大幅重構，拆分出 `ExecuteLayoutPhase`。
* 現有的 `RenderNode` 將精簡，剝離所有計算空間相關的邏輯。

### 6.2 `VisualElement.cs`
* **新增屬性**：`desiredSize`, `layoutRect`, `m_isLayoutDirty`。
* **新增方法**：`CalculateLayout(LayoutConstraint)` (供框架遞迴呼叫), `MeasureContent(LayoutConstraint)` (供葉節點覆寫，計算本體尺寸)。
* **API 職責轉移**：既有的 `OnBeginRender`, `OnRender`, `OnEndRender` 介面保留不變，但**實作內絕對禁止呼叫影響佈局的 ImGui API (如 `SameLine`)**。

### 6.3 既有 UI 控制項 (`ImTK.UI.Element.Basic/`)
* 所有的基礎元件（如 `Button`, `TextField`, `CheckBox` 等）必須全數翻修，將原先寫在 `OnRender` 內的佈局邏輯拆除，並實作 `MeasureContent`，確保能乾淨且精確地對接新的四階段生命週期。
