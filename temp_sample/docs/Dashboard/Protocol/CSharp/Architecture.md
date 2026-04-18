# C# 接收端架構設計 (C# Architecture)

本文件描述了 C# 接收端特化的架構設計。關於與 VEX C++ 端一致的底層通訊協定、指令格式與序列化演算法，請參閱上一層 `../` 的通用協定文件。

## 1. 架構概覽

C# 端的目標不僅是「接收與解碼」資料，更需要將資料即時渲染至 UI 介面上。本專案使用 `ImTK` (基於 ImGui) 作為 UI 框架。為了達到「資料與 UI 不分離」的目標，實體層設計直接繼承自 UI 元素。

整體的資料流向如下：
`WebSocket (Background)` -> `ConnectionWindow (UI Event Hook)` -> `Packet Parser` -> `Command Dispatcher` -> `Registry` -> `DashEntity (UI)`

## 2. 通訊與解析層

* **WebSocketService**: 在背景執行緒透過 `System.Net.WebSockets.ClientWebSocket` 連接至 VEX 主機，持續讀取 Byte Stream（支援 `Binary` 與 `Text` MessageType 以相容 VEXCode Extension 的格式）並存入 Buffer。
* **ConnectionWindow**: 負責接收來自 `WebSocketService` 的資料，並直接呼叫 `PacketParser.ProcessBuffer`（取消了受 UI 實例化生命週期影響的 Delegate 綁定，改採直接呼叫以確保穩定性）。
* **PacketParser**: 實作流式解碼 (Stream Decoding)。處理 `0xEE` 標頭搜尋、`0xFF` 長度擴充，並驗證 XOR Checksum。當 Buffer 滿足完整封包長度時，提取 Payload。
* **CommandDispatcher**: 解析 Payload 內的第一個 Byte (`Command ID`)。支援連續多指令解析，並呼叫 `Registry` 處理 `[0x00] Reset`、`[0x05] Create Entity` 與 `[0x06] Sync Entity`。

## 3. Entity 模型與 UI 綁定

在 C# 端，所有的 Entity 都直接繼承自 ImTK 的 `VisualElement`，這表示它們既是資料容器，也是 UI 繪製邏輯的提供者。

### 基礎抽象類別 `DashEntity`
* 繼承自 `VisualElement`。
* 儲存基本的 `id`, `typeId`, `path`, `group`, `name`。
* 當建立時，解析 `path`。如果 `path` 中沒有 `/`，則預設 `group` 為 `"Inspector"`。
* 定義抽象方法 `public abstract void receive(byte opcode, byte[] data);` 供子類別實作反序列化。

### 具體實體 (Value Entities)
* `IntEntity`: 實作動態長度解壓縮，並在 `Render()` 中呼叫 `ImGui.InputInt`。
* `FloatEntity`: 先用 Int 動態長度解碼再除以 `100.0f`，在 `Render()` 中呼叫 `ImGui.InputFloat`。
* `BoolEntity`: 讀取單一 Byte，在 `Render()` 中呼叫 `ImGui.Checkbox`。

## 4. 註冊表 (Registry) 與反射工廠 (Reflection Factory)

`Registry` 是靜態全域管理器，負責維護 ID 對應的實體 (`DashEntity[]`) 以及動態視窗 (`Dictionary<string, DashEntityWindow>`)。

### 跨執行緒調度 (Main Thread Dispatcher)
由於 WebSocket 接收發生在背景執行緒，直接修改 UI 結構 (`Window.Add`) 會引發例外。因此 `Registry` 實作了 `ConcurrentQueue<Action>`，將所有的建立與同步邏輯包裝起來。
配合內部實作的 `RegistryModule : ImTKModule`，利用 ImTK 引擎每一幀的 `Update` 週期來安全地清空佇列並執行操作。

### 反射自動註冊 (Reflection Auto-Registration)
為了提高擴充性，Registry 不使用手動的 `switch-case` 來建立物件。相反地，所有繼承 `DashEntity` 的類別都應該標註 `[EntityType]` 屬性：

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class EntityTypeAttribute : Attribute
{
    public byte TypeId { get; }
    public bool IsReference { get; }
    // ...
}

[EntityType(0x01, isReference: false)]
public class IntEntity : DashEntity { ... }
```

Registry 在初始化時會掃描組件，建立 `typeTable` (用於動態建構) 與 `isReferenceTable` (用於在 Sync 時判斷 Payload 是否包含 Opcode)。

## 5. 動態視窗 (DashEntityWindow)
代表一個 Group（例如 `"Chassis"`）。它繼承自 `ImTK.WindowView`。
當 `Registry` 透過 `CreateEntity` 遇到新的 Group 時，會手動 `new DashEntityWindow(groupName)`，然後將新的 `DashEntity` (VisualElement) 加入該視窗。如此一來，同一 Group 的變數會自動整理在同一個浮動視窗中進行渲染。