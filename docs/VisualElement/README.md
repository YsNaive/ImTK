# VisualElement 核心 UI 架構

`VisualElement` 是 ImTK 框架中最核心的基底類別。它放棄了通用的資料結構樹 (如 `Hierarchy<T>`)，量身打造了一套專門為了 UI 佈局與防呆而設計的「雙層樹狀結構」與「安全迭代機制」。

## 1. 雙層樹狀結構 (Dual-layer Tree Architecture) 與 Content Container 機制

在傳統的 UI 框架中，父子關係是單一的。但在 `VisualElement` 中，父子關係被解耦為兩層：

* **邏輯樹 (Logical Tree)**：開發者透過 `element.Add(child)` 和 `child.parent` 操作的樹。它代表了使用者心中 UI 的「語義結構」。
* **物理樹 (Physical Tree)**：內部使用 `element.hierarchy` 儲存，且真正被 `RenderVisualTree` 遞迴走訪算圖的結構。

### Content Container 重定向
關鍵樞紐在於虛擬屬性 `public virtual VisualElement contentContainer`。
這是一種強大的封裝機制（類似 Web 的 Shadow DOM）。如果你開發了一個複雜的 `ScrollView`，內部包含了一個滾動條與一個隱藏的 `ContentView`：
你可以覆寫 `contentContainer { get { return this.myContentView; } }`。
如此一來，外部使用者呼叫 `scrollView.Add(button)` 時，這個 button 在**邏輯上**認為自己的 parent 是 scrollView，但在**物理上**它會被加入到隱藏的 `myContentView` 裡面被正確排版。
> **彈性用法**：此設計也允許指向「非自身的控件」，以達到特殊的跨層級 UI 互動效果。

## 2. 延遲修改 (Deferred Modification) 與無分配走訪

UI 系統常常會遇到一個崩潰問題：在 `Render` 或 `Update` 走訪子節點 (迴圈) 時，某個子元件的事件被觸發，並呼叫了 `Remove()` 刪除自己。這會引發「集合已被修改 (Collection was modified)」的例外。

### `BeginIteration` 的安全鎖
為了解決這個問題，`VisualElement.Hierarchy` 實作了深度追蹤計數器：
1. 進入 `UpdateVisualTree` 或 `RenderVisualTree` 遍歷子節點前，呼叫 `BeginIteration()` (`m_iterationCount++`)。
2. 在此期間，任何呼叫 `Add`, `Remove`, `Clear` 的操作**不會立即修改陣列**。
3. 它們會被封裝為 C# 的 `Action` 委派，塞入 `m_pendingActions` 佇列。
4. 當迴圈結束 `EndIteration()` 歸零時，佇列中的指令才會被一口氣執行。
5. **事件同步性 (已優化)**：`onHierarchyChanged` 事件也是在延遲動作執行完畢後才會觸發。這確保了事件訂閱者收到通知時，邏輯樹 (`parent`) 與物理陣列狀態是完全一致的。

> **知識點**：這個機制的設計初衷是「避免每幀大量的陣列重新分配」。雖然在發生動態修改時會產生微小的記憶體分配（建立委派物件），但在多數 UI 情境下這是完全可以接受的妥協。

## 3. 事件效能最佳實踐 (Best Practices)
由於 `onHierarchyChanged` 的觸發會導致相關的佈局重算：
* **單個修改**：每次 `Add` 或 `Remove` 都會觸發一次。
* **批量操作**：強烈建議開發者使用 `AddRange` 或 `Clear` 來處理批量的增刪。系統內部經過優化，即使操作數百個元件，這兩個方法在執行完畢後也**只會觸發一次** `onHierarchyChanged`。

## 4. 樣式系統 (`ImStyle`) 的 Scoped 隔離
`VisualElement` 內建了 `ImStyle` 屬性，用於映射 `ImGuiStyleVar` 和 `ImGuiCol`。
它的初始化是懶惰的 (Lazy Initialization)。在 `RenderVisualTree` 中，會自動呼叫 `style.Push()` 與 `style.Pop()`，這確保了某個元件修改了 ImGui 樣式後，**絕對不會污染**到層級之外的其他元件。
