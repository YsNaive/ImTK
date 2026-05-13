# FieldDrawer 欄位渲染系統

`FieldDrawer` 系統是用於在 ImTK 框架中，根據資料型別動態生成並渲染可互動 UI 的核心架構。它類似於 Unity Editor 的 PropertyDrawer，提供了從資料對應到 UI 元素的自動化綁定與渲染能力。

## 1. 核心概念與資料流

`FieldDrawer<T>` 是所有具體欄位渲染器的基底類別，它繼承自 `VisualElement`。

為了處理複雜的資料狀態（特別是 Value Type 與 Reference Type 的差異，以及 UI 變更與 Data Binding 的循環問題），資料流被嚴格收口在內部的 `_SetValue` 方法，並向外提供以下 Proxy 方法：

*   **`T value { get; set; }`**：標準賦值。會進行相等性檢查 (`EqualityCheck`)，若值有改變則更新資料、觸發 UI 重繪，並派發 `ValueChangedEvent<T>` 事件。
*   **`SetValueWithoutNotify(T newValue)`**：常供外部資料綁定 (Data Binding) 系統同步 UI 狀態時使用。只更新內部資料並重繪，**不會觸發事件**，避免造成無限循環。
*   **`SetValueWithChanged(T newValue)`**：供具體的 Drawer 子類（如 `IntField`）內部呼叫。當使用者在 ImGui 控制項中修改數值時使用。因為這代表確切的修改，它會略過相等性檢查，直接更新並觸發事件。
*   **`NotifyValueChanged()`**：主要供 Reference Type (如 `ObjectDrawer`) 使用。當內部的子屬性改變時，即使自身的 Reference 沒變，呼叫此方法可強制派發一個 `isInternalChange = true` 的事件，通知外部其內容已被修改。

## 2. 事件系統

### `ValueChangedEvent` 與 `ValueChangedEvent<T>`
數值變更的通知使用自訂的 `ValueChangedEvent<T>` 事件。
*   **不冒泡 (Non-bubbling)**：為了效能與邏輯清晰，Drawer 的事件預設不會在 Visual Tree 中往上冒泡。需要監聽的系統必須直接對該 Drawer 註冊 Callback。
*   **`isInternalChange` 標記**：當處理 Reference Type 時，若只是內部屬性變更，`previousValue` 與 `newValue` 可能指向同一個物件。這個標記可讓監聽者明確區分是「全新替換」還是「內部狀態改變」。

## 3. 佈局模式 (Layout Mode)

`FieldDrawer` 原生支援了標準的 `[ Icon ] [ Label ] [ Field ]` 排版結構，並將其分為兩種模式：

*   **`Inline` (同行展開)**：適合簡單的 Value Type (`int`, `string`)。Label 與實際的輸入控制項會在同一行繪製（透過 `ImGui.SameLine()` 等技巧）。
*   **`Expand` (換行展開)**：適合複雜的 Reference Type 或佔據較大空間的控制項。Label 會繪製在上方，輸入控制項或子節點會從下一行開始繪製。

子類不需要覆寫排版邏輯，只需在覆寫的 `OnRenderSelf()` 中處理具體的 ImGui 輸入控制項即可。若是複合型 Drawer（會產生子 VisualElement），甚至不需覆寫，依賴 Visual Tree 的原生遞迴渲染即可。

## 4. 註冊與自動生成

系統提供了 `FieldDrawerRegistry` 與 `FieldDrawerFactory` 來達成自動化建構。

### `[CustomFieldDrawer]` 標籤
開發者可使用此 Attribute 將特定的 Drawer 類別註冊給指定的資料型別。
*   **`targetType`**：目標資料型別（如 `typeof(int)`）。
*   **`requiredModifier`**：可選的修飾器。例如，可為 `typeof(int)` 註冊一個專屬的 Drawer 並要求欄位必須帶有 `[SliderAttribute]`。
*   **`allowInheritType`**：預設為 `true`。若為 true，當找不到精確匹配的子型別 Drawer 時，Registry 會自動向上尋找其基底類別的 Drawer。

### `FieldDrawerFactory`
這是一個提供 Fluent API (流暢介面) 的建構器。它能根據給定的值或型別，自動從 Registry 中尋找最合適的 Drawer 實例化，並可接收額外的 Modifier Attributes。
```csharp
var drawer = FieldDrawerFactory.Create()
    .FromType(typeof(int))
    .Label("My Age")
    .AddModifier(new SliderAttribute(1, 100))
    .Build();
```
Drawer 在建立後，可透過實作 `ApplyModifier(Attribute)` 來讀取修飾器的參數（如 Slider 的 min, max 值）以調整自身狀態。
