# Chassis 歷史演進與重構紀錄

本文件記錄了底盤模組從早期的巨型耦合邏輯，拆分為當前組件化架構的演進脈絡。

## 舊有架構分析 (Pre-Componentization)

在早期的 `./old/src/chassis.cpp` 中，底盤控制邏輯包含了許多為克服現實環境而硬編碼的補償機制，這導致了高度耦合與難以測試。

### 1. 舊版 `_execute_move_to_point` 控制邏輯
這曾經是一個點到點移動的阻塞迴圈 (`while`)，包含了許多混雜的邏輯：
* **角度誤差計算**：`angle_error = normalizeAngle(target_angle - currentTheta)`。
* **Misalignment 修正懲罰**：基於姿態的動態速度衰減。當誤差 > 45 度時，將前進速度砍到 30%，讓機器人「先轉向，再前進」。
* **`seamless_mode` (無縫模式)**：允許機器人在連續多點移動時，不降速到 0，而是保持 `exit_speed` 退出當前迴圈。

### 2. 重構為組件化架構的原因
* 舊有架構讓新增控制演算法（如 Pure Pursuit 或 Motion Profiling）變得極為困難，因為所有東西都寫死在 `while` 迴圈中。
* **解決方案**：如 `README.md` 所述，將這些經驗補償拆分歸屬：
  * Arcade 混音與轉向優先邏輯歸屬於 `IDrivetrain` (TankDrivetrain)。
  * Misalignment 懲罰機制歸屬於特定的 `IController` (如 `PIDController`)，這允許在未來採用完整的 Motion Profiling 時，直接抽換控制器以捨棄這種暴力的懲罰機制。
