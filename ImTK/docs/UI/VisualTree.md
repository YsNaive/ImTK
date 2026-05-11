# ImTK 視覺樹與排版系統設計 (Visual Tree & Layout Architecture)

## 1. 摘要與定位 (Abstract & Scope)

ImTK 的 UI 系統目標是**「在 Immediate Mode (ImGui) 的底層上，搭建出具備 Retained Mode (類似 Unity UI Toolkit) 開發體驗的物件導向框架」**。

`VisualElement` 是構成整個介面的基礎節點。它不僅負責封裝 ImGui 的渲染狀態機以提供自適應排版，更維護了一套複雜的**雙軌樹狀結構 (Dual-layer Tree Hierarchy)**。
為了徹底根絕 Immediate Mode 下常見的 ID 衝突與狀態修改衝突，本框架採用了嚴密的「自動 Hash ID 封裝」與「事件解耦渲染」設計。

---

## 2. 雙軌樹狀結構：Logical Tree vs Physical Tree

為了讓開發者能以最直覺的方式組合複雜元件（例如將按鈕加入捲動視窗），而不需要知道元件內部的實作細節，ImTK 採用了 `IVisualElementHierarchy` 介面將「邏輯操作」與「物理操作」明確隔離。

### 2.1 介面隔離與 `hierarchy` 封裝
* **`VisualElement` (邏輯層)**：實作 `IVisualElementHierarchy`，對外提供 `parent` (邏輯父節點) 與 `Children()`。
* **`VisualElementHierarchy` (物理層)**：專職管理內部的 `List<VisualElement> m_children` 與 `parent` (物理渲染父節點)。它被封裝為 `VisualElement` 的 `hierarchy` 屬性。

**對外 API 表現：**
* 開發者調用 `element.Add(child)` 被視為**邏輯操作**。
* 底層系統調用 `element.hierarchy.Add(child)` 被視為**物理操作**。

### 2.2 `contentContainer` 影子樹機制的轉發規則
每個 `VisualElement` 都擁有一個 `public virtual VisualElement contentContainer => this;`。子類別 (如 ScrollView) 可以覆寫它，並回傳內部用來容納子節點的容器。

當外部呼叫邏輯的 `element.Add(child)` 時，其轉發邏輯為：
1. **設定邏輯連結**：`child.parent = element`。
2. **判斷是否為最終容器**：
   * 如果 `element.contentContainer == element`：代表已經到達真正的物理儲存層，此時執行 `element.hierarchy.Add(child)` (將其加入物理清單並設定物理父節點)。
   * 如果 `element.contentContainer != element`：代表內容必須放在內部的影子容器中，則呼叫 `element.contentContainer.Add(child)` 進行遞迴轉發。

---

## 3. 渲染管線與 API 權限 (The Render Pipeline)

在 ImGui 的基礎上，`VisualElement` 的渲染流程採用了 **Template Method Pattern (樣板方法模式)**，將渲染過程拆分為三個層次，以確保排版彈性與底層狀態安全的平衡。

### 3.1 三層渲染架構

1. **`internal void Render()`** (防護罩層 / 入口)
   * **職責**：框架內部調用的絕對入口，不可被子類別覆寫 (`non-virtual`)。
   * **內容**：執行 `ImGui.PushID`、處理穿透 (`pickingMode`)、呼叫排版層 (`OnRenderLayout`)、計算並推導滑鼠 Hover 狀態、發送生命週期事件，最後 `ImGui.PopID`。

2. **`protected virtual void OnRenderLayout()`** (排版與樹狀走訪層)
   * **職責**：決定自身的視覺內容與子節點的渲染順序，負責 ImGui 範圍排版（如 `ImGui.BeginChild`, `ImGui.Indent`）。
   * **子類別實作 (如 Composite Container)**：
     ```csharp
     protected override void OnRenderLayout() {
         ImGui.Indent();
         base.OnRenderLayout(); // base 會繪製本體並用 for 迴圈走訪 child.Render()
         ImGui.Unindent();
     }
     ```

3. **`protected virtual void OnRenderSelf()`** (本體渲染層)
   * **職責**：僅用來使用 ImGui API 繪製元件自身的視覺內容（如 `ImGui.Button`），不需理會子節點。
   * **子類別實作 (如 Button)**：這是 90% 以上普通 UI 元件唯一需要覆寫的方法。

---

## 4. 根絕 ImGui ID 衝突 (Auto Hash ID)

在 ImGui 中，頻繁動態生成的 UI 元件極易發生 ID 字串 Hash 衝突（例如 ListView 中的多個相同 Button）。為了提供 Retained Mode 的無憂體驗，我們將 ID 堆疊與視覺樹強制綁定。

**實作機制：**
1. **全域唯一實例 ID**：在 `VisualElement` 建構時，透過一個靜態計數器 (`s_elementCounter`) 賦予自身一個不重複的 `int m_elementId`。
2. **自動包裹 Push/Pop**：在底層的 `Render()` 函式中，框架會自動呼叫：
   ```csharp
   ImGui.PushID(m_elementId);
   OnRenderLayout(); // 開發者自定義的渲染
   // ...
   ImGui.PopID();
   ```

---

## 5. 渲染迴圈與結構修改的安全性 (Iteration Safety)

在 ImGui 的渲染階段 (GuiRender) 修改 UI 樹結構（如 Add/Remove）會導致走訪崩潰 (`Collection was modified`)。
為了解決此問題，我們採用了 **「防呆攔截」** 搭配 **「事件解耦 (Event Queuing)」**。

**安全防護規範：**
* 所有的 `hierarchy.Add()`、`Remove()`、`Clear()` 都會**立即生效**。
* 若在這些方法內部偵測到 `ImTKApplication.CurrentState == ApplicationState.GuiRender`，框架將直接攔截操作並報錯，強制規定「禁止在渲染階段改變結構」。
