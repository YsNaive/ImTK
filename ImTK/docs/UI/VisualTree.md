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

## 6. Style 系統與 Theme 系統 (Style & Theme Architecture)

為了讓開發者能以類似 CSS 或 Unity UITK 的語法糖修改元件外觀，ImTK 實作了專屬的 `VisualElement.style` 與 `ImTKTheme` 系統。

### 6.1 記憶體最佳化的 Style Table
ImGui 底層 API (`PushStyleColor` / `PushStyleVar`) 接受 `uint`, `float`, `Vector2` 等型別。為了避免頻繁修改樣式時發生 Boxing 或過多的記憶體分配，`VisualElementStyle` 採用了特殊的 Union Struct (`StyleEntry`) 搭配延遲初始化 (Lazy Init) 的陣列。

* **`StyleEntry`**: 利用 C# 的 `[StructLayout(LayoutKind.Explicit)]`，讓 Color、Float、Vector2 共用同一塊最大 8 bytes 的記憶體。
* **零成本預設**: 預設情況下，`VisualElement` 不會分配任何 Style List 的記憶體，直到被顯式賦值或套用 Theme。

### 6.2 Override 與 Theme 的層級 (Cascading)
`style` 內部維護兩個 List：
1. **`m_themeStyles`**: 透過 `element.SetTheme(theme)` 或 `ApplyTheme()` 寫入，代表當前主題的設定。
2. **`m_overrideStyles`**: 透過開發者手動調用屬性糖 (如 `element.style.textColor = ...`) 寫入，代表強制覆寫。

在 `Render()` 時，管線會確保 **Override 的權重高於 Theme**。這意味著切換全域主題時，開發者的手動覆寫 (如特定變紅的警告字) 不會被洗掉。

### 6.3 Theme 的 Fallback 機制 (變體繼承)
`ImTKTheme` 支援層疊式 Fallback 機制。透過設定 `theme.parent` 指標：
1. 取值時若本體沒有值 (Nullable 狀態為 null)，會自動向上詢問 `parent`。
2. **極度容易擴展**: 若想建立一個 Error 變體，只需 `new ImTKTheme() { parent = currentTheme }` 然後修改 `TextPrimary`，其他所有屬性都會自動繼承，並隨 Parent 動態更新。

---

## 7. Flags 封裝語法糖 (`element.flags.*`)

ImGui 的元件常需要傳入不同的 Flags (如 `ImGuiWindowFlags`, `ImGuiChildFlags`)。
為了避免屬性污染基底的 `VisualElement`，並解決 ImGui Enum 開發體驗不佳的問題，框架提供了 `ElementFlags<TEnum>` 泛型基底。

**實作範例 (`WindowFlags`)**:
* 子類別只要實作對應 Enum 的 Flags 類別 (例如繼承 `ElementFlags<ImGuiWindowFlags>`)。
* 將該 Flags 物件掛載為特定元件 (如 `Window.flags`) 的唯讀屬性。
* 元件在 `Render` 底層直接調用 `ImGui.Begin("ID", ref open, flags.Value)`。這保證了極快的位元運算速度與完美的 C# IntelliSense 開發體驗，絕無冗餘的 Boolean 狀態同步問題。
