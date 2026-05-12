# 資源與資料庫系統設計藍圖 (Asset & Database Architecture)

## 1. 摘要與定位 (Abstract & Scope)

在小型工具與應用程式開發中，頻繁處理檔案讀寫、使用者設定儲存與 GPU 資源（如圖片）是一大痛點。
ImTK 採用「職責分離與實例化範圍」的雙軌架構，將資源管理劃分為兩大靜態入口：`Resource` (唯讀全域) 與 `ImTKDatabase` (可讀寫本地)。

**核心定位**：
*   **不追求「無所不知」的泛用型推斷**：放棄 Unity 般龐大且複雜的 `Importer / Loader` 註冊系統。
*   **物件導向封裝**：將檔案的序列化與反序列化邏輯歸還給資源物件本身（透過覆寫虛擬方法 `OnLoad` 與 `OnSave`）。
*   **執行期快取與生命週期安全**：專注於管理已載入記憶體中的離散資源，確保單一路徑的實例唯一性，並透過 `DatabaseModule` 在系統關閉時安全釋放 C++ 指標。

---

## 2. 雙軌靜態 API 與環境控管 (Dual API & Environment Control)

為解決多開發者協作、多設備存取以及發布模式下權限不同的問題，ImTK 引入了 `ImTKEnvironment` 進行路徑控管，並將操作 API 嚴格區分為唯讀與讀寫。

### 2.1 ImTKEnvironment (環境變數控管)
透過靜態的 `ImTKEnvironment`，開發者可以設定應用程式名稱與開發狀態。
*   `GlobalAssetPath`: 預設指向 `AppDomain.CurrentDomain.BaseDirectory` (執行檔所在目錄)。
*   `LocalDataPath`: 預設指向作業系統的 `%AppData%/{OrganizationName}/{ApplicationName}`。

### 2.2 Resource (唯讀全域資源)
*   **用途**：載入隨應用程式打包發布的固定資源（如預設 Icon、語言檔範本、主題設定）。
*   **行為限制**：只開放 `GetAsset<T>`。不提供任何建立或寫入的方法，從根本杜絕了在正式環境中觸發 `UnauthorizedAccessException` 的可能性。
*   **存取點**：受限於 `ImTKEnvironment.GlobalAssetPath`。

### 2.3 ImTKDatabase (可讀寫本地資料庫)
*   **用途**：管理應用程式執行期間產生的資料，或使用者自訂的偏好設定。
*   **API 提供**：`GetAsset<T>`, `CreateAsset<T>`, `GetOrCreateAsset<T>`, `MarkDirty()`, `SaveAssets()`。
*   **存取點**：受限於 `ImTKEnvironment.LocalDataPath`。

---

## 3. 核心架構與 API 設計 (Core Interface & Base Classes)

### 3.1 強制泛型載入 (Strict Generic Loading)

捨棄在檔案內部加入 `$type` 或依賴副檔名推斷的做法，採用強制傳入泛型 `T` 的載入模式：

```csharp
var myConfig = ImTKDatabase.GetOrCreateAsset<AppConfig>("config.json");
var myIcon = Resource.GetAsset<Texture2D>("assets/icon.png");
```

這使開發者保有極致的自由度，且路徑與型別在記憶體快取中具有**絕對唯一性保證**。若對同一個路徑呼叫不同型別，會拋出 `AssetTypeMismatchException`。

### 3.2 資源基底類別 (Asset Base Classes)

為避免複雜的外部 Loader 系統，ImTK 要求開發者自訂的資源類別直接繼承對應的基底類別，並自行處理序列化邏輯：

*   **`IAsset` / `ImTKAsset`**:
    *   基礎唯讀資源。實作 `IDisposable` 以處理如 GPU 紋理的釋放。
    *   需覆寫 `protected abstract void OnLoad(string absolutePath);`。
*   **`ISaveableAsset` / `ImTKSaveableAsset`**:
    *   可寫回磁碟的資源。
    *   需額外覆寫 `protected abstract void OnSave(string absolutePath);`。

### 3.3 預設實作：JsonAsset&lt;T&gt;

ImTK 內建提供了 `JsonAsset<T>` 來簡化純資料物件 (POCO) 的存檔需求。它使用 `System.Text.Json` 進行序列化，並將資料封裝在 `Data` 屬性中。

```csharp
// 1. 定義你的資料類別 (必須有 public 無參數建構子)
public class AppConfig
{
    public int WindowWidth { get; set; } = 1024;
    public string Theme { get; set; } = "Dark";
}

// 2. 獲取或建立資源
var configAsset = ImTKDatabase.GetOrCreateAsset<JsonAsset<AppConfig>>("config.json");

// 3. 讀寫資料
Console.WriteLine(configAsset.Data.Theme);
configAsset.Data.WindowWidth = 1920;

// 4. 標記修改並存檔
configAsset.MarkDirty();
ImTKDatabase.SaveAssets(); // 或交由系統關閉時自動儲存
```

---

## 4. 記憶體管理與安全防護 (Memory & Safety)

### 4.1 IsDisposed 安全標記防護
所有資源實作 `IAsset` 介面，其中包含 `IsDisposed` 屬性。當資源被 `Unload()` 且底層指標被釋放後，其 `IsDisposed` 將被設為 `true`。
任何持有該資源參照的 UI 元件（如 `TextureView` 或綁定資料的 Drawer），在渲染前**必須**檢查 `if (asset.IsDisposed) return;`，以防止將失效的指標送入 GPU 導致崩潰。

### 4.2 狀態同步與 Version 機制
當設定檔透過 `ImTKDatabase.MarkDirty(asset)` 標記為需存檔時，系統會自動將該資源的 `Version++`。
UI 元件可透過輕量的整數比對來實現零訂閱 (Zero-Subscription) 的高效狀態同步，避免傳統 C# event 常見的 Memory Leak。

### 4.3 內部核心：AssetManager
`Resource` 和 `ImTKDatabase` 內部實際上都封裝了一個 `AssetManager` 實例。這個管理器負責路徑的標準化（如防禦絕對路徑）、快取字典的管理以及防呆旗標 (`isReadOnly`) 的執行。

---

## 5. 持久化與存檔 (Persistence & Save)

針對可讀寫資源，採用「髒標記 (Dirty Flag) + 統一存檔」的策略：

1.  當透過 UI 修改資源數值時，呼叫 `ImTKDatabase.MarkDirty(asset)` 或 `asset.MarkDirty()`。
2.  資料**不會**立即寫入磁碟，以避免拖拉 Slider 時的效能衝擊與頻繁 I/O。
3.  開發者可透過手動點擊「儲存按鈕」，或由系統模組 (`DatabaseModule`) 在 `OnClose` 時統一呼叫 `ImTKDatabase.SaveAssets()`，將所有被標記為髒的資源一次性寫回磁碟。
