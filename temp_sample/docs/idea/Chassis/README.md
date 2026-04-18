# Chassis (底盤協調器) 規格與架構

本文件定義了 GCVex 專案中底盤控制模組的核心架構。

## 1. 介面劃分與組件化架構

為了提高可測試性與可擴展性，`Chassis` 被設計為一個「純邏輯控制」的協調者 (Coordinator)，將具體的物理細節委派給底下的三個抽象介面：

### 1.1 `IDrivetrain` (動力系統抽象)
* **職責**：將抽象的運動指令轉換為實際馬達的電壓或轉速輸出。
* **無 Shared Pointer 設計**：我們強調由外部控管生命週期。這使得 `Chassis` 與 `TankDrivetrain` 接受物件參考 (`reference`) 或指標 (`*`) 而非 `shared_ptr`，並引進 `MotorGroup` 作為統一管理馬達介面的作法。

### 1.2 `ILocator` / `IPoseTracker` (定位系統抽象)
* **職責**：持續追蹤並提供機器人當前的絕對座標與朝向 $(X, Y, \Theta)$。

### 1.3 `IController` (運動規劃與控制抽象)
* **職責**：根據當前位置、目標位置、物理限制（最大速度、加速度），計算出下一刻「應該」輸出的前進速度 $V$ 和旋轉速度 $\omega$。
* **實作例**：`PIDController` (純點到點 PID)、`TrapezoidalProfile` (梯形速度曲線)、`PurePursuit` (路徑跟隨)。

## 2. Chassis 運作流程

* **職責**：作為上述三大組件的容器，提供狀態機簡單易用的 API。
* **運作**：在狀態機 `tick()` 中呼叫 `Chassis::update()`。`Chassis` 會：
  1. 向 `ILocator` 詢問當前位置。
  2. 將位置資訊交給 `IController` 計算 $V$ 和 $\omega$。
  3. 最後將計算結果交給 `IDrivetrain` 驅動馬達。
