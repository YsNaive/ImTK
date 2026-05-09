# ImTK 視覺樹與事件排版系統設計 (Visual Tree, Event & Layout Architecture)

## 1. 摘要與定位 (Abstract & Scope)

ImTK 的 UI 系統目標是**「在 Immediate Mode (ImGui) 的底層上，搭建出具備 Retained Mode (類似 Unity UI Toolkit) 開發體驗的物件導向框架」**。

`VisualElement` 是構成整個介面的基礎節點。它不僅負責封裝 ImGui 的渲染狀態機以提供類似 CSS/Flexbox 的自適應排版，更維護了一套複雜的**雙層樹狀結構 (Dual-layer Tree Hierarchy)** 與**事件冒泡系統 (Event Bubbling)**。這賦予了開發者極致乾淨的 API 體驗與強大的複合元件開發能力。

*(註：`VisualElement` **不是** `ImTKObject`。它的生命週期渲染 (`OnGuiRender`) 必須由其頂層容器，如 `Window` 來驅動，以確保 ImGui `Begin/End` 巢狀呼叫的絕對安全。)*

---

## 2. 雙層樹狀結構：Logical Tree vs Physical Tree

為了讓開發者能以最直覺的方式組合複雜元件（例如將按鈕加入捲動視窗），而不需要知道元件內部的實作細節，ImTK 採用了類似 Shadow DOM 的雙層樹結構設計。

### 2.1 核心概念：`contentContainer` 影子樹機制
每個 `VisualElement` 都擁有一個 `contentContainer` 屬性。
*   **預設情況**：`contentContainer` 指向自己。呼叫 `Add(child)` 時，子節點直接加入自身的物理清單。
*   **複雜元件 (如 ScrollView)**：它內部建立了一個用來限制範圍的 `m_scrollContainer` 節點，並覆寫 `contentContainer => m_scrollContainer;`。

**轉發規範 (Forwarding Rule)**：
當外部開發者呼叫 `Add`, `Insert`, `Remove`, `Clear` 時，這些操作**必須**被轉發給 `contentContainer` 執行。這確保了外部看似扁平的「邏輯樹」，在底層被安全地放入正確的「物理樹」位置。

### 2.2 Parent 的邏輯錯亂與雙軌指標
當 `Button` 被加入了 `ScrollView` 內部的影子容器，它的父節點必須被清楚區分，否則會引發邏輯與渲染的雙重災難：

*   **`physicalParent` (內部渲染用)**：指向真正的容器 (如 `m_scrollContainer`)。專供 ImTK 底層進行樣式疊加與 ImGui 巢狀呼叫使用。
*   **`parent` (外部邏輯用)**：指向邏輯上的容器 (如 `scrollView`)。專供開發者進行 UI 樹查詢與**事件冒泡**使用。

---

## 3. 模擬事件冒泡系統 (Bubbling Event System)

ImGui 原生並無 DOM 事件傳遞的概念。為支援複雜的 UI 互動（如事件代理 Event Delegation），ImTK 在 `VisualElement` 中實作了一套對標 Unity UITK 的事件冒泡系統。

### 3.1 核心事件結構
事件物件採類別 (Class) 實作，並搭配 Object Pool 以避免頻繁觸發時的 GC 壓力。

```csharp
public abstract class UIEventBase
{
    public VisualElement Target { get; internal set; }        // 原始觸發者
    public VisualElement CurrentTarget { get; internal set; } // 當前處理者
    public bool IsHandled { get; private set; }

    public void StopPropagation() => IsHandled = true; // 攔截事件
}
```

### 3.2 派發與冒泡邏輯 (Dispatching & Bubbling)
*   **發送**：子元件透過呼叫 `SendEvent(new ClickEvent())` 觸發事件。
*   **冒泡路徑**：事件**嚴格沿著邏輯樹 (`parent` 指標)** 向上傳遞。這確保了被封裝在 `contentContainer` 內的子節點事件，能正確無誤地傳遞給外部呼叫 `Add()` 的邏輯父容器。
*   **攔截**：只要任何一層的監聽器呼叫了 `e.StopPropagation()`，冒泡迴圈即刻終止。

```csharp
// 開發者使用範例：在 List 容器上統一監聽內部所有子項目的點擊事件 (事件代理)
myListView.RegisterCallback<ClickEvent>(e =>
{
    Console.WriteLine($"點擊了：{e.Target.Name}");
    e.StopPropagation(); // 阻止事件繼續向 Window 傳遞
});
```

---

## 4. 排版系統設計 (Layout System Encapsulation)

ImGui 本質上是「打字機流 (Cursor Flow)」的排版，預設由上而下繪製。ImTK 不引入重量級的座標計算引擎，而是設計**特定職責的排版節點 (Layout Nodes)** 來安插 ImGui 的排版指令。

### 4.1 基礎流向控制 (Flow Control)
*   **`VerticalView`**：預設排版容器。無需額外邏輯，維持 ImGui 的向下排版。
*   **`HorizontalView`**：水平排版容器。覆寫其渲染邏輯，使用 `ImGui.BeginGroup()` 包裝，並在走訪子節點時，從第二個子節點開始插入 `ImGui.SameLine()` 指令。
*   **`ScrollView`**：獨立捲動與裁剪容器。在走訪子節點前後，分別插入 `ImGui.BeginChild(...)` 與 `ImGui.EndChild()`。

### 4.2 自適應尺寸與樣式綁定 (Auto-sizing & Styling)
為支援「填滿剩餘空間 (100% Width)」等自適應排版，必須擴充 `ImStyle` 系統，並在渲染前干預 ImGui 狀態機。

*   **開發者 API**：`btn.Style.Width = new ImLength(100, LengthUnit.Percent);`
*   **渲染前狀態預設**：在 `VisualElement.Render()` 實際呼叫 ImGui 繪製指令前，解析自身的 `ImStyle`。若發現 `Percent(100)`，則自動安插執行 `ImGui.SetNextItemWidth(-1f);`。

透過這種「樣式解析 -> ImGui 狀態預設 -> 元件渲染」的標準化流程，ImTK 得以在純 Immediate Mode 的底層上，無縫還原出現代前端的自適應排版體驗。