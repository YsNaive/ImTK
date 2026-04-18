# 生命週期管理與系統框架 (Application Lifecycle)

本文件描述了 `gcvex::Application` 的生命週期管理系統，以及新增加的物件導向子系統抽象層 `ISubSystem`。

## 1. 核心概念

整個 VEX 機器人的生命週期管理是由 `gcvex::Application` 接管的。該模組不再依賴傳統的巨大繼承樹 (Super-loop 或是複雜的 SubSystem 基礎類別來管理所有的機器人邏輯)，而是採用 **Event-Bus / Delegate (事件委派)** 的架構，允許開發者將特定的函數（或物件的成員函數）動態註冊到系統中。

### 1.1 生命週期階段
系統分為五個主要的生命週期事件：
* `init`: 於 `pre_auton` 階段觸發。**禁止**在此存取其他子系統或外部動態變數，專注於本身元件的記憶體分配與建構。
* `start`: 於 `init` 完成後觸發。這是一個安全的階段，允許子系統之間互相存取、進行依賴注入（Dependency Injection）。
* `enable`: 於進入 `autonomous` 或 `usercontrol` 模式時觸發，適合進行數值重置（如重置 PID 積分或編碼器）。
* `disable`: 於離開 `autonomous` 或 `usercontrol` (進入 Disabled 狀態) 時觸發。
* `loop`: 於主迴圈中依據指定的 `interval` 定期持續觸發 (`cb(time, dt)`)，不論當前是否 Disable 都會執行。絕對禁止在此執行阻塞操作 (如 `vex::wait()`)。

## 2. 物件導向封裝：`ISubSystem`

雖然 `gcvex::Application` 提供了強大的函數指標 (`std::function`) 註冊機制，但為了方便開發者以**物件導向 (OOP)** 的方式撰寫獨立的硬體或邏輯模組（例如：底盤控制、里程計定位），我們提供了 `ISubSystem` 基礎介面。

### 2.1 RAII 自動註冊機制
`ISubSystem` 利用 C++ 的 RAII (Resource Acquisition Is Initialization) 特性，將生命週期的註冊與註銷完美綁定在建構子與解構子中：
* **建構時**：自動將物件的虛擬方法 (`init`, `start`, `enable`, `disable`, `loop`) 綁定到 `gcvex::Application`。
* **解構時**：自動呼叫 `unregister` 卸載，避免記憶體洩漏或 dangling pointer 的風險。

### 2.2 強制唯一性與防呆設計
為了防止在賽場上意外建立兩個控制相同硬體的系統（例如建立兩個 Odometry 定位系統導致互相搶佔），`ISubSystem` 具備強制唯一性檢查：
* 建構時必須透過 `protected` 建構子傳遞一個唯一的 `name` 字串。
* 內部維護了一個註冊表（隱藏在匿名空間，以 `vex::mutex` 保護執行緒安全）。
* 若發現相同的 `name` 已經存在，系統會呼叫 `gcvex::Debug::raise` 觸發 Fatal Error 畫面，並立刻中斷程式執行。

### 2.3 實作範例

開發者只需要繼承 `ISubSystem`，在建構子初始化列表提供名稱，並覆寫需要的生命週期函數即可：

```cpp
#include "framework/ISubSystem.h"

class MyOdometry : public gcvex::Application::ISubSystem {
public:
    // 強制在建構時提供唯一名稱 "Odometry"，並設定 loop 間隔為 10ms
    MyOdometry() : ISubSystem("Odometry", 10) {}

    // 選擇性覆寫需要的生命週期
    void start() override {
        // 綁定其他子系統的指標或進行依賴注入
    }

    void enable() override {
        // 重置編碼器數值
    }

    void loop(int time, int dt) override {
        // 進行持續非阻塞的里程計數學推導
    }
};

// 只要實例化物件，就會自動註冊，並受到唯一性保護
MyOdometry* odom = new MyOdometry();
```

### 2.4 主迴圈暫停機制 (Pause & Resume)
`gcvex::Application::run()` 接管了 VEX 主迴圈，負責持續派發 `loop` 事件。如果在開發除錯期間需要暫停：
* 可以呼叫 `gcvex::Application::pause()` 來暫停派發所有的 `loop` 事件，此時所有的背景迴圈包含硬體控制、`debug` 繪製或其他子系統的執行都會停止，但不會終止已經發生的動作。
* 呼叫 `gcvex::Application::resume()` 可以恢復 `loop` 的派發。
* 狀態可透過 `gcvex::Application::isPaused()` 來檢查。

### 2.5 致命錯誤機制 (Fatal Error)
當呼叫 `gcvex::Application::raise` (或 `gcvex::Debug::raise`) 時，系統會進入致命錯誤狀態：
* 當前呼叫的子線程會立即被無窮休眠卡死。
* 主線程 (`run()`) 偵測到致命錯誤後，會**主動且唯一**地終止所有 VEX 任務 (`vex::task::stopAll()`)。
* 主線程親自清空畫面並畫上紅色錯誤訊息，確保畫面不會被其他子系統（如 `debug_loop`）覆蓋。
* 最後，主線程本身也會進入無窮休眠，永遠凍結系統狀態。
