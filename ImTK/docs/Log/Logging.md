# ImTK 日誌與偵錯系統設計 (Logging & Debugging System Architecture)

## 1. 摘要與定位 (Abstract & Scope)

對於一個健壯的框架而言，可靠的日誌系統是不可或缺的基礎設施。
`ImTKLog` 的定位是「極度輕量、極早啟動、高度擴展」的核心日誌引擎。

為解決模組啟動順序與依賴的「雞與蛋」難題，日誌系統**不繼承**自 `ImTKModule`。它的核心邏輯完全靜態且獨立，並在應用程式生命週期的最早期 (Pre-Bootstrap) 即可運作，確保在框架反射掃描或圖形環境建立崩潰時，依然能忠實記錄錯誤。

---

## 2. 架構職責切分 (Architecture & Separation of Concerns)

為了兼顧「輕量化」、「模組過濾」與「開發者體驗 (DX)」，ImTK 日誌系統採用四層架構：

### 2.1 上下文環境：`LogContext` (發送端)
開發者不直接呼叫靜態類別寫死字串，而是透過建立實例化的 `LogContext` 來封裝環境資訊（例如模組名稱）。
*   **職責**：將日誌蓋上「模組印章」，然後交給 Manager。
*   **優勢**：方便實作特定模組的日誌過濾，且提供豐富的語法糖。

```csharp
public class LogContext
{
    public string ModuleName { get; }
    public LogContext(string moduleName) => ModuleName = moduleName;

    public void Info(string message) { /* 封裝成 Entry 並交給 Manager */ }

    // 語法糖：條件式日誌，搭配 C# 內插字串處理器達到零效能開銷
    public void InfoIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message) { ... }
}
```

### 2.2 日誌資料實體：`LogEntry` (載體)
這是系統中流動的資料本體。為了降低 GC 壓力與維持多執行緒傳遞的高效能，設計為 `readonly struct`。
*   **特性**：放棄了複雜的格式化字串陣列 (object[]) 與訊息折疊功能，回歸單純的字串傳遞，以換取極致的輕量與執行緒安全。

```csharp
public readonly struct LogEntry
{
    public readonly DateTime Timestamp;
    public readonly LogLevel Level;
    public readonly string ContextName; // 來源模組
    public readonly string Message;
    public readonly Exception Exception;
}
```

### 2.3 核心分派器：`ImTKLog` (Manager)
唯一的靜態全域入口。
*   **職責**：管理所有的輸出端 (`ILogSink`)，接收 `LogContext` 傳來的 `LogEntry` 並安全地分派。
*   **全域過濾**：已拔除全域過濾機制。過濾職責完全交由各個 `ILogSink` 自行處理，以提供更高的靈活性（例如：Console 顯示 Warning 以上，File 記錄 Trace）。

### 2.4 終端輸出：`ILogSink` (接收端)
借鑑 Serilog 等現代框架的術語，Sink 代表「日誌資料最終流入並被處理的水槽」。
*   **實作**：如 `ConsoleSink`, `FileSink`, `MemoryRingBufferSink`。

---

## 3. `ILogSink` 介面與擴展性設計 (Extensibility)

為確保未來的開發者能輕鬆擴充自訂的輸出端（如寫入 Discord Webhook），系統採用「管線與過濾 (Pipeline & Filter)」設計，提供一個虛擬基底類別 `LogSinkBase`。

### 3.1 `LogSinkBase` 基底類別
此類別封裝了通用的「層級過濾」、「模組排除 (黑名單)」與「格式化 (Formatting)」邏輯。

```csharp
public abstract class LogSinkBase : ILoggerSink
{
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
    public HashSet<string> ExcludedContexts { get; } = new HashSet<string>();

    // 格式化委派：提供絕對自由度的樣板，可透過 LogFormatterBuilder 建立
    public Func<LogEntry, string> Formatter { get; set; }

    public void Emit(LogEntry entry)
    {
        if (entry.Level < MinimumLevel) return;
        if (ExcludedContexts.Contains(entry.ContextName)) return;

        string formattedMsg = Formatter(entry);
        WriteToTarget(formattedMsg, entry); // 交由子類實作具體 I/O
    }

    protected abstract void WriteToTarget(string formattedMsg, LogEntry originalEntry);
}
```

### 3.2 基於 Builder 的樣板格式化 (Builder-based Formatting)
透過鏈式調用的 `LogFormatterBuilder` 來定義輸出的格式，並自動整合對 Exception 的處理。利用內部的 `StringBuilder` 降低字串連接的記憶體開銷。

```csharp
var formatter = new LogFormatterBuilder()
    .Timestamp()     // 預設為 [HH:mm:ss]
    .Level()         // 預設為 [Level]
    .ContextName()   // 預設為 [ContextName]
    .Text(" ")       // 補一個空白
    .Message()       // 輸出訊息本體 (若有 Exception 則一併輸出)
    .Build();

var consoleSink = new ConsoleSink() { Formatter = formatter };
```

---

## 4. UI 整合與執行緒安全 (UI Integration & Thread Safety)

既然 ImTK 是一個 UI 框架，日誌系統最終必須提供視覺化的除錯介面（如內建的 `ImTKConsoleWindow`）。

### 4.1 Memory Ring Buffer Sink
*   專為 UI 渲染設計的 Sink。
*   在記憶體中維護固定大小的佇列（例如保留最新 1000 筆 LogEntry），避免無限增長。

### 4.2 執行緒安全防護
背景執行緒（如下載任務）會頻繁呼叫日誌，而 ImGui 渲染嚴格綁定主執行緒。
*   **寫入防護**：`ImTKLog` 向 Sink 分派日誌，以及 Sink 寫入內部集合時，必須具備多執行緒安全機制（如使用 `ConcurrentQueue` 或 `lock`）。
*   **讀取防護**：`ImTKConsoleWindow` 在主執行緒 `OnGuiRender` 讀取 Memory Sink 資料並繪製時，需確保不因背景寫入而引發 `Collection was modified` 例外。通常透過在 `OnLogicUpdate` 階段將日誌拷貝到 UI 專用緩衝區來達成。