# 視覺樹架構 (Visual Tree Architecture)

ImTK UI 視覺樹實作了**雙層架構**：**邏輯樹 (Logical Tree)** 與**物理樹 (Physical Tree)**。此設計確保了開發者組合的標準元件與內部封裝結構（例如 Shadow DOM）之間的清晰分離。

## 雙層樹規則 (Dual-Tree Rules)

每一個 `VisualElement` 都有可能屬於兩個階層：
1. **邏輯樹**：透過 `VisualElement.parent` 存取。
2. **物理樹**：透過 `VisualElement.hierarchy.parent` 存取。

為了保持一致性並提供清晰的語意，`ImTK` 嚴格分離了這兩層結構，並且沒有實作屬性後備 (fallback) 邏輯。
* **`parent`** 代表該元素的邏輯擁有者。
* **`hierarchy.parent`** 代表該元素實際被渲染的物理排版父節點。

## 節點類型 (NodeType)

節點在樹狀結構中的狀態由 `VisualElement.NodeType` 定義：

* **`None`**：孤兒節點（邏輯父節點與物理父節點皆為 `null`）。
* **`LogicNode`**：透過邏輯 `Add()` 方法加入的節點（邏輯父節點與物理父節點皆存在）。
* **`PhysicsNode`**：透過 `hierarchy.Add()` 加入的內部封裝子節點（邏輯父節點為 `null`，物理父節點存在）。
* **`Invalid`**：無效狀態，元素擁有邏輯父節點但沒有物理父節點。

## 自動同步 (Auto-Sync)

為了維持一致性並確保狀態永遠不會脫鉤，移動一個 `VisualElement` 時會執行**自動同步 (Auto-Sync)** 協議：

* 當元素被加入到一個新樹（邏輯或物理）且其當前狀態為 `LogicNode` 時，它會隱式呼叫其邏輯 `parent.Remove()` 然後再加入新樹。
* 若元素狀態為 `PhysicsNode`，則會隱式呼叫其 `hierarchy.parent.hierarchy.Remove()` 然後再加入新樹。

此機制保證了元素會乾淨地從舊的階層中脫離，避免出現不一致的狀態或幽靈排版。

## 結構變更事件與延遲處理 (HierarchyChangedEvent and Deferred Processing)

每當樹狀結構發生變更（透過 `Add`, `Remove`, `Clear`, 或 `AddRange`），就會派發一個 `HierarchyChangedEvent`。

為了最佳化效能並避免在執行期間迭代時發生「Collection was modified」例外，此事件會被**延遲 (deferred)** 且**整併 (coalesced/debounced)**。

變更操作只會呼叫 `EventDispatcher.MarkHierarchyDirty(element)`。在 `LogicUpdate` 階段中，`EventDispatcher` 會使用 `HashSet` 有效地處理這些通知，無論該容器在該影格內發生了多少次結構變更，都只會派發一次不氣泡 (non-bubbling) 的 `HierarchyChangedEvent`。

## 事件氣泡 (Event Bubbling)

UI 事件（如 `MouseEvents`）嚴格沿著**物理樹** (`hierarchy.parent`) 氣泡傳遞。這確保了互動邏輯的準確性，讓內部封裝的元素（例如按鈕內部的標籤）能正確地將事件往上傳遞給它們的排版宿主，而不會發生邏輯斷層。
