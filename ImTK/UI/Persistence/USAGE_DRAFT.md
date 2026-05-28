# UI 狀態持久化快取機制 (開發者暫存指南)

本資料夾 (`ImTK/UI/Persistence`) 包含了 ImTK 的自動化 UI 狀態快取機制底層。
此文件為暫存的開發者使用指南，待未來機制更完善與驗證後將整合至主要的 `docs/` 教學文件中。

## 機制簡介
這套系統允許 UI 元件 (如 `SplitView`、`Slider`、`Foldout` 等) 自動將使用者的操作狀態 (例如分割比例、展開/收合狀態) 存檔至本地端 (`imtk_cache.json`)。
*   **效能極佳**：拖拉 UI 元件時完全不會觸發 IO 或產生 GC (無 Boxing)。
*   **全自動調度**：讀取與寫入生命週期由 `Window` 與 `Panel` 在背景自動完成 (透過定時器收集與關閉時強制寫檔)。
*   **防撞機制**：開發者只需指定元件的 `persistenceKey`，底層會自動加上 Window ID 作為前綴，防止不同視窗間的 Key 發生衝突。

---

## 如何讓自訂元件支援自動存檔？

身為元件開發者，只要按照以下 3 個步驟即可無痛支援狀態存檔：

### 1. 繼承 VisualElement
確保你的 UI 元件是繼承自 `VisualElement`。

### 2. 覆寫 `OnWriteState` 與 `OnReadState`
這兩個方法定義了你的元件要交出什麼變數給系統存檔，以及如何將讀取到的值還原回內部變數。

```csharp
using ImTK.UI.Persistence;

public class MySplitView : VisualElement
{
    // 假設這是你想記錄的狀態
    private float m_ratio = 0.5f;

    // 定義當系統要求寫檔時，你要交出什麼變數
    protected internal override void OnWriteState(StateWriter writer)
    {
        writer.WriteFloat(persistenceKey, m_ratio);
    }

    // 定義當系統初始化時，你要怎麼把資料讀回來
    protected internal override void OnReadState(StateReader reader)
    {
        // 讀取快取值。若找不到之前的快取（例如初次開啟），將會 fallback 使用預設值 0.5f
        m_ratio = reader.ReadFloat(persistenceKey, 0.5f);
    }
}
```

### 3. 在建立元件時賦予 `persistenceKey`
在組合 UI 樹 (Visual Tree) 時，只要開發者有為該元件設定 `persistenceKey`，系統就會在背景自動接管它的存檔與讀取。

```csharp
var splitView = new MySplitView() 
{ 
    persistenceKey = "MainLayout_SplitRatio" // 只要有設定 Key，該元件就擁有存檔能力
};
myWindow.hierarchy.Add(splitView);
```

---

## 注意事項
1. **防撞警告**：如果在同一個 `Window` 中，將兩個元件設定了完全一樣的 `persistenceKey`，系統會在背景產生 `Error` 級別的 Log 日誌來警告開發者 (Key Collision)，以防 UI 狀態互相覆蓋。
2. **預設路徑**：快取資料將與 Dear ImGui 原生的 `imgui.ini` 一起，統一存放於 `[ImTKEnvironment.LocalDataPath]/imgui/imtk_cache.json` 中。
