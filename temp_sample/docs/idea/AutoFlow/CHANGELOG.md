# Auto Flow 歷史演進與重構紀錄

本文件記錄了 Auto Flow 模組架構的演進過程與舊版架構痛點。

## 從 Callback Trigger 到 平行狀態機

### 舊版 Trigger 運作機制
在早期的 `./old/skill_auto.cpp` 和 `./old/include/chassis.h` 中，使用了基於 Builder Pattern 和 Callback 的 Trigger 系統：

```cpp
move_to_point(-13.25, -12.0)
    .maxSpeed(50)
    .onDistanceRemaining(7.0, [](double remaining) {
        liftintake.set(1); // 抬起 intake
        return true;
    });
```

這在 `_execute_move_to_point` 的 `while` 迴圈中，每次 Tick (20ms) 檢查 `dist_error` 是否小於 `trigger_distance` 並執行 Callback。

### 舊版痛點與重構原因
* **耦合度極高**：底盤 (Chassis) 的核心移動邏輯裡，混入了對其他機構的狀態檢查與呼叫。底盤不應知道什麼是 Intake。
* **難以維護**：大量巢狀的 Lambda 函式導致自動程式變長時難以閱讀（可靠性與可複用性低的主因）。
* **無法處理耗時動作**：Callback 內部不能有任何 `wait()` 或耗時操作，否則會直接阻塞底盤 PID 計算，導致失控。

### 新版解法
為了解決上述問題，系統重構為 [README.md](./README.md) 中描述的 **平行狀態機**，達成了完全解耦與支援背景非阻塞等待 (`Mode::Once`) 的目標。
