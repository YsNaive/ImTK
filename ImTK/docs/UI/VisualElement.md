# ImTK 視覺元件與渲染架構 (VisualElement & Render Architecture)

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

在 ImGui 的基礎上，`VisualElement` 的渲染流程採用了 **Template Method Pattern (樣板方法模式)**，由框架統一控制渲染流程，元件開發者只需覆寫特定的三個生命週期方法。

**特別注意 (Layout Engine 已上線)**：由於全新排版引擎已經全面導入，這些渲染方法**嚴格限定只能處理純粹的視覺繪製**。絕對禁止在實作內呼叫任何會改變游標或影響相鄰節點排版的 ImGui API（如 `ImGui.SameLine`、`ImGui.Indent` 等）。所有的排版與空間計算都已由獨立的 Layout Phase 接管處理。

### 3.1 三層渲染生命週期

`RenderEngine.RenderFlat` 在走訪視覺樹時，會嚴格依序呼叫以下三個對外開放的虛擬方法：

1. **`public virtual bool OnBeginRender()`** (渲染前置與範圍定義)
   * **職責**：用於宣告需要包裹子節點的 ImGui 範圍（例如 `ImGui.BeginChild`、`ImGui.BeginGroup`）。
   * **回傳值**：決定**是否要繼續渲染子節點**。若回傳 `true`，框架會遞迴走訪並渲染其內部所有的子元件；若回傳 `false` 則會跳過子節點走訪（常用於視窗折疊或被 Clip 裁切時優化效能）。

2. **`public virtual void OnRender()`** (本體渲染)
   * **職責**：僅用來呼叫 ImGui API 繪製元件自身的純視覺內容（如 `ImGui.Button`、`ImGui.Text`）。對於多數葉節點元件而言，這是唯一需要覆寫的方法。

3. **`public virtual void OnEndRender()`** (渲染後置與範圍收尾)
   * **職責**：與 `OnBeginRender` 成對出現，負責關閉作用域（例如呼叫 `ImGui.EndChild`、`ImGui.EndGroup`）。

### 3.2 內部防護與事件推導
框架底層在呼叫這三個方法的過程中，會自動為元件套用以下安全防護：
* **自動 ID 堆疊**：利用 `PushID/PopID` 避免同名元件發生衝突。
* **樣式隔離**：執行 `requiredStyle.Push()` 與 `Pop()`，確保元件只受到屬於自己的 CSS/Theme 影響。
* **事件推導解耦**：在渲染結束後，根據 `pickingMode` 統一計算本體與子節點的 Hover 狀態，並安全地推送 `MouseEnterEvent` 或 `MouseLeaveEvent` 至事件佇列，不在渲染中途觸發 Callback。

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

### 6.1 HashedString 與 VisualElement.StyleKey (核心解耦)
為了將 UI 元件的樣式語義 (如 BackgroundColor) 與 ImGui 底層具體的 ImGuiCol (如 WindowBg, Button 等) 徹底解耦，我們引進了 `VisualElement.StyleKey` 統一列舉。並且透過 `HashedString` 實作 Token (如 "--primary") 和 class 名字的 O(1) 查表。

* **`StyleProperty`**: 利用 C# 的 `[StructLayout(LayoutKind.Explicit)]`，讓 Color、Float、Vector2、Int 和 TokenHash 共用記憶體，實現零裝箱與極致效能。（註：為了維持嚴格的 16 bytes 長度，`Thickness` 等複合型別已被拆解為四個獨立的 Float Key）。
* **零成本預設**: 預設情況下，`VisualElement` 不會分配任何 Style List 的記憶體，直到被顯式賦值或套用 Theme。

### 6.2 層疊樣式表 (Cascading Style Sheets) 與 ComputedStyle
ImTK 的樣式遵循完整的 CSS 層疊優先級 (Inline > Local Ancestor > Global)：
1. **`StyleSheet.Global`**: 全域註冊的 `StyleBlock` (透過 class 綁定)。
2. **`localStyleSheet`**: 掛載於祖先節點的樣式表 (向父節點遞迴層疊)。
3. **`Inline Style`**: 元件自身的 `style` 覆寫，優先級最高。

