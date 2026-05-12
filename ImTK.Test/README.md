# ImTK Test Framework

ImTK 提供了一套輕量、無外部依賴的測試框架，讓開發者可以在開發過程中驗證底層邏輯與 UI 生命週期。
測試分為兩種層級：**Headless 測試** (無 UI) 與 **Integration 整合測試** (含 UI 生命週期)。

## 目錄結構規範 (Mirror Directory Structure)

為了維持測試專案的可維護性，`ImTK.Test` 的目錄結構必須**嚴格對齊 (Mirror)** 核心庫 `ImTK` 的目錄結構。
例如：
- 測試 `ImTK/Core/Lifecycle.cs` -> `ImTK.Test/Core/LifecycleTests.cs`
- 測試 `ImTK/UI/Event/EventDispatcher.cs` -> `ImTK.Test/UI/Event/EventDispatcherTests.cs`

## 1. Headless 測試 (IHeadlessTest)

此類測試用於驗證不需要依賴圖形渲染與 ImGui 狀態的底層邏輯（如資料結構、路徑解析）。
這些測試將在 `ImTKSilk.Run()` 啟動視窗主迴圈**之前**被執行，並於終端機輸出報表。

**如何建立一個 Headless 測試：**
1. 建立一個類別並實作 `IHeadlessTest` 介面。
2. 實作 `Run()` 方法。
3. 使用 `ImTKAssert` 來進行驗證。

```csharp
using ImTK.Test.Framework;

namespace ImTK.Test.Core
{
    public class MathTests : IHeadlessTest
    {
        public void Run()
        {
            int result = 1 + 1;
            ImTKAssert.AreEqual(2, result, "Basic math should work.");
        }
    }
}
```
*HeadlessRunner 會利用 Reflection 自動掃描並執行這個測試。*

## 2. UI 整合測試 (IIntegrationTest)

此類測試用於驗證必須在框架正常運轉下才能測試的行為（如 `ImTKModule` 的 `OnInitialize` 觸發、`VisualElement` 事件冒泡）。
這些測試將在 UI 啟動後，由內建的 `TestRunnerModule` 接管執行，結果將顯示於 UI 的 Test Report 面板中。

**如何建立一個 Integration 測試：**
1. 建立一個類別並實作 `IIntegrationTest` 介面。
2. 提供 `TestName` 與 `IsManualOnly` 屬性。若測試過於繁重，請將 `IsManualOnly` 設為 `true`。
3. 實作 `Run()` 方法。

```csharp
using ImTK.Test.Framework;

namespace ImTK.Test.UI
{
    public class EventBubbleTests : IIntegrationTest
    {
        public string TestName => "Event Bubble Logic";
        public bool IsManualOnly => false; // 設為 false 會在啟動時自動執行

        public void Run()
        {
            // 在此處撰寫依賴 ImTK 生命週期或視覺樹的測試邏輯
            ImTKAssert.IsTrue(true, "Mock logic.");
        }
    }
}
```

## 3. ImTKAssert 斷言庫

為了確保錯誤能與 `ImTKLog` 系統整合，請一律使用 `ImTKAssert` 來進行結果驗證：
- `ImTKAssert.IsTrue(condition, msg)`
- `ImTKAssert.IsFalse(condition, msg)`
- `ImTKAssert.AreEqual(expected, actual, msg)`
- `ImTKAssert.NotNull(obj, msg)`
- `ImTKAssert.Throws<TException>(action, msg)`

若斷言失敗，會自動觸發 `ImTKLog.Error` 並拋出例外，這將被 Runner 捕獲並記錄為測試失敗。
