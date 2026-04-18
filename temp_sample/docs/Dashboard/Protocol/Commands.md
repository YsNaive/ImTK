# Dashboard 通訊協定：指令集規範 (Commands)

在一個 Packet 的 Payload 區塊中，可以包含**一個或多個連續的指令**。
接收端在讀取 Payload 時，必須根據每一個指令的 **第一個 Byte (Command ID)** 來決定後續的解析長度與動作。

## 傳輸方向標示
* `[C]`：由 VEX 發送至 C# 端 (VEX -> C#)
* `[V]`：由 C# 端發送至 VEX (C# -> VEX)
* `[B]`：雙向皆可發送 (Bidirectional)

---

## 系統控制指令

### `[0x00]` Reset `[C]`
重置 Dashboard 狀態。
* **觸發時機**：VEX 端剛開機、或是 Dashboard 系統重新啟動時。
* **C# 端行為**：必須清除所有儲存的 Entity，重置所有介面與註冊表狀態。
* **結構**：`[0x00]` (長度：1 byte)

### `[0x01]` Fetch `[V]`
重新建立並發送所有資料實體至 C# 端。
* **觸發時機**：當 C# 端剛連線，或是中途斷線重連，發現自己沒有最新的變數清單時，主動發送。
* **VEX 端行為**：VEX 將會把目前註冊表中所有的 Entity，重新發送 `Create Entity` 與 `Sync Entity`。
* **結構**：`[0x01]` (長度：1 byte)

### `[0x02]` Teleop `[V]`
開始執行手動控制階段 (User Control / Driver)。
* **結構**：`[0x02]` (長度：1 byte)

### `[0x03]` Auto `[V]`
開始執行自動階段 (Autonomous)。
* **結構**：`[0x03]` (長度：1 byte)

### `[0x04]` Stop `[V]`
結束當前的執行階段 (回到 Disable 狀態)。
* **結構**：`[0x04]` (長度：1 byte)

---

## Entity 操作指令

這些指令負責同步機器人的內部變數狀態。

### `[0x05]` Create Entity `[C]`
在 C# 端宣告並建立一個實體。
* **觸發時機**：當 VEX 程式中初次建立一個 `DashEntity`，或是收到 `Fetch` 請求時。
* **結構**：
  `[0x05] [Entity ID] [Type ID] [Path Length] [Path Bytes...]`
* **欄位說明**：
  * **Entity ID** (1 byte): `0~255`，該變數的全局唯一識別碼。
  * **Type ID** (1 byte): 該變數的型別 (對應解析邏輯，詳見 [DashEntity.md](./DashEntity.md))。
  * **Path Length** (1 byte): 字串的長度 N (最大 50)。
  * **Path Bytes** (N bytes): ASCII 字串。最後一個 `/` 之前會被 C# 視為分類目錄 (Group)，之後視為名稱。

### `[0x06]` Sync Entity `[B]`
同步實體的資料狀態。
* **觸發時機**：當 VEX 端的變數改變時，或 C# 端試圖修改 VEX 端的變數時。
* **結構**：
  `[0x06] [Entity ID] [Data Length] [Data Bytes...]`
* **欄位說明**：
  * **Entity ID** (1 byte): 要同步的對象 ID。
  * **Data Length** (1 或 3 bytes): 緊接在後的資料位元組長度 N。若資料長度 $\ge 255$，此欄位將使用 `0xFF` 擴充標記，真實長度將會由後面的 2 bytes (UInt16) 決定（詳見 `PacketStructure.md` 的長度擴充標記說明）。
  * **Data Bytes** (N bytes): 經過 `DashSerializer` 壓縮的資料，或是 `ReferenceEntity` 的 Opcode Payload。詳細解析請對照該 Entity 的 Type ID 與 [DashEntity.md](./DashEntity.md) 進行解碼。

### `[0x07]` Fetch Entity `[V]`
向 VEX 請求單一實體的完整建構資料。
* **觸發時機**：當 C# 端收到 `Sync Entity (0x06)`，但發現本地端的註冊表中並不存在該 `Entity ID` 時。為了避免封包遺失造成的不同步，C# 端主動要求 VEX 重傳該 ID 的 `Create Entity`。
* **結構**：
  `[0x07] [Entity ID]` (長度：2 bytes)