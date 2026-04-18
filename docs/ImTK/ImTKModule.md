# 自動模組管理系統 (ImTKModule)

`ImTKModule` 是整個 ImTK 框架的「隨插即用 (Plug-and-Play)」引擎。它解決了在 ImGui 應用程式中，開發者需要手動將各個元件的更新方法寫入到一個龐大主迴圈的痛點。

## 1. 核心設計理念與架構

本模組被設計為一個基於反射 (Reflection) 的自動發現系統。任何繼承自 `ImTKModule` 的非抽象類別，都會被視為一個獨立的子系統模組。

**常見實作模式 (Private Nested Module)**：
為了保持命名空間乾淨並限制存取，通常會將繼承的類別設計為目標類別內部的**私有嵌套類別**。例如：
```csharp
public class Window : VisualElement
{
    private class Module : ImTKModule
    {
        private Module() { } // 私有建構子防止外部手動實例化

        public override void OnLoad() { /* 讀取狀態 */ }
        public override void Update(double deltaTime) { /* 呼叫更新 */ }
    }
}
```

## 2. 生命週期階段 (Lifecycle)

ImTK 的程式進入點 (`ImTKSilk.Initialize`) 會自動驅動以下生命週期，無須開發者介入：

1. **發現與註冊 (`InitializeAll`)**：
   系統掃描 `AppDomain` 中所有的 Assembly (排除 `System` 與 `Microsoft` 開頭者)，尋找繼承自 `ImTKModule` 的類別，並使用 `Activator.CreateInstance(type, nonPublic: true)` 強制實例化後註冊到內部清單。

2. **載入 (`OnLoad`)**：
   當 Silk 視窗創建成功，並且 ImGui Context 完全準備就緒後觸發。
   * **適用場景**：載入客製化字型、從 JSON 反序列化先前的狀態、設定 ImGui 的指標 (如 `IniFilename`)。

3. **每幀更新 (`Update` & `Render`)**：
   由視窗主迴圈每幀觸發。
   * **注意**：這兩個方法被設計為容錯迭代。即使某個模組在執行中嘗試載入新模組，迭代也能安全進行。

4. **關閉 (`OnClose`)**：
   在視窗接收到關閉信號且資源即將銷毀前觸發。
   * **適用場景**：將當前狀態寫入磁碟 (如 `SaveWindowState`)、釋放透過 `Marshal` 取得的非受管記憶體。

## 3. 開發者防呆與邊界注意事項

* **無參數建構子限制**：`ImTKModule` 依賴反射進行自動實例化，因此子類別**必須**提供一個無參數的建構子 (可以是 `private`)。若加入帶參數的建構子且未補上無參數版本，該模組將會因為拋出例外而被靜默略過 (Ignored)。
* **執行順序未定義**：模組註冊順序取決於 Reflection 返回 `Type` 的順序。不同模組間**嚴禁**在 `OnLoad` 階段有強依賴關係 (例如 Module A 的 OnLoad 依賴 Module B 已經 OnLoad 完畢)。若需要通訊，請透過靜態變數於 `Update` 階段處理。
