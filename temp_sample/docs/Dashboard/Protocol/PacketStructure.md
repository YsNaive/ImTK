# Dashboard 通訊協定：封包與傳輸層規範 (Packet Structure)

本文件定義了 VEX 機器人與前端 (C# / Web) 之間的序列埠傳輸層格式。

## 1. 傳輸層限制

由於 VEXnet 的無線電通訊限制，為避免緩衝區溢位與延遲，單一實體傳輸 (Physical Transmission) 的大小被嚴格限制在 **150 bytes** 以內。
任何超出此限制的邏輯封包 (Logical Packet) 同步，都將由底層通訊模組自動透過切片 (Chunking) 機制分批發送，應用層無需自行處理。

## 2. 邏輯封包結構 (Logical Packet Layout)

每個合法的傳輸封包必須完全遵循以下格式：

| 欄位名稱 | 佔用位元組 (Bytes) | 說明 |
| :--- | :---: | :--- |
| **Header** | 1 | 用於同步字節流，尋找邏輯封包起點。<br>**TX (VEX 到 C#): `0xEE`**<br>**RX (C# 到 VEX): `0xEF`** |
| **Length** | 1 | 標示緊接在後的 `Payload` 區塊的總長度 (N bytes)。 |
| **Payload** | N | 實際的通訊指令與資料（可以包含**多個連續的 Command**）。 |
| **Checksum** | 1 | XOR 校驗碼。 |

## 3. 校驗碼 (Checksum) 計算演算法

為了確保傳輸的資料沒有受到無線電雜訊干擾，接收端在收到一組邏輯封包後，必須驗證其 Checksum。

* **計算範圍**：包含 `Header` (`0xEE` 或 `0xEF`)、`Length`、以及整個 `Payload`。
* **演算法**：對範圍內的每一個位元組依序執行 XOR (互斥或) 運算，初始值為 `0`。

### C# / Pseudo Code 驗證範例

```csharp
// packetBytes 包含了完整的邏輯封包 (Header, Length, Payload, Checksum)
public bool VerifyChecksum(byte[] packetBytes)
{
    if (packetBytes.Length < 3) return false;

    byte calculatedChecksum = 0;

    // 計算到倒數第二個 byte 為止 (不包含封包自帶的 Checksum)
    for (int i = 0; i < packetBytes.Length - 1; i++)
    {
        calculatedChecksum ^= packetBytes[i];
    }

    // 與封包最後一個 byte 進行比對
    byte receivedChecksum = packetBytes[packetBytes.Length - 1];
    return calculatedChecksum == receivedChecksum;
}
```

## 4. Length 擴充標記 (大於 255 Bytes)

由於 `Length` 預設只佔用 1 byte，其最大值為 `254`。
當 Payload 的總長度大於等於 `255` 時（例如傳輸巨型的陣列或路徑資料），傳輸層採用 **`0xFF` Escape Sequence（擴充標記）** 進行處理：

1. 如果接收端讀取到的 `Length` 為 `0xFF` (255)，則此 byte **不代表實際長度**。
2. 接收端必須緊接著讀取接下來的 **2 bytes**，並將其解析為 `UInt16` (Little Endian)，這個值才是真實的 Payload 長度。

**範例**：
* 長度為 100 bytes：`[Length: 0x64]`
* 長度為 300 bytes：`[Length: 0xFF] [0x2C] [0x01]` (0x012C = 300)

*(註：此 0xFF 擴充標記規則同時適用於 [Commands.md](./Commands.md) 中的 `Data Length` 欄位)*

## 5. 雙模式發送策略 (Dual-Mode Packet Strategy)

由於序列埠具有 `150 bytes` 的實體傳輸限制 (PHYSICAL_MTU)，發送端在建構邏輯封包時，採用了智能的大小封包分離策略。這對於接收端實作 Ring Buffer 的解碼邏輯非常重要：

### 模式 A：小封包聚合模式 (Small Packet Mode)
* **目的**：最小化延遲並保留變數去重覆蓋 (Deduplication) 的機會。
* **行為**：發送端會將多個小指令（如 `Int32`, `Float`）聚合進同一個邏輯封包中發送。
* **關鍵特性**：小邏輯封包的總長度**絕對不會超過 150 bytes**。它保證可以在一個實體傳輸週期內被完整送出，絕不切塊。如果目前的剩餘頻寬無法容納下一個小指令，發送端會直接放棄剩餘頻寬，將指令保留至下一個週期。

### 模式 B：巨型封包切塊模式 (Large Packet Mode)
* **目的**：支援超大物件 (如 Odometry 陣列) 的傳輸，而不需在應用層實作複雜的 Chunking 協議。
* **行為**：當單一指令的大小超過 `147 bytes` (MAX_PAYLOAD) 時，發送端會為其建立一個巨型的邏輯封包（使用 `0xFF` Length 擴充標記），並將其放入專屬的傳輸緩衝區。
* **關鍵特性**：這個巨型邏輯封包會被底層硬生生地切碎，分批在多個 150 bytes 的實體傳輸週期中送出。

### 接收端實作重點：流式解碼 (Stream Decoding)
因為「巨型封包」會被切塊跨越傳輸，接收端**絕對不能**假設每次 `Serial.Read()` 讀到的就是一個完整的邏輯封包！
接收端必須維護一個 **Ring Buffer** 或 List：
1. 在 Buffer 中尋找合法的 Header (`0xEE`) 作為起始。
2. 讀取下一個位元組作為 `Length`。若為 `0xFF`，則繼續讀取 UInt16 得出真實長度 `N`。
3. 計算這個邏輯封包的總長度：`1 (Header) + Length Bytes + N (Payload) + 1 (Checksum)`。
4. **確認 Buffer 中剩餘的資料是否達到總長度**。
   * 如果資料不夠，停止解析，保留 Buffer，等待下一次 Serial 觸發讀取（這就是巨型封包正在跨越傳輸的狀態）。
   * 如果資料足夠，執行 Checksum 驗證。成功則提取這 N 個 bytes 的 Payload 進行連續指令解析。