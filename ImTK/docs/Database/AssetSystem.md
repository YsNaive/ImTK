# ImTKDatabase 資源管理系統設計藍圖 (Asset System Architecture)

## 1. 摘要與定位 (Abstract & Scope)

在小型工具與應用程式開發中，頻繁處理檔案讀寫與 GPU 資源（如圖片）是一大痛點。
`ImTKDatabase` 是 ImTK 框架的次世代資源管理核心。它的設計哲學不追求如同大型遊戲引擎（如 Unity AssetDatabase）那般「無所不知」的泛用型推斷，而是專注於「執行期快取與生命週期安全」。

**核心定位**：`ImTKDatabase` 是**「執行期資源快取管理器 (Runtime Asset Cache Manager)」**。
*   它專注於管理**已載入記憶體中**的離散資源（如 `.png`, `.json`, `.config`）。
*   它**不負責**管理巨量大數據（如高頻讀寫的 `.bin` 檔案）或時間序列資料，這類需求應由獨立的資料引擎處理。
*   它**不執著於**推斷未載入檔案的型別，而是將型別決定的權力交還給開發者。

---

## 2. 核心架構與 API 設計 (Core API & Architecture)

### 2.1 強制泛型載入 (Strict Generic Loading) vs 檔案內嵌標記

在 Unity 或 Godot 中，檔案內部通常會包含 `$type` 或依賴 `.meta` GUID 來推斷反序列化型別，以支援通用的資源瀏覽器。但在 ImTK 定位的小型工具開發中，這會導致**格式綁死**與**重構脆弱**。

為解決此問題，`ImTKDatabase` 採用「強制傳入 `T`」的載入模式：

```csharp
// 開發者明確指示需要的型別與路徑
var myConfig = ImTKDatabase.Load<AppConfig>("config.json");
var myIcon = ImTKDatabase.Load<Texture2D>("assets/icon.png");
```

*   **優勢**：檔案內部不需要包含難以維護的 `$type` 資訊。開發者保有極致的自由度，可以讀取任意來源的純文字或 JSON 檔。
*   **根目錄基準**：所有載入路徑皆相對於初始化時設定的 `RootPath`，確保跨平台路徑解析的一致性。

### 2.2 外部註冊載入器 (External AssetLoader Registry)

既然檔案本身不帶型別資訊，`ImTKDatabase` 將反序列化的工作委派給註冊的 `IAssetLoader`。
*   在 `OnInitializeDependencies` 階段，各模組可向 `ImTKDatabase` 註冊支援特定副檔名或型別的載入器（如 `JsonAssetLoader<T>`, `TextureLoader`）。
*   這使得擴充對 `.ini` 或 `.xml` 等自訂格式的支援變得極為容易，貫徹了「檔案格式解耦」的設計理念。

### 2.3 絕對唯一性保證 (Absolute Uniqueness)

這是在 Retained Mode UI 中實現資料綁定 (Data Binding) 的鐵律。

*   **行為規範**：對於相同的路徑與相同的型別，`ImTKDatabase.Load<T>` **必然回傳同一個記憶體實例 (Reference)**。
*   **防呆機制**：若開發者嘗試對同一個路徑呼叫不同型別的 `Load`，系統必須拋出型別衝突例外（如 `AssetTypeMismatchException`），防止記憶體狀態不一致與難以追蹤的 Bug。

---

## 3. 記憶體管理與安全防護 (Memory & Safety)

管理包含 GPU 指標的資源（如 ImGui Texture）極易產生 Memory Leak 或是存取空指標的崩潰。`ImTKDatabase` 捨棄了複雜的「參照計數 (Ref Counting)」與依賴 C# GC 的「弱參照 (WeakReference)」，採用更適合小工具開發的「手動卸載 + 安全標記」混合防護策略。

### 3.1 為什麼不用參照計數或弱參照？
*   **參照計數**：開發者容易忘記呼叫 `Release()`，造成隱形記憶體洩漏，嚴重破壞開發者體驗 (DX)。
*   **弱參照**：依賴 C# GC (Garbage Collector) 在背景執行緒呼叫 Finalizer 去清理 OpenGL 指標是非常危險的，極易引發 Thread Crash。

### 3.2 手動卸載機制 (Explicit Unload)
資源一旦被載入，就會常駐在 `ImTKDatabase` 的字典快取中，直到：
1.  開發者明確呼叫 `ImTKDatabase.Unload(path)`，主動釋放記憶體。
2.  應用程式關閉，觸發 `ImTKModule.OnClose`，進行全域清洗。

### 3.3 `IsDisposed` 安全標記防護
所有資源必須實作 `IAsset` 介面：

```csharp
public interface IAsset : IDisposable
{
    string Path { get; }
    int Version { get; }
    bool IsDisposed { get; }
}
```

當資源被 `Unload()` 且底層指標（如 OpenGL Texture ID）被釋放後，其 `IsDisposed` 將被設為 `true`。
任何持有該資源參照的 UI 元件（如 `TextureView` 或 `RuntimeDrawer`），在渲染前**必須**檢查 `if (asset.IsDisposed) return;`。這保證了即使資源被其他系統提早卸載，UI 也不會將失效的指標送入 GPU 導致崩潰，只會優雅地顯示為空或預設狀態。

---

## 4. 狀態同步與熱更新 (Reactivity & Synchronization)

當資源被外部（如 FileSystemWatcher 監聽到檔案修改）或內部程式碼修改時，UI 必須同步更新。為避免傳統 C# `event` 造成的 Memory Leak 問題（因為 UI 節點頻繁創建與銷毀，極易忘記解除註冊），ImTK 採用高效的**版本號機制 (Version Numbering)**。

### 4.1 Version 機制運作原理
1.  `IAsset` 內部維護一個整數 `Version`。
2.  當資源內容發生變化時（例如開發者呼叫 `ImTKDatabase.MarkDirty(asset)`，或系統自動重新載入檔案），該資源的 `Version++`。
3.  依賴該資源的 UI 元件，自己記住上一次的 `lastSeenVersion`。
4.  利用 ImGui Immediate Mode 的特性，在每幀的迴圈中進行極輕量的整數比對：

```csharp
// 位於 UI 渲染元件內
if (m_asset.Version != m_lastVersion)
{
    RefreshInternalState(); // 執行昂貴的更新（如重新產生 UI 內容或重新綁定 GPU ID）
    m_lastVersion = m_asset.Version;
}
```

*   **優勢**：零訂閱、徹底免除 Event Leak 風險、效能極佳（每幀僅進行整數比對）。

---

## 5. 持久化與存檔 (Persistence & Save)

針對設定檔或資料物件的儲存，採用「髒標記 (Dirty Flag) + 統一存檔」的策略，借鑑 Unity 的 `SaveAssets` 模式：

1.  當透過 UI (Drawer) 修改資源數值時，呼叫 `ImTKDatabase.MarkDirty(asset)`。
2.  資料**不會**立即寫入磁碟以避免拖慢拖拉 Slider 時的效能與頻繁 I/O。
3.  開發者可透過手動點擊「儲存按鈕」，或由框架在關閉前 (`OnClose`) 或特定的 `OnLateUpdate` 時機，統一呼叫 `ImTKDatabase.SaveAssets()`，將所有被標記為髒的資源一次性寫回磁碟。