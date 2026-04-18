# Dashboard 子系統與通訊協定導覽

`Dashboard` 是一個依附於 `gcvex::Application` 架構的獨立子系統，專責處理與外部 C# 端點（或任何相容的接收端）的序列埠通訊。

## 1. 開關設定與初始化

Dashboard 的啟用受到編譯期的巨集保護，以達到正式競賽的零開銷：

* 在 `include/gc_config.h` 中設定 `#define ENABLE_DASHBOARD 1` 即可啟用。
* 啟用後，Dashboard 會在程式初始化時，自動將自己註冊為一個 `gcvex::Application::ISubSystem`。
* 它的底層 Dispatch Loop 會以 **8Hz** 的頻率將指令與變數變化推播至序列埠。

## 2. 變數系統：`DashEntity`

Dashboard 系統使用 **代理模式 (Proxy Pattern)** 的 `DashEntity` 系統，你不需要手動處理同步或是發送邏輯，只需要建立對應型別的 Entity，變數值的更動與初次設定都會自動進入通訊佇列。

變數可以透過 `Path` 字串建立 Group（如 `"Drive/Speed"`）。其中最後一個 `"/"` 之前的部分會自動被前端解析為分類目錄。
> **注意**：Path 字串的最大長度限制為 **50** 個字元，超過會觸發 `Debug::raise` 致命錯誤。全系統最多允許註冊 **256** 個不同的 Entity。

`DashEntity` 依據其型別的輕重，被分為兩種使用方式：

### 2.1 輕量級數值：`ValueEntity<T>`
適用於輕量的純數值（如 `int`, `float`, `bool`）。它是真正意義上的 Proxy，你可以在任何地方宣告同名、同型別的 `ValueEntity`，它們在底層 `Registry` 中都會對應並讀寫同一塊記憶體實體。

```cpp
// 建議用法：在需要的地方直接宣告並呼叫 set / get
gcvex::ValueEntity<float> speedEnt("Chassis/TargetSpeed_rpm");
speedEnt.set(200.0f);

float currentTarget = speedEnt.get();
```

### 2.2 複雜硬體與多型態物件：`ReferenceEntity<T>`
適用於複雜的硬體或複合型別（如 Motor、Chassis、Path）。`ReferenceEntity` 並不會自己 Allocate 記憶體，而是綁定一個**已經存在**的物件指標。

```cpp
// 該類別需要有對應的 DashEntityHandler<T> 特化實作
gcvex::ReferenceEntity<MotorProvider> motorEnt("Motors/LeftFront", &myLeftFrontMotor);

// 當你需要發送自定義 Opcode 觸發前端特定行為時
motorEnt.send(0x01);

// 或者可以當作語法糖直接操作底層指標
motorEnt->set_power(100.0f);
```

---

## 3. 通訊協定與前端開發指引

為了最小化 VEXnet 序列埠的頻寬佔用，Dashboard 使用自訂的**二進位壓縮通訊協定**。

所有的封包結構、指令集定義、以及 `DashEntityHandler` 如何對各型別資料（如浮點數、整數）進行壓縮的規範，皆統一放置於本專案的 `./DashboardProtocol` 資料夾中。

如果你是負責撰寫前端 (C# / Web) 接收端的開發者，請**直接參閱以下規格書**，不需要閱讀 C++ 原始碼：

1. **[PacketStructure.md](./DashboardProtocol/PacketStructure.md)** - 傳輸層、封包長度限制、以及 Checksum 的計算方式。
2. **[Commands.md](./DashboardProtocol/Commands.md)** - 各指令 (`0x00` ~ `0x07`) 的位元組結構與觸發方向。
3. **[DashEntity.md](./DashboardProtocol/DashEntity.md)** - `Entity Type` 對照表、`Reference` Opcode 規範、以及各型別反序列化的詳細演算法與 Pseudo Code。