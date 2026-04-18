# Dashboard 通訊協定：Entity 代理系統與型別規範

`Entity` 是 Dashboard 通訊協議中用於映射資料變數的抽象概念。
為了將序列埠的頻寬佔用降到最低，VEX 端不會每次都傳送字串名稱（如 `"Chassis/Speed"`），而是透過 `[0x05] Create Entity` 指令將字串綁定到一個 1-byte 的 `Entity ID` (`0~255`)，並指定一個 **Type ID**。
之後所有的同步 `[0x06] Sync Entity`，雙方都只依靠這個 ID 進行溝通。

## Entity 兩大分類：Value 與 Reference

根據資料的複雜度與用途，通訊協議將 Entity 分為兩大類，對接收端的解析邏輯有決定性的影響：

1. **Value Entity (值型別)**：代表一個純量狀態（如整數、浮點數、布林值）。
   - **解析規則**：當收到 `Sync Entity` 時，`Data Bytes` 區段**全部都是資料本體**，直接依照該 `Type ID` 的解壓縮演算法進行還原。
2. **Reference Entity (參考型別)**：代表一個複雜的物件（如 Motor、Chassis、Path 等），可能包含多種狀態，或是支援特定的操作指令（RPC）。
   - **解析規則**：當收到 `Sync Entity` 時，`Data Bytes` 的**第一個 Byte 必定為 Opcode (操作碼)**。C# 端必須先讀取 Opcode，以決定後續的 Payload 該如何解析。

---

## Entity Type Protocol Lookup Table (協議對照表)

當接收端收到 `Create Entity` 時，必須將該 Entity 的 ID 與以下表格的 `Type ID` 進行綁定，以確保之後收到 `Sync Entity` 時使用正確的解碼器。

詳細的二進位壓縮與解碼 Pseudo Code，請點擊對應的連結進入子協議文件：

| Type ID | 型別名稱 | 分類類別 | 序列化格式概覽 | 詳細協定文件 |
| :---: | :--- | :--- | :--- | :--- |
| `0x01` | `Int32` | **Value** | 動態長度 (1, 2, 或 4 Bytes)，Little Endian | [Int32.md](./EntityProtocol/Int32.md) |
| `0x02` | `Float` | **Value** | 乘 100 後套用 Int32 壓縮，動態長度 | [Float.md](./EntityProtocol/Float.md) |
| `0x03` | `Bool` | **Value** | 固定 1 Byte (`0x00` / `0x01`) | [Bool.md](./EntityProtocol/Bool.md) |
| `>= 0x04` | *(如 Motor, Chassis)* | **Reference** | `[Opcode 1 Byte] [Custom Payload...]` | (詳見下方 Opcode 規範) |

> **請注意**：以上的 Type ID 表僅為基礎內建型別。接收端不應單純使用 `>= 0x04` 來武斷判定一個 Entity 必定是 Reference。最安全且正確的做法是：當收到 `[0x05] Create Entity` 時，將該 Entity ID 與其對應的 Type ID 記錄下來。未來收到 `Sync Entity` 時，查表確認其是否屬於有註冊的 Reference 類型，再來決定是否要讀取 Opcode。

---

## Reference Entity 與 Opcode 規範

所有屬於 **Reference Entity** 的變數（非單純純數值），在收到 `Sync Entity` 時，其 Payload 的第一個 Byte 永遠被視為 **Opcode (操作碼)**。
接收端必須根據這個 Opcode 來決定如何解析後面的位元組。

### 全局保留 Opcode

#### `[0x00]` 完全同步 (Full Sync)
* **意義**：後面的 Payload 包含了這個物件的「完整序列化狀態」。
* **格式**：`[0x00] [Custom Object Payload...]`

### 自訂 RPC Opcode (`0x01` ~ `0xFF`)

除了保留的 `0x00` 外，剩餘的 Opcode 提供給開發者自訂為 **遠端程序呼叫 (RPC)**，用來發送輕量級的狀態改變或指令。
例如：如果這是一個 Motor 實體，可以規定 `0x01` 代表設定電壓，`0x02` 代表設定煞車等。這些自訂 Opcode 的定義，應該記錄在特定硬體的協議文檔中。