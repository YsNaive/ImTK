# RuntimeDrawer 架構與使用指南

`RuntimeDrawer` 是 ImTK 框架中負責資料呈現與互動輸入的核心元件體系，取代了舊有的 `FieldElement<T>` 系統。其設計哲學深受 Unity UI Toolkit (`[Indent][Label][Field]`) 啟發，致力於分離排版、標籤與底層 ImGui 控制項，並提供強大的資料綁定與事件冒泡機制。

## 1. 核心類別結構

### `IDrawer` 介面
定義了非泛型的抽象互動介面，包含 `GetValue()`, `SetValue()`, 以及 `RegisterValueChanged` 等無型別事件綁定。

### `RuntimeDrawer` (非泛型基底類別)
負責處理所有與**視覺佈局 (Layout)** 相關的邏輯。
* 負責讀取與套用級聯樣式 (`drawerLabelWidth` 與 `drawerIndentWidth`)。
* 負責計算絕對縮排 (`SetCursorPosX`)。
* 負責繪製左側統一寬度的標籤 (`label`)。
* 負責管理子物件容器 (`contentContainer`) 與 `indentLevel`。

### `RuntimeDrawer<T>` (泛型基底類別)
繼承自 `RuntimeDrawer`，負責**資料狀態 (State)** 與**型別安全**。
* 維護強型別的 `value`。
* 實作變更檢測 (透過 `EqualityComparer`)，若資料改變，自動觸發事件。
* 提供與 `IDrawer` 非泛型介面的雙向綁定橋接。

## 2. 佈局與排版 (Alignment & Layout)

在 `RuntimeDrawer` 的渲染流程中，排版是由絕對游標位置嚴格控制的：
1. 元件起點 X = `當前游標 X` + (`indentLevel` * `drawerIndentWidth`)。
2. 繪製 `label`。
3. 下一個繪製物件 (通常是子類別的 ImGui 輸入框) 的起點 X 會被強制鎖定在 `起點 X` + `drawerLabelWidth`。

> **自訂樣式**：`drawerLabelWidth` 與 `drawerIndentWidth` 都可以透過 `element.style` 來設定，並支援**向上層級聯繼承 (Cascading)**。只要在父容器設定一次，內部所有的 Drawer 都會自動對齊。

## 3. 事件冒泡 (Event Bubbling) 機制

ImTK 的 `RuntimeDrawer` 實作了 UI 樹的事件冒泡。
當一個繼承自 `RuntimeDrawer<T>` 的元件數值發生改變時：
1. 它會觸發自身的 `onValueChanged`。
2. 接著自動向上尋找 `parent`，只要遇到實作 `IDrawer` 的類別，就會觸發其內部的 `NotifyValueChanged()`。
3. 此機制允許你在頂層容器一次性監聽所有子欄位的變更，而不需要逐個綁定。

## 4. 複合型結構與資料綁定 (Composite Structs)

對於像 `Vector3` 這樣的 Value Type (或是 Reference Type 的內部屬性)，因為 C# 型別特性的關係，單純依賴 UI 冒泡可能無法觸發父物件 `EqualityComparer` 的改變。

我們強烈建議透過**綁定委派 (Delegation Binding)** 來建立複合型 Drawer：

```csharp
// 建立 X 軸的 FloatDrawer
var xDrawer = new FloatDrawer("X", initialValue.X);

// 在 X 軸變更時，重組 Struct 並使用 SetValueWithoutNotify 修改父層
xDrawer.RegisterValueChanged(() =>
{
    var tmp = this.value;
    tmp.X = (float)xDrawer.GetValue();
    this.SetValueWithoutNotify(tmp); // 藉由後續的冒泡機制，自動觸發父層的對外事件
});

this.Add(xDrawer);
```
這種做法確保了資料的單向流動，且避免了因為雙向綁定導致的無限迴圈或事件重複觸發。

## 5. 折疊元件 (FoldoutDrawer)

`FoldoutDrawer` 是一個負責管理顯示/隱藏的結構 Drawer。
當它被加入 `RuntimeDrawer` 體系中，它會接管並覆寫 ImGui 的 `TreeNode` 行為，並且**自動對內部的子 Drawer 實施 `indentLevel + 1`** 的縮排遞增，提供層次分明的樹狀編輯介面。
