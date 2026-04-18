# PID 歷史演進與重構紀錄

本文件記錄了 PID 模組的演進脈絡。

## 舊有架構分析 (Tick-based PID)

在早期的 `./old/src/pid.cpp` 中，PID 實現隱含地假設了每次呼叫 `calculate()` 的時間間隔都是固定的（例如 20ms）。

### 舊版數學模型與痛點
* **I 項**：`integral += error` (未考慮 $\Delta t$)
* **D 項**：$D = (error - prevError) \cdot kD$ (未除以 $\Delta t$)

這在早期的死迴圈架構中沒有問題，但當專案升級為**事件驅動的 `Application` 與平行狀態機**後，迴圈的執行頻率 (`Tick`) 可能因為負載而產生微小波動。這會導致舊版的微分項與積分項嚴重失真。

### 重構決定
因此，我們將 PID 升級為 **Time-based PID**（如 `README.md` 所述），強制要求在計算時考慮時間差，以確保控制演算法在任何硬體負載下都能保持一致的精準度。
