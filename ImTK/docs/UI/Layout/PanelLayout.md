# 頂層佈局管理器 (Panel Layout Architecture)

## 1. 摘要

在 ImTK 框架中，為了實現類似現代軟體（如 Unity Editor）的複雜視窗佈局（包含頂部的 MenuBar、底部的 StatusBar、側邊的 SideBar 以及中央用來停靠視窗的 DockSpace），我們引入了一個全域的頂層佈局模組：**`Panel`**。

本文件說明 `Panel` 如何透過**抽象的空間切割演算法 (Reserved Area API)** 來達成高擴充性、低耦合的頂層介面渲染。

---

## 2. 佈局預留區域機制的設計 (Reserved Area API)

與其將 `Panel` 寫死成管理特定的 `MainMenuBar` 或 `StatusBar`，我們將佈局抽象化為對可用矩形空間 (`Rect`) 的**切割與保留**。

### 2.1 機制運作原理
1.  **初始空間**：`Panel` 在渲染時，會先取得 ImGui Main Viewport 的完整可用空間 (`WorkPos` 與 `WorkSize`)。
2.  **依序切割**：`Panel` 會依照註冊的 `priority`（優先權），依序呼叫各個模組註冊的空間切割函式 (`Func<Rect, Rect> reservedFunc`)。
    *   例如：MenuBar 模組要求並切割了上方 24px 的高度。它將剩餘的矩形回傳給 `Panel`。
    *   接下來 SideBar 模組收到剩下的矩形，並要求切割左方的 200px 寬度，再將剩餘矩形回傳。
3.  **全域 DockSpace 自動掛載 (Centralized)**：所有切割函式執行完畢後，針對**最後剩下的 `Rect`**，`Panel` 會自動呼叫 `ImGui.SetNextWindowPos/Size` 鎖定該空間，並開啟一個帶有嚴格 Flags (`NoDecoration | NoMove | NoBringToFrontOnFocus | NoBackground | NoSavedSettings`) 的底層隱形宿主視窗，在裡面呼叫 `ImGui.DockSpace()` 建立全域視窗停靠區。

### 2.2 放棄原生的 `BeginMainMenuBar`
因為 ImGui 提供的 `ImGui.BeginMainMenuBar()` 內部會自動搶佔空間並干擾我們客製化的 `Rect` 系統，我們必須**放棄使用這個特化的 API**。
取而代之的是，所有的邊緣模組 (MenuBar, SideBar 等) 必須使用普通的 `ImGui.Begin()`，搭配 `ImGui.SetNextWindowPos/Size`，在自己被分配到的那塊 `Rect` 上進行繪製。
**注意**：邊緣模組必須套用極為嚴格的 Window Flags（如 `NoDecoration | NoSavedSettings | NoNavFocus | NoBringToFrontOnFocus | NoBackground`），以確保不會干擾一般視窗的焦點與層級。

---

## 3. 防呆與生命週期限制

為了避免空間切割函式在渲染過程中被隨意增加或移除，進而導致版面瞬間錯亂或運算崩潰，`Panel` 對註冊 API 設定了極其嚴格的生命週期限制：

*   **註冊階段限制**：各模組必須在系統初始化階段（即 `ApplicationState.Initialize` 期間，如模組的 `OnInitialize` 方法中）呼叫 `Panel.RequireArea(...)` 來註冊自己的切割函式。實作上會引入全域的 `isLayoutLocked` 布林值。
*   **鎖定與防呆**：在 `ApplicationState.Initialize` 結束後，系統會設定 `Panel.isLayoutLocked = true`。此後若有任何模組試圖呼叫註冊 API，`Panel` 會直接調用 `ImTKLog.Error` 並 `return`，從根本上防範執行期不可預期的佈局變更。

---

## 4. `Panel` 與 `VisualElement` 的關係

在 ImTK 中，我們貫徹**全面擁抱 VisualElement** 的設計理念。
不管是浮動的視窗（`Window`），還是透過 `Panel` 切割出來的邊緣預留區塊（MenuBar, SideBar），它們內部的排版與內容都是依賴 `VisualElement` 來建構。
這意味著 `Panel` 除了管理空間切割，還必須負責在渲染迴圈中，針對這些邊緣區塊的 `VisualElement` 根節點，呼叫它們的 `RenderVisualTree()`。
