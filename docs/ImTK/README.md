# ImTK 核心框架

ImTK 的核心模組負責將 ImGui (Immediate Mode GUI) 封裝為 C# / .NET 生態中常見的 Retained Mode 物件導向架構。

## 核心機制：自動模組管理系統 (`ImTKModule`)

在多數的 ImGui 應用中，開發者必須在一個巨大的迴圈中手動呼叫各個元件的 Update 和 Render。ImTK 徹底解決了這個問題，提供了一個基於反射的全自動生命週期框架。

### 生命週期階段
1. **發現與註冊 (`InitializeAll`)**：
   在程式啟動 (`ImTKSilk.Initialize`) 時，系統會透過 `AppDomain.CurrentDomain.GetAssemblies()` 掃描所有組件。
   它會尋找所有繼承自 `ImTKModule` 的非抽象類別（通常設計為帶有私有建構子的 private nested class），使用 `Activator.CreateInstance` 實例化並統一註冊。

2. **初始化 (`OnLoad`)**：
   當 Silk 視窗與 ImGui Context 準備完畢後呼叫。適合進行狀態反序列化或字型載入。

3. **主迴圈 (`Update` & `Render`)**：
   每幀由引擎自動驅動。在這裡，開發者呼叫自訂的 `VisualElement` 樹的更新與算圖。

4. **關閉 (`OnClose`)**：
   程式結束前呼叫。適合儲存狀態 (`SaveWindowState`) 與釋放底層指標資源。

### 架構意圖與優勢
此設計極大化地解耦了系統元件。例如 `Window` 類別可以擁有自己的私有 `Module` 來處理 `window_state.json`，而不需要在全域的 `Program.cs` 中留下任何痕跡。這讓 ImTK 的各項功能可以做到真正的「隨插即用 (Plug-and-Play)」。

## 欄位元素介面 (`IFieldElement` 與 `FieldElement<T>`)

針對資料輸入，ImTK 提供了統一的介面架構：
* **`IFieldElement`**：非泛型的基本介面，讓框架能夠統一調用 `RegisterValueChanged` 與 `UnregisterValueChanged`，方便開發通用的表單或屬性面板。
* **`FieldElement<T>`**：泛型實作基底。它自動封裝了值的變更偵測，當內部 `value` 被修改時，會自動觸發內部註冊的 Action 委派，將底層 ImGui 變更狀態與 C# 事件系統完美結合。
