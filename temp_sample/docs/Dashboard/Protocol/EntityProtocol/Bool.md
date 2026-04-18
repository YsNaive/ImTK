# Entity Type: Bool (0x03)

`Bool` 是一個 **Value Entity**，用於同步布林值狀態。

## 壓縮規則
長度固定為 **1 Byte**。

* `0x00` 代表 `False`。
* `0x01` (或任何非零值) 代表 `True`。

### C# / Pseudo Code 解析範例

```csharp
public bool DeserializeBool(byte[] payloadBytes, int payloadLength)
{
    if (payloadLength != 1) return false;
    return payloadBytes[0] != 0x00;
}
```