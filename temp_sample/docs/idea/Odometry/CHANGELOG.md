# Odometry 歷史演進與重構紀錄

## 從舊版 Euler Integration 升級

在早期的 `./old/src/odometry.cpp` 中，使用了**兩顆追蹤輪 + IMU** 的配置，且計算模型存在兩個痛點：

### 1. 耦合的分支判斷
舊版依賴全域巨集 `USE_HORIZONTAL_TRACKER` 來決定是否要處理橫向追蹤輪。這種做法破壞了物件導向設計，我們透過抽象出 `ILocator` 介面，並提供多種具體實作徹底解決了這個問題。

### 2. 精度不足的 Euler Integration
舊版數學模型使用 Average Theta 將微小的運動近似為直線：
* $\Delta X = LocalY \cdot \sin(Avg\Theta) + LocalX \cdot \cos(Avg\Theta)$

雖然在低速或高頻率 Tick 下誤差不大，但在高速過彎時會產生明顯的漂移。這促使我們在目前的架構中升級為 **Arc Integration (弧線積分)** 演算法，以弦長代替直線進行全域座標的轉換。
