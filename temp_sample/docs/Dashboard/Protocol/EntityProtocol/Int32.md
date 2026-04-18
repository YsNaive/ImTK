# Entity Type: Int32 (0x01)

`Int32` 是一個 **Value Entity**，用於同步 32 位元整數。

## 動態長度壓縮演算法 (Dynamic Length Compression)

為了極大化頻寬效益，整數不會永遠佔用 4 bytes。發送端會根據數值的實際大小，將其壓縮為 1, 2 或 4 bytes (使用 Little Endian)。
接收端必須透過 `Sync Entity` 標頭所提供的 `[Data Length]` 欄位，來反推目前的整數被壓縮成多大，並進行**符號擴充 (Sign Extension)**。

### 壓縮規則 (發送端)
1. 若數值落在 `[-128, 127]` (符合 8-bit signed)，序列化為 **1 Byte**。
2. 若數值落在 `[-32768, 32767]` (符合 16-bit signed)，序列化為 **2 Bytes**。
3. 否則，序列化為完整的 **4 Bytes**。

### C# / Pseudo Code 解析範例 (接收端)

```csharp
// payloadBytes 是從 Sync Entity 中取出的 Data Bytes 陣列
// payloadLength 是這個陣列的長度
public int DeserializeInt32(byte[] payloadBytes, int payloadLength)
{
    if (payloadLength == 1)
    {
        // 讀取為 8-bit 有號整數，隱式進行符號擴充
        return (sbyte)payloadBytes[0];
    }
    else if (payloadLength == 2)
    {
        // 讀取為 16-bit 有號整數 (Little Endian)，然後隱式擴充為 32-bit
        short value = (short)(payloadBytes[0] | (payloadBytes[1] << 8));
        return value;
    }
    else if (payloadLength == 4)
    {
        // 讀取為 32-bit 整數 (Little Endian)
        return payloadBytes[0] |
               (payloadBytes[1] << 8) |
               (payloadBytes[2] << 16) |
               (payloadBytes[3] << 24);
    }

    return 0; // 錯誤的長度
}
```