當元素的 `classList` 修改時，會觸發 Dirty 標記，並在 `Render()` 階段由 `ComputeStyle.Overlay` 快取出一份合併好的 `m_computedStyles`，然後利用 `StyleMapping` 查表自動將 `VisualElement.StyleKey` 注入為 `ImGuiCol`，保證每幀渲染效能。

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
* 將該 Flags 掛載為特定元件 (如 `Window.flags`) 的唯讀屬性。
* 元件在 `Render` 底層直接調用 `ImGui.Begin("ID", ref open, flags.Value)`。這保證了極快的位元運算速度與完美的 C# IntelliSense 開發體驗，絕無冗餘的 Boolean 狀態同步問題。

## 8. 邏輯像素與自適應縮放 (Logical Pixels & Adaptive Scaling)

在多螢幕、高 DPI (如 4K) 的現代環境中，ImTK 採用了**「邏輯像素 (Logical Pixels)」**與**「物理像素 (Physical Pixels)」**分離的優雅設計。

### 8.1 隱式轉換架構
開發者在設定 `element.style.width`、`padding` 或字型大小時，所有的數值皆被視為純粹的**邏輯像素**。
無論在哪種解析度的螢幕上，開發者與 Debugger 看到的都是這個原始的邏輯數值。

### 8.2 管線末端的並行縮放
在 `RenderEngineStylePipeline` 解析樣式的最後階段，框架會透過 `RenderingContext.CurrentDpiScale` 動態捕捉當前視窗所屬螢幕的 DPI 比例。
當比例不為 1.0 時，框架會對分流後的兩個核心資料結構並行執行物理像素換算：
1. **`resolvedLayoutState.Scale()`**：將換算後的物理尺寸餵給 Flexbox 排版引擎，確保畫面留下精準的空間。
2. **`resolvedStyle.Scale()`**：將換算後的樣式餵給 ImGui 原生繪製引擎，包含 `PushFontOnly` 的字型放大，確保高解析度下的光柵化銳利度。

這種設計不僅徹底解耦了繪圖引擎與排版引擎，更讓視覺元件自身不需要處理任何乘法與比例計算，達到完美的跨螢幕 DPI 自動適應體驗。

## 9. 未提及的概念補充 (Concept Supplements)

### 9.1 FontSource (字型來源與 Glyph 範圍)
`FontSource` 封裝了字型檔案的路徑、大小與支援的字元範圍 (GlyphRanges)。它能自動偵測作業系統預設字型目錄並容錯處理副檔名，為多國語言與中文字型提供可靠的載入基礎。

### 9.2 ResolvedStyle (樣式計算與快取)
`ResolvedStyle` 負責管理一個元件最終的合併樣式。透過層疊計算（Inline > Theme Fallback > Global StyleSheet），將結果快取於此，並提供極低的 GC 記憶體分配與 O(1) 效能。

### 9.3 NodeType (節點類型)
`NodeType` 列舉用於區分目前的元件在視覺樹中所扮演的角色（如 `LogicNode` 或 `PhysicsNode`），輔助系統正確地執行雙軌樹結構的同步與解綁。

### 9.4 WindowKey (視窗識別)
`WindowKey` 是一個內部結構，結合了視窗型別 (`Type`) 與特定的識別字串 (`WindowId`)，用來在全域生命週期中精確地追蹤並防止多個相同配置的視窗重複開啟或錯亂。

### 9.5 StyleClass, StyleKeyword 與 StyleValue
*   **StyleClass**：實作了類似 CSS `classList` 的機制，允許開發者動態為元件 `Add`, `Remove`, `Toggle` 類別，觸發樣式的重新計算。
*   **StyleKeyword**：定義了樣式狀態的關鍵字，例如 `Null` (未設定) 與 `Inherit` (繼承自父層)。
*   **StyleValue**：泛型結構體，封裝了具體數值與 `StyleKeyword`，允許流暢地以隱式轉換的方式設定元件的樣式。
