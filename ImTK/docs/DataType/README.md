# 共用資料結構 (DataType)

本模組存放了 ImTK 框架中經常被使用於幾何運算、排版計算與 UI 狀態表示的基礎資料結構 (Structs)。

為了保持與標準函式庫的相容性，所有浮點數向量（如 `Vector2`, `Vector3`）皆直接採用 `System.Numerics` 底下的原生結構。本目錄僅擴充標準庫所缺乏，或是針對 UI 特化的型別。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 幾何與整數向量

在像素對齊或格線系統中，常需要使用整數向量。

*   **[`Vector2Int`](../../DataType/Vector2Int.cs)**: 二維整數向量 (x, y)。
*   **[`Vector3Int`](../../DataType/Vector3Int.cs)**: 三維整數向量 (x, y, z)。
*   **[`Rect`](../../DataType/Rect.cs)**: 浮點數矩形空間，包含 `x`, `y`, `width`, `height`。內建 `Contains()` 碰撞檢測與擴展/平移方法，廣泛應用於 `RenderEngine` 的 `layoutRect` 佈局計算。
*   **[`RectInt`](../../DataType/RectInt.cs)**: 整數矩形空間。

### 2. 顏色與字串雜湊

*   **[`Color`](../../DataType/Color.cs)**: 支援 RGBA (0.0f ~ 1.0f) 浮點表示的顏色結構。內建與 ImGui 底層所需 `uint` 色碼格式的雙向轉換，並提供類似 `Color.white`, `Color.clear` 的常數快捷鍵。
*   **[`HashedString`](../../DataType/HashedString.cs)**: 一種利用字串內容計算並快取 HashCode 的結構。專門用於需要高頻率比較字串 ID（例如在 Dictionary 查找或是事件識別）的場景，以空間換取時間，提升比對效能。
