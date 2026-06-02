# 核心生命週期與基礎架構 (Core)

本模組為 ImTK 框架最底層的引擎驅動核心，負責啟動應用程式、建立全域單例模組，以及嚴格管控所有物件的雙層生命週期狀態機。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 核心狀態機與進入點 (State Machine)

*   **[`ImTKApplication`](../../Core/ImTKApplication.cs)**: 全域的應用程式進入點與狀態機引擎。負責驅動所有階段的 Update 與 Render。
*   **[`ApplicationState`](../../Core/ApplicationState.cs)**: 列舉了框架嚴謹的生命週期階段 (如 `InitializeSelf`, `InitializeDependencies`, `Enable`, `LogicUpdate`, `GuiRender` 等)。

### 2. 生命週期基底類別 (Lifecycle Bases)

開發者編寫的邏輯或系統皆須繼承以下兩者之一：

*   **[`ImTKModule`](../../Core/ImTKModule.cs)**: **系統級全域單例**。
    *   **限制**：必須具有唯一、無參數且非公開 (private/protected) 的建構函式，由系統啟動時反射生成。
*   **[`ImTKObject`](../../Core/ImTKObject.cs)**: **動態邏輯物件**。
    *   **特性**：類似 Unity 的 GameObject，支援在執行期動態實例化。內建 `SubscribeEvent<T>` 代理機制，可自動於 `OnDisable` 階段解除事件綁定，實現零記憶體洩漏 (Leak-Proof)。

### 3. 基礎服務與工具 (Core Services)

*   **[`Time`](../../Core/Time.cs)**: 全域時間管理器。
    *   提供 `Time.DeltaTime`, `Time.UnscaledDeltaTime`, `Time.TotalTime` 等精準的畫格推進時間，並支援 `TimeScale` 慢動作或暫停。
*   **[`NativeUtf8Buffer`](../../Core/NativeUtf8Buffer.cs)**: 記憶體安全的底層字串指標分配器。
    *   為符合架構規範（嚴禁使用 `fixed` 直接鎖定 C# 字串陣列），當需要傳遞字串指標給 ImGui 底層時，應使用此類別以防 GC 回收造成懸空指標 (Dangling Pointers)。
*   **[`ImTKProfiler`](../../Core/ImTKProfiler.cs)**: 內建的效能分析探測器與時間測量工具。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討底層驅動與狀態機機制的技術文件：

*   **[`Global_Architecture.md`](Global_Architecture.md)**：從 OS 執行檔啟動到完全退出的全域巨觀生命週期藍圖。
*   **[`Lifecycle.md`](Lifecycle.md)**：深入探討雙層架構設計。詳解 `ImTKApplication` 狀態機防護機制 (防重入)，以及 `ImTKModule` 與 `ImTKObject` 在各個階段 (Hook) 的觸發順序與實作建議。
