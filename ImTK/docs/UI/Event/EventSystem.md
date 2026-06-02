# ImTK 事件冒泡系統 (Event System & Object Pool)

## 1. 摘要

為了在 Immediate Mode (ImGui) 基礎上提供類似 Unity UI Toolkit 或 HTML DOM 的 Retained Mode 互動體驗，ImTK 實作了一套基於「事件冒泡 (Event Bubbling)」、「物件池 (Object Pool)」與「延遲派發 (Deferred Dispatch)」的事件系統。

本文件專注於 `VisualElement` 節點之間的 UI 互動事件（如點擊、滑鼠移入/移出等）。全域的按鍵攔截與廣播將由其他的 Global Event Bus 處理。

---

## 2. 事件系統設計核心 (The Event Pipeline)

### 2.1 事件的解耦與延遲派發 (Event Queuing)
由於 ImGui 的互動檢測與渲染是綁定的（例如 `ImGui.Button` 的回傳值就是點擊判定），若在事件的 Callback 中直接呼叫 `VisualElement.Add/Remove`，將會導致渲染迴圈中發生 `Collection was modified` 異常。

**解決方案：**
* **生產階段 (GuiRender)**：在渲染期間，UI 元件不會立即執行開發者註冊的 Callback，而是將產生的事件推入全域或局部的 Event Queue 中。
* **消費階段 (LogicUpdate / 渲染後)**：在安全的生命週期階段，框架統一將 Queue 中的事件取出並派發。此時開發者在 Callback 中修改 UI 樹結構是完全安全的，且能立即生效。

### 2.2 事件冒泡路徑 (Bubbling Route)
UI 事件被派發時，會沿著 **邏輯樹 (`VisualElement.parent`)** 由下往上冒泡，而不是物理渲染樹。
* 這是為了支援封裝（Shadow DOM 機制）。例如被封裝在 `ScrollView` 內部容器的 `Button` 觸發事件後，外部邏輯上的 `ScrollView` 可以正確地收到該事件。
* 在任何 Callback 中呼叫 `e.StopPropagation()` 即可終止冒泡。

---

## 3. `UIEventBase` 與屬性狀態

所有 UI 事件繼承自 `UIEventBase`。根據專案命名規範，其核心狀態屬性採用小寫駝峰 (camelCase)：

*   `public VisualElement source { get; internal set; }`：事件的**原始觸發來源**。
*   `public VisualElement current { get; internal set; }`：事件**當前冒泡到達的節點**。
*   `public bool IsPropagationStopped { get; private set; }`：是否被中斷。

### Object Pool (物件池機制)
為避免在每幀觸發的事件（如 Hover）造成嚴重的 GC 垃圾，事件必須從物件池中獲取。
*   **獲取**：`ClickEvent.GetPooled()`
*   **回收**：事件冒泡完成後，派發器自動調用 `event.Dispose()` 或放回池中。
*   **重置**：`UIEventBase` 提供 `Init()`/`Reset()` 在回收時清理 `source` 與 `current`，防止指標殘留 (Memory Leak)。

---

## 4. 滑鼠事件與 ImGui 狀態機的折衷 (Hybrid Approach)

要完全在 C# 層實作 Hit Test (碰撞檢測) 並模擬滑鼠事件是非常昂貴且容易與 ImGui 原生焦點機制的打架的。因此，ImTK 採用了依賴 ImGui 狀態機來推導事件的「折衷方案」。

### 4.1 依賴 ImGui 推導事件
我們不自己計算座標，而是在 `VisualElement.RenderVisualTree()` 呼叫 ImGui 渲染後，透過 `ImGui.IsItemHovered()`、`ImGui.IsItemActivated()` 等狀態機來決定是否發射事件。
對於持續捕捉 (Pointer Capture) 行為，我們也完全依賴 ImGui 的 Active ID 鎖定機制（例如 Slider 在拖曳時自動鎖定）。

### 4.2 `pickingMode` (穿透機制) 與狀態記憶
ImGui 預設的 Hover 是排他的（滑鼠放在子元件上時，被遮擋的父容器不算被 Hover），這違反了事件冒泡的常理。
因此，我們在 `VisualElement` 中引入了修正機制：
1.  **狀態記憶**：使用 `m_wasHovered` 記住上一幀的狀態，以推導出邊緣觸發的 `MouseEnterEvent` 與 `MouseLeaveEvent`。
2.  **`pickingMode` 屬性**：
    *   `PickingMode.Position` (預設)：會阻擋滑鼠事件，作為實體的碰撞體。
    *   `PickingMode.Ignore` (可穿透)：自身計算 Hover 時會無視碰撞，並將游標穿透給底層（或在父子間做狀態收集與修正），從而確保 UI 冒泡的反饋符合現代 UI 框架的常理。
