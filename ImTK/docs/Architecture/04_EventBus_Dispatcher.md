# ImTK 事件匯流排與執行緒調度 (Event Bus & Dispatcher Architecture)

## 1. 摘要與背景 (Abstract & Background)

在現代應用程式開發中，模組間的解耦 (Decoupling) 與背景非同步任務 (Async Background Tasks) 是常態。
若無統一的事件傳遞機制，模組之間會產生強依賴；若無執行緒調度機制，背景任務更新 ImGui 介面時將導致程式崩潰。

為此，ImTK 引入了 `ImTKDispatcher` (主執行緒調度器) 與 `ImTKEventBus` (型別安全事件匯流排)。
本設計特別強調了**「開發體驗 (DX)」**與**「記憶體安全」**，透過與 ImTK 核心生命週期的深度整合，徹底解決了傳統 C# 事件訂閱極易造成的 Memory Leak 與過早回收 (Premature GC) 問題。

---

## 2. 執行緒安全防護：`ImTKDispatcher`

ImGui 的渲染指令與 OpenGL 的資源配置嚴格綁定於「主執行緒 (Main Thread)」。
`ImTKDispatcher` 的職責是提供跨執行緒的防護與切換能力。

### 2.1 核心機制
*   **主執行緒註冊**：在應用程式極早期 (Phase 1)，系統會記錄啟動執行緒的 ID 作為 `MainThreadId`。
*   **安全檢查 API**：提供 `ImTKDispatcher.IsMainThread` 供 UI 模組或 OpenGL 呼叫前進行 `Assert` 防呆。
*   **非同步推播 (Enqueue)**：允許背景執行緒（如 `Task.Run` 中的網路下載器）將一段 UI 更新邏輯推入佇列。
    ```csharp
    ImTKDispatcher.Enqueue(() => { myWindow.UpdateProgress(100); });
    ```
*   **生命週期整合執行**：在全域生命週期的 `OnLateUpdate` 階段，由主執行緒安全地消耗並執行佇列中的所有 `Action`。

---

## 3. 型別安全事件匯流排：`ImTKEventBus`

為了實現模組間的鬆耦合通訊，ImTK 採用基於型別 (Type-based) 的訊息路由，而非字串 (String-based)。

### 3.1 為什麼選擇「型別安全」？
*   **優勢**：享有編譯期檢查 (Compile-time Check)、IDE 重新命名與重構支援。不會因為字串打錯而引發找不到事件的 Bug。
*   **實作約束**：所有事件資料實體必須實作空介面 `IImTKEvent`，作為路由的憑證。

```csharp
// 事件定義範例
public struct FileLoadedEvent : IImTKEvent
{
    public string FilePath;
    public byte[] Data;
}
```

### 3.2 發布與訂閱的執行緒保障
`ImTKEventBus` 內部深度整合了 `ImTKDispatcher`。
當開發者訂閱事件時，系統預設確保該 Handler **必定在主執行緒上執行**。這表示即使事件是由背景執行緒發布 (`Publish`)，接收端的 UI 模組也能安全地直接操作介面，無須自行 `Invoke`。

---

## 4. 記憶體洩漏防護：自動解綁架構 (Implicit Unsubscription)

這是 ImTK 事件系統最具巧思的設計。
傳統事件系統常面臨兩難：
1.  **強參照 (Strong Reference)**：開發者容易忘記 `Unsubscribe`，導致 UI 物件永遠無法被 GC 回收 (Memory Leak)。
2.  **弱參照 (Weak Reference 搭配 Token)**：若開發者忘記將 `IDisposable` Token 存入類別欄位，匿名委派會被 GC 提早回收，導致事件神秘失效 (Premature GC)。

### 4.1 解決方案：生命週期隱式綁定 (Lifecycle Binding)

ImTK 放棄了危險的 Token 弱參照模式，改採**「內部強參照 + 生命週期自動清理」**的策略。

1.  **限制直接存取**：開發者**不應直接呼叫**全域的 `ImTKEventBus.Subscribe()`。
2.  **基底類別代理**：所有的事件訂閱，必須透過掛載於全域生命週期樹上的基底類別（`ImTKModule` 或 `ImTKObject`，乃至未來的 `VisualElement`）提供的 `SubscribeEvent<T>` 方法進行。
3.  **自動清理機制**：
    基底類別內部維護一個私有的取消訂閱清單 (`List<Action> _myEventUnsubscribers`)。
    當該模組或物件被關閉/銷毀（進入 `OnDisable` 生命週期階段）時，基底類別會自動遍歷並執行所有的取消註冊動作。

### 4.2 實作與使用範例

**框架底層 (ImTKObject 基底類別實作)：**
```csharp
public abstract class ImTKObject
{
    private readonly List<Action> _eventUnsubscribers = new();

    // 給子類別呼叫的安全註冊 API
    protected void SubscribeEvent<T>(Action<T> handler) where T : IImTKEvent
    {
        // 向全域 Bus 進行強參照註冊，並取得註銷用的委派
        Action unsub = ImTKEventBus.GlobalSubscribe(handler);
        _eventUnsubscribers.Add(unsub);
    }

    public virtual void OnDisable()
    {
        // 框架保證在物件失效時，絕對乾淨地清空所有訂閱，零 Memory Leak
        foreach(var unsub in _eventUnsubscribers) unsub();
        _eventUnsubscribers.Clear();
    }
}
```

**開發者應用端 (極致簡潔的 DX)：**
```csharp
public class MyUIWindow : ImTKObject
{
    public override void OnEnable()
    {
        // 隨意使用 Lambda，不怕提早回收，也不用存 Token
        SubscribeEvent<FileLoadedEvent>(e => {
            this.StatusText = $"Loaded: {e.FilePath}";
        });
    }

    // 開發者完全不需要覆寫 OnDisable 去手動 Unsubscribe！
}
```

---

## 5. 總結

透過將 `ImTKDispatcher` 與基於 `ImTKObject/Module` 生命週期的「自動解綁機制」結合，
ImTK 的 Event Bus 達成了 **型別安全 (Type-Safe)**、**執行緒安全 (Thread-Safe)** 與 **零負擔防洩漏 (Zero-Boilerplate Leak Proof)** 的三大目標。
這為後續構建複雜的 UI 框架（如 VisualElement 間的通訊）奠定了最穩固的底層基礎。