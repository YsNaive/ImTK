# PID (Proportional-Integral-Derivative) 控制器規格

本文件定義了 GCVex 中 PID 控制器的架構與數學模型。它被設計為一個純數學計算器，完全不依賴狀態機或特定的 Tick 頻率。

## 1. 核心輸入參數
* **`kP`, `kI`, `kD`**: 比例、積分、微分常數。
* **`kF` (Feedforward)**: 前饋常數（選配，升級為 PIDF 時使用）。
* **`windupMax`**: 限制積分項的絕對值上限，作為抗積分飽和（Anti-Windup）機制。
* **`maxOutput`**: 限制最終 PID 輸出的絕對值。

## 2. 引入真實時間 $\Delta t$ (Time-based PID)
為了適應非固定頻率的狀態機與背景迴圈，`calculate()` 方法必須傳入經過時間 `delta_time`，以確保微積分項不會失真：

* **積分項 (I)**: $I = \sum (error \cdot \Delta t) \cdot kI$
* **微分項 (D)**: $D = \frac{error - prevError}{\Delta t} \cdot kD$

> **相容性注意**：如果外部不打算提供真實時間，傳入預設值 `1.0` 即可完美退化相容於舊的 Tick-based 邏輯。

## 3. 狀態解耦與重置
PID 物件不具備「狀態機階段」的概念。它只提供純淨的 `calculate()` 與 `reset()` 介面。
重置內部狀態（如積分和前次誤差）的責任交給呼叫者，例如在進入一個新的 `MoveState` 時，手動呼叫 `pidInstance.reset()`。

## 4. 前饋控制 (Feedforward, F) 的預留
在處理點到點移動或速度追蹤時，F 參數可用於預測理想輸出以克服系統靜摩擦力，模型為：$Output = (P + I + D) + Feedforward$。
