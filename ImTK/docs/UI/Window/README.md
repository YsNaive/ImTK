# 視窗架構 (Window Architecture)

`Window` 類別是 ImTK UI 系統中最高層級的容器節點，繼承自 `VisualElement`。它提供了與 ImGui 視窗 (Begin/End)、生命週期管理以及多實例防撞機制的深度整合。

## 渲染攔截與 ID 機制 (Render Override & ID Bypassing)

一般的 `VisualElement` 會在 `Render()` 中包裹 `ImGui.PushID` 來防止相同結構的元件產生 ID 衝突。然而，對於 `Window` 而言，動態產生的 ID 會導致 ImGui 無法在 `imgui.ini` 中正確記錄與恢復視窗的停靠 (Docking) 與大小狀態。

為了解決這個問題，`VisualElement` 提供了 `m_useAutoId` 控制旗標。`Window` 預設將此旗標設為 `false`，繞過動態 ID，並強制將唯一性綁定在其字串識別碼上。

它覆寫了 `OnRenderLayout()`，在內部處理了 `ImGui.Begin` 與 `ImGui.End` 的呼叫，並根據視窗是否被展開 (`isExpanded`) 來決定是否遞迴渲染子節點 (`OnRenderSelf` 與 `hierarchy`)，藉此節省效能。

## 顯示名稱與防撞機制 (Display Name & Unique ID)

為了讓開發者能開啟多個同類型的視窗（例如多個獨立的 Inspector）而不發生版面衝突，`Window` 利用了 ImGui 的 `###` 語法魔術，將「顯示名稱」與「唯一識別碼」分離：

* **`displayName`**：使用者在畫面上看到的視窗標題。
* **`windowId`** (public get, protected set)：開發者指定的唯一識別碼。若未指定，預設為空字串。
* **`imguiId`**：內部動態組合的字串，格式為 `$"{displayName}###{windowId}"`。

## 單例與多實例管理 (Singleton vs Multi-Instance)

`Window` 的管理是由全域的 `Panel` 模組透過 `WindowKey` (結合 `Type` 與 `windowId`) 來進行 $O(1)$ 的查詢與防撞。

開發者可以透過兩種方式開啟視窗：

1. **實體方法**：
   ```csharp
   var myWindow = new MyCustomWindow("Title", "Unique_123");
   myWindow.Open();
   ```

2. **泛型語法糖 (靜態方法)**：
   ```csharp
   // 單例模式：尋找是否已有 MyCustomWindow，有則聚焦，無則建立並開啟。
   Window.Open<MyCustomWindow>();

   // 多實例模式：指定特定的 windowId 進行尋找或建立。
   Window.Open<MyCustomWindow>("Unique_123");
   ```
   *註：`Open<T>` 方法利用同類別的存取特權，在內部設定了 protected 的 `windowId`，使得開發者只需提供無參數建構子即可支援多實例操作。*

## 生命週期與 Panel 整合 (Lifecycle & Panel Integration)

`Window` 提供三個類似 Unity 的虛擬生命週期鉤子供開發者覆寫：
* `OnEnable()`: 視窗註冊並開啟時觸發，適合進行事件綁定。
* `OnDisable()`: 視窗關閉並卸載時觸發，適合進行清理。
* `Update()`: 每一幀呼叫，適合處理非渲染邏輯。

`Window` 的生命週期完全由 `Panel` 模組整合驅動。當呼叫 `Open()` 時，視窗會被註冊至 `Panel`。
`Panel` 會在 `OnGuiRender` 階段統一繪製所有已註冊的視窗，並在 `OnLogicUpdate` 階段統一呼叫 `Update()` 與處理延遲卸載 (防止迭代修改例外)。
