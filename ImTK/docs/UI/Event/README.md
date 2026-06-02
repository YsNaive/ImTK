# UI 區域事件 (UI Events)

本模組提供了綁定於 `VisualElement` 邏輯樹上的**區域事件冒泡系統**，並內建了高效的事件物件池 (Object Pool) 機制以避免 GC 垃圾產生。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 核心底層與註冊機制

所有 UI 事件皆繼承自基底類別 `UIEventBase`，必須透過 `VisualElement.RegisterCallback<T>` 來訂閱。

*   **[`UIEventBase`](../../../UI/Event/UIEventBase.cs)**: 所有 UI 事件的基底類別，包含事件來源 (`source`) 與當前冒泡節點 (`current`) 等屬性。
*   **[`EventDispatcher`](../../../UI/Event/EventDispatcher.cs)**: 負責於安全週期內，將駐留在佇列中的延遲事件派發至 `VisualElement`。
*   **[`EventPool<T>`](../../../UI/Event/EventPool.cs)**: UI 事件的物件池，支援 `Get()` 與回收，降低高頻觸發事件的記憶體負擔。

### 2. 滑鼠互動事件

*   **[`MouseEnterEvent`](../../../UI/Event/MouseEvents.cs)**: 當滑鼠進入元素區域時觸發（可依據 `pickingMode` 決定是否阻擋穿透）。
*   **[`MouseLeaveEvent`](../../../UI/Event/MouseEvents.cs)**: 當滑鼠離開元素區域時觸發。
*   **[`ClickEvent`](../../../UI/Event/MouseEvents.cs)**: 當滑鼠點擊元素時觸發。

### 3. 數值與結構改變事件

*   **[`ValueChangedEvent<T>`](../../../UI/Event/ValueChangedEvent.cs)**: 當輸入框 (如 `TextField`, `IntField`) 的數值發生改變時觸發，包含舊值與新值。
*   **[`HierarchyChangedEvent`](../../../UI/Event/HierarchyChangedEvent.cs)**: 當 UI 樹狀結構（父子關係）發生變化時觸發。為避免 Collection was modified，此事件會在下一幀延遲派發。

### 4. 樹狀視圖事件 (TreeView)

*   **[`TreeNodeSelectedEvent`](../../../UI/Event/TreeEvents.cs)**: 當使用者點擊選取 `TreeNode` 時觸發。
*   **[`TreeNodeExpandedEvent`](../../../UI/Event/TreeEvents.cs)**: 當使用者展開或折疊 `TreeNode` 的子節點時觸發。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討底層設計的技術文件：

*   **[`EventSystem.md`](EventSystem.md)**：深入探討為何要依賴 ImGui 狀態機來推導滑鼠事件 (Hybrid Approach)、事件冒泡路徑 (Bubbling Route) 的設計考量，以及延遲派發 (Deferred Dispatch) 的運作機制。
