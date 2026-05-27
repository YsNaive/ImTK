# 資源與資料庫系統設計藍圖 (Asset & Database Architecture)

## 1. 摘要與定位 (Abstract & Scope)

在小型工具與應用程式開發中，頻繁處理檔案讀寫、使用者設定儲存與 GPU 資源（如圖片）是一大痛點。
ImTK 採用「職責分離 (Importer 模式)」與「雙軌資料庫」架構，將資源管理劃分為兩大靜態入口：`Resource` (唯讀全域) 與 `ImTKDatabase` (可讀寫本地)。

**核心定位**：
*   **純資料容器 (Passive Data Containers)**：`ImTKAsset` 本身不包含任何讀寫 (`OnLoad` / `OnSave`) 邏輯，開發者自訂的設定檔將變成純淨的 POCO。
*   **Type-based 解析器與 Fail-Fast**：棄用模糊的副檔名推斷，採用「基於請求型別」的明確註冊表。找不到對應的解析器就報錯，不提供暗箱 Fallback。
*   **執行期快取與生命週期安全**：專注於管理已載入記憶體中的離散資源，確保單一路徑的實例唯一性，並透過 `DatabaseModule` 在系統關閉時安全釋放資源。

---

## 2. 雙軌靜態 API 與環境控管 (Dual API & Environment Control)

為解決多開發者協作、多設備存取以及發布模式下權限不同的問題，ImTK 引入了 `ImTKEnvironment` 進行路徑控管，並將操作 API 嚴格區分為唯讀與讀寫。

### 2.1 ImTKEnvironment (環境變數控管)
透過靜態的 `ImTKEnvironment`，採用 **延遲載入 (Lazy Evaluation)** 與 **零設定反射**，預設自動汲取專案中的 `[AssemblyCompany]` 與 `[AssemblyProduct]`。
*   `IsDevelopment`: 自動讀取 `[AssemblyConfiguration]` 判斷是否為 Debug 編譯。
*   `GlobalAssetPath`: 預設指向 `AppDomain.CurrentDomain.BaseDirectory` (執行檔所在目錄)。
*   `DevelopmentLocalDataPath`: 開發模式下的路徑隔離。若設定，測試時產生的設定檔就不會污染系統正式的 AppData。
*   `LocalDataPath`: 預設指向作業系統的 `%AppData%/{CompanyName}/{ApplicationName}`。

### 2.2 Resource (唯讀全域資源)
*   **用途**：載入隨應用程式打包發布的固定資源（如預設 Icon、語言檔範本、主題設定）。
*   **行為限制**：經由此入口載入的 `ImTKAsset` 將被強制注入 `IsReadOnly = true`。呼叫 `MarkDirty()` 將引發安全例外，從根本杜絕覆寫官方檔案的可能。
*   **存取點**：受限於 `ImTKEnvironment.GlobalAssetPath`。

### 2.3 ImTKDatabase (可讀寫本地資料庫)
*   **用途**：管理應用程式執行期間產生的資料，或使用者自訂的偏好設定。
*   **行為**：載入的 `ImTKAsset` 之 `IsReadOnly = false`。可透過 `MarkDirty()` 追蹤變更並寫回磁碟。
*   **存取點**：受限於 `ImTKEnvironment.LocalDataPath` (或 Development 隔離路徑)。

---

## 3. 核心架構與 API 設計 (Core Importer Pattern)

### 3.1 極簡的 Load&lt;T&gt; 對外介面
API 已全面精簡。不論是圖片、純讀 JSON 或可覆寫設定檔，開發者皆只使用單一入口：

```csharp
var myConfig = ImTKDatabase.Load<GameConfig>("config.json");
var myIcon = Resource.Load<Texture2D>("assets/icon.png");
```

「檔案遺失時是直接報錯，還是自動產生一份帶預設值的檔案」這項決策，完全交由底層註冊的 **Importer 實作** 來決定。

### 3.2 唯一的資源基底：ImTKAsset
萬物皆為 `ImTKAsset`。不再區分是否可儲存，資源是否可被寫回磁碟，取決於**系統中有沒有為它註冊 Exporter**，以及**它是否被定義為唯讀**。

### 3.3 解析器系統 (IAssetImporter / IAssetExporter)
所有的 I/O 邏輯皆隔離在 `ImTK.Database.Importers` 中：
*   實作 `IAssetImporter<T>` 提供 `Import(absolutePath, normalizedPath)`。
*   實作 `IAssetExporter<T>` 提供 `Export(asset, absolutePath)`。

### 3.4 註冊表機制 (Registry)
AssetManager 內部維護 Type-based 字典。系統支援開放式泛型註冊：
```csharp
// 為特定的型別註冊專屬處理器
ImTKDatabase.RegisterImporter(typeof(Texture2D), new TextureImporter());

// 為泛型容器註冊開放式處理器
ImTKDatabase.RegisterImporter(typeof(JsonAsset<>), typeof(StrictJsonImporter<>));
```
遇到未註冊的型別，將嚴格拋出 `AssetImporterNotFoundException`。

---

## 4. 記憶體管理與安全防護 (Memory & Safety)

### 4.1 唯讀防護與 Dirty 標記
開發者修改資料後，**必須主動宣告**這份資料髒了：
```csharp
config.MarkDirty(); // 遞增 Version，並標記 IsDirty = true
```
若 `config.IsReadOnly == true`，此方法將直接拋出 `InvalidOperationException`。

### 4.2 IsDisposed 安全標記防護
當資源被 `Unload()` 且底層指標被釋放後，其 `IsDisposed` 將被設為 `true`。
任何持有該資源參照的 UI 元件（如 `TextureView` 或綁定資料的 Drawer），在渲染前**必須**檢查 `if (asset.IsDisposed) return;`，以防止將失效的指標送入 GPU 導致崩潰。

### 4.3 狀態同步與 Version 機制
呼叫 `MarkDirty()` 會自動將該資源的 `Version++`。UI 元件可透過輕量的整數比對來實現零訂閱 (Zero-Subscription) 的高效狀態同步，避免傳統 C# event 常見的 Memory Leak。

---

## 5. 持久化與存檔 (Persistence & Save)

針對可讀寫資源，採用「髒標記 (Dirty Flag) + 統一存檔」的策略：

1.  當透過 UI 修改資源數值時，呼叫 `asset.MarkDirty()`。
2.  資料**不會**立即寫入磁碟，以避免拖拉 Slider 時的效能衝擊與頻繁 I/O。
3.  開發者可手動呼叫 `ImTKDatabase.SaveAssets()`，或由系統模組 (`DatabaseModule`) 在 `OnClose` 時統一呼叫，將所有髒資源透過對應的 Exporter 寫回磁碟。

## 6. 資源例外處理 (AssetExceptions)
系統具備嚴謹的例外邊界：
*   **`AssetImporterNotFoundException` / `AssetExporterNotFoundException`**：型別未註冊。
*   **`AssetTypeMismatchException`**：快取的資源型別與本次請求的泛型型別不一致。
*   **`AssetPathInvalidException`**：路徑包含非法格式或嘗試進行路徑穿越攻擊時拋出。
