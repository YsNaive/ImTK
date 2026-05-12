# Button (按鈕)

`Button` 是 ImTK 基礎的互動元件，封裝了 `ImGui.Button` 並結合了框架的事件系統。

## 核心設計理念

* **生命週期安全 (Lifecycle Safety)**：在原生的 ImGui 中，按鈕點擊的回傳值 (`true`) 是在 `GuiRender` 階段同步發生的。若開發者在該 if 區塊內進行了新增或刪除節點的操作，將會引發 `VisualElementHierarchy` 的 `CheckSafeState` 防護機制。
* **延遲派發 (Deferred Dispatch)**：為了保護視覺樹的完整性，`Button` 會在被點擊時觸發 `ClickEvent`，此事件會被放入 `EventDispatcher` 佇列中，並延後至下一個 `LogicUpdate` 階段才實際執行所註冊的 Callback 函數。

## 語法與使用方式

### 1. 初始化與事件綁定

`Button` 提供了便利的建構函式，允許你在建立時直接設定文字標籤與點擊回呼 (Callback)。

```csharp
using ImTK.UI;

// 建立一個按鈕，並於建構時直接綁定事件
Button myButton = new Button("Click Me!", evt =>
{
    ImTKLog.Info("Button was clicked safely in LogicUpdate!");
});
```

### 2. 使用事件包裝器 (Event Wrapper)

除了使用 `RegisterCallback<ClickEvent>(...)`，你也可以透過 C# 的 `event` 語法糖 `onClicked` 來更直覺地管理訂閱：

```csharp
Button submitBtn = new Button("Submit");

Action<ClickEvent> onSubmit = evt => { /* do something */ };

// 訂閱事件
submitBtn.onClicked += onSubmit;

// 取消訂閱
submitBtn.onClicked -= onSubmit;
```

## 類別規格

* **父類別**：`VisualElement`
* **觸發事件**：`ClickEvent`
* **主要屬性**：
  * `text` (`string`): 顯示於按鈕上的文字標籤。
  * `onClicked` (`event Action<ClickEvent>`): 對應 `ClickEvent` 的註冊介面。
