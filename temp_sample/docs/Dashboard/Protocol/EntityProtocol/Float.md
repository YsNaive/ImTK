# Entity Type: Float (0x02)

`Float` 是一個 **Value Entity**，用於同步 32 位元浮點數。
在機器人控制中，浮點數通常介於 `[-1.0, 1.0]`，或是如轉速、座標等不需極端小數精度的物理量。

## x100 壓縮演算法

為了避免浮點數傳輸總是佔用 4 bytes，並避免 IEEE 754 的解析差異，本系統將浮點數強制轉化為 `Int32` 進行壓縮傳輸。

### 壓縮規則 (發送端)
1. 將浮點數乘上 `100.0f`。
2. 使用 `std::round()` 進行四捨五入取整數。
3. 強制轉型為 `int32`。
4. **完全套用 [Int32.md](./Int32.md) 的動態長度壓縮演算法**。

> **注意：** 這表示大於 `32767 / 100 = 327.67` 的浮點數才會佔用 4 bytes，而 `[-1.28, 1.27]` 之間的浮點數（例如搖桿數值、馬達百分比）只會佔用 **1 Byte**！

### C# / Pseudo Code 解析範例 (接收端)

```csharp
// 由於 Float 也是繼承了 Int32 的壓縮，直接呼叫 Int32 的反序列化函數
public float DeserializeFloat(byte[] payloadBytes, int payloadLength)
{
    // 1. 先用 Int32 規則解碼出整數
    int intValue = DeserializeInt32(payloadBytes, payloadLength);

    // 2. 除以 100 還原為浮點數
    return intValue / 100.0f;
}
```