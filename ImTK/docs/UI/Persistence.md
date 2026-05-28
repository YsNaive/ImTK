# UI 狀態持久化 (Persistence)

本系統負責將 UI 元件的暫時狀態（例如：Slider 的數值、視窗的大小、文字輸入框的內容）透過 JSON 格式持久化儲存，確保應用程式重啟後能復原上次的工作環境。

底層儲存機制依賴 `ImTKDatabase` 產生的 `imtk_cache.json`。

## `[Persistent]` Attribute (語法糖)

為了解放開發者手動覆寫 `OnWriteState` 與 `OnReadState` 的繁瑣，ImTK 提供了 `[Persistent]` 標籤與 `PersistentTypeCache` (快取反射機制)。

### 基礎用法

開發者只需在繼承自 `VisualElement` 的類別中，為想要存檔的 `Field` 或 `Property` 加上 `[Persistent]` 即可。

```csharp
public class MySettingsPanel : VisualElement
{
    [Persistent]
    private float m_sliderValue = 0.5f;

    // 自訂儲存的 Key 名稱
    [Persistent("CustomStringKey")]
    public string Description { get; set; } = "Hello";

    public MySettingsPanel()
    {
        // 注意：元件必須要有 persistenceKey 才會觸發存檔機制！
        this.persistenceKey = "MyUniqueSettingsPanelKey";
    }
}
```

> [!WARNING]
> 元件的 `persistenceKey` 必須在所屬的 `Window` 內為「唯一」，系統會利用它作為防撞機制。若有重複的 Key，會在 Log 產生 Error。

### 深層遞迴拆解 (Recursive Flattening)

`[Persistent]` 支援將複雜的類別 (Class) 或結構 (Struct) 攤平存入 JSON 字典中。

```csharp
public struct MyConfig
{
    public float Width;
    public int Height;
}

public class MyElement : VisualElement
{
    // Flatten 預設為 true。系統會自動拆解，產生諸如 "MyElementKey.Config.Width" 的 JSON Key。
    // IncludeAllMembers = true 代表會自動將 Width 與 Height 納入存檔，不需要在它們身上再加標籤。
    [Persistent(IncludeAllMembers = true)]
    private MyConfig Config = new MyConfig { Width = 100, Height = 200 };
}
```

> [!TIP]
> 系統利用了 **父層回推 (Recursive Push-back)** 機制，因此支援**任意深度**的巢狀 `struct` 更新，完全不受 C# 裝箱副本 (Boxed Copy) 問題的影響！

### 支援型別與防呆
- 目前支援的基礎資料型別有：`float`, `int`, `string`, `bool`。
- 若 `Flatten = false` 且型別不受支援（例如 `Vector2`），系統不會崩潰，而是透過 `ImTKLog.Error` 在背景發出非阻斷式的防呆警告。
- 具備防範「循環參考 (Circular Reference)」的安全機制，在快取建置時會精準抓出無限遞迴。

## 觸發時機 (`ViewStatePersister`)

- **讀取 (Load)**：當 `Window` 觸發渲染樹重建 (`RenderCache` Dirty) 後，會掃描所有的 `RenderOpType.Begin` 節點。如果該元件有設定 `persistenceKey` 且尚未讀取過，則觸發 `OnReadState`。
- **寫入 (Save)**：會在每一幀 `ImTKApplication.Run` 結束前，批次掃描所有活動中的視窗，收集元件狀態標記 Dirty，並最終寫入 `imtk_cache.json`。
