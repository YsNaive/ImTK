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

---

## 6. 高效 CSS 子集樣式系統 (StyleSystem & Design Tokens)

為了讓開發者能以類似 CSS 或 Unity UITK 的語法糖修改元件外觀，ImTK 實作了專屬的 `VisualElement.style` 與 `ImTKTheme` 系統。

### 6.1 HashedString 與 ImTKStyleKey (核心解耦)
為了將 UI 元件的樣式語義 (如 BackgroundColor) 與 ImGui 底層具體的 ImGuiCol (如 WindowBg, Button 等) 徹底解耦，我們引進了 `ImTKStyleKey` 統一列舉。並且透過 `HashedString` 實作 Token (如 "--primary") 和 class 名字的 O(1) 查表。

* **`StyleProperty`**: 利用 C# 的 `[StructLayout(LayoutKind.Explicit)]`，讓 Color、Float、Vector2 和 TokenHash 共用記憶體，實現零裝箱。
* **零成本預設**: 預設情況下，`VisualElement` 不會分配任何 Style List 的記憶體，直到被顯式賦值或套用 Theme。

### 6.2 層疊樣式表 (Cascading Style Sheets) 與 ComputedStyle
ImTK 的樣式遵循完整的 CSS 層疊優先級 (Inline > Local Ancestor > Global)：
1. **`StyleSheet.Global`**: 全域註冊的 `StyleBlock` (透過 class 綁定)。
2. **`localStyleSheet`**: 掛載於祖先節點的樣式表 (向父節點遞迴層疊)。
3. **`Inline Style`**: 元件自身的 `style` 覆寫，優先級最高。

當元素的 `classList` 修改時，會觸發 Dirty 標記，並在 `Render()` 階段由 `ComputeStyle.Overlay` 快取出一份合併好的 `m_computedStyles`，然後利用 `StyleMapping` 查表自動將 `ImTKStyleKey` 注入為 `ImGuiCol`，保證每幀渲染效能。

### 6.3 Design Tokens 與自動 Fallback
Theme 從死板的屬性轉型為 `Dictionary<int, Color>` 等 Token 儲存器：
1. 透過字串 Token (如 `style.backgroundColor = "--bg"`) 設定屬性。
2. **安全偵錯**: 在 `ComputeStyle` 解析時若找不到 Token，系統會觸發 `ImTKLog.Warning` 並 Fallback 至極度鮮豔的顏色 (如 `Color.Magenta`)，以防視覺污染。

---

## 7. Flags 封裝語法糖 (`element.flags.*`)

ImGui 的元件常需要傳入不同的 Flags (如 `ImGuiWindowFlags`, `ImGuiChildFlags`)。
為了避免屬性污染基底的 `VisualElement`，並解決 ImGui Enum 開發體驗不佳的問題，框架提供了 `ElementFlags<TEnum>` 泛型基底。

**實作範例 (`WindowFlags`)**:
* 子類別只要實作對應 Enum 的 Flags 類別 (例如繼承 `ElementFlags<ImGuiWindowFlags>`)。
* 將該 Flags 物件掛載為特定元件 (如 `Window.flags`) 的唯讀屬性。
* 元件在 `Render` 底層直接調用 `ImGui.Begin("ID", ref open, flags.Value)`。這保證了極快的位元運算速度與完美的 C# IntelliSense 開發體驗，絕無冗餘的 Boolean 狀態同步問題。
