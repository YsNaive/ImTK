# Auto Flow (自動程序) 與狀態機 (StateMachine) 規格

本文件描述了 GCVex 專案中用於編寫自動階段 (Autonomous) 腳本的核心架構：**平行狀態機 (Parallel / Concurrent States)**。

## 1. 核心概念

在 Tick-driven 的狀態機架構中，我們透過平行狀態機來解決「底盤移動時阻塞等待，導致無法同時控制其他機構」的問題。
我們的 API 使用 Fluent (鍊式) 設計，支援非常直觀地表達平行或序列關係。

### 1.1 主狀態與副狀態
* **主狀態 (如 DriveState)**：負責持續向 `Chassis` 等核心硬體下達移動更新指令。
* **副狀態 (如 IntakeState, MechanismState)**：負責監聽特定的條件並觸發自身的動作。

### 1.2 狀態機介面與用法
```cpp
// 建立一個移動狀態
auto moveState = new MoveToPointState(-13.25, -12.0);

// 建立一個 Intake 狀態，設定它的「啟動守衛 (Guard)」
auto intakeLiftState = new SetLiftState(1);
intakeLiftState->setStartCondition([]() {
    return Chassis::getInstance().getDistanceToTarget() <= 7.0;
});

// 使用 add 將兩個狀態平行加入狀態機，它們會同時被 Update
stateMachine.add(moveState);
stateMachine.add(intakeLiftState);

// 如果想要序列執行，可以使用 then() 語法糖
// moveState->then(new SomeNextState());
```

**優勢**：
* **完全解耦**：`MoveToPointState` 只管底盤，完全不知道 Intake 的存在。
* **支援複雜邏輯**：副狀態被觸發後可以有自己的 Tick 邏輯，甚至可以包含自己的小狀態機，完全不會卡死底盤運作。

## 2. 狀態物件模式 (State Object Modes)

對於需要等待協力廠商函式庫或硬體 API 阻塞操作的情境（如 `vex::wait()`），若將所有阻塞操作拆分成非阻塞的狀態機節點會導致「狀態爆炸」。我們引入了 `Mode` 參數來解決：

* **`Mode::Loop` (預設)**：狀態機每個 Tick 呼叫 `execute()`，必須為非阻塞邏輯，依靠回傳 `is_done()` 結束狀態。
* **`Mode::Once`**：在狀態物件中設定 `mode = Mode::Once`。當狀態機切換到此狀態時，啟動一個背景任務 (VEX Task/Thread) 單次呼叫 `execute()`。此模式允許內含耗時阻塞邏輯（如 `vex::wait()`），執行完畢後會透過無鎖 (Lock-Free) 機制自動回調完成，且不會卡死主執行緒與平行的其他狀態（如 Odometry 更新）。
