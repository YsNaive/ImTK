# 資料庫與資源存取 (Database & Assets)

本模組為 ImTK 框架提供了一套隔離的雙軌資源管理系統，負責統一處理設定檔、存檔與靜態唯讀資源的存取，並透過 Importer / Exporter 模式將 IO 邏輯徹底解耦。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 雙軌資源管理入口 (Asset Managers)

框架為了防止全域樣式被意外覆寫，實作了嚴格的唯讀/讀寫隔離。

*   **[`ImTKDatabase`](../../Database/ImTKDatabase.cs)**: **本地可讀寫資料庫**。
    *   用於存取使用者設定、工作區快取等。
    *   載入的資源 `IsReadOnly = false`。
    *   **核心 API**：`ImTKDatabase.Load<T>(path)`, `ImTKDatabase.SaveAssets()`。
*   **[`Resource`](../../Database/Resource.cs)**: **全域唯讀資源庫**。
    *   用於載入不可變更的全域資源（如預設圖示、共用樣式表）。
    *   載入的資源強制設定為 `IsReadOnly = true`，呼叫 `MarkDirty` 會拋出例外。
    *   **核心 API**：`Resource.Load<T>(path)`。

### 2. 資源實體 (Asset Entities)

在 ImTK 中，萬物皆為資料載體 (POCO Container)。

*   **[`IAsset`](../../Database/IAsset.cs)**: 所有資源必須實作的基礎介面。提供 `IsDirty`, `IsReadOnly`, `Version`, `MarkDirty()` 等屬性。
*   **[`ImTKAsset`](../../Database/ImTKAsset.cs)**: 最常被繼承的資源基底類別，實作了髒標記管理。開發者撰寫的設定檔實體應繼承此類別。

### 3. 資源解析與匯出器 (Importers & Exporters)

支援註冊各種副檔名或型別的解析方式。

*   **[`IAssetImporter<T>` / `IAssetExporter<T>`](../../Database/Importers/IAssetImporter.cs)**: 自訂解析器必須實作的介面。
*   **[`JsonAssetHandler<T>`](../../Database/Importers/JsonAssetHandler.cs)**: 內建的標準 JSON 雙向解析器 (嚴格模式，找不到檔案即拋出異常)。
*   **[`FallbackJsonAssetHandler<T>`](../../Database/Importers/JsonAssetHandler.cs)**: 內建的寬鬆 JSON 解析器。若檔案損毀或不存在，會自動實例化一個帶有預設值的物件並觸發自動存檔 (非常適合應用在偏好設定檔)。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討底層設計與快取機制的技術文件：

*   **[`AssetSystem.md`](AssetSystem.md)**：深入探討為何要採用 Importer 解耦架構、`Version` 機制的實作細節，以及資料庫如何透過 WeakReference 與快取避免重複載入與記憶體浪費。
