# Odometry (里程計與定位) 規格

本文件定義了 GCVex 專案中的里程計定位模組。該模組負責持續追蹤機器人在場地上的絕對座標與朝向 $(X, Y, \Theta)$。

## 1. 介面抽象化：`ILocator` / `IPoseTracker`

為了支援未來多種不同的感測器配置（甚至感測器融合），里程計被抽象為 `ILocator` 介面。
具體的實作（例如 `OdomOneWheel`, `OdomTwoWheels`, 甚至是結合 GPS 的版本）都必須繼承此介面，並由使用者在 `Application` 的生命週期中進行綁定與註冊。

## 2. 核心參數配置

無論是哪種實作，通常需要依賴以下物理常數（建議封裝至配置結構體中）：
* **`WHEEL_DIAMETER`**: 追蹤輪直徑（英吋），用於將編碼器數值轉換為物理移動距離。
* **`VERTICAL_OFFSET`**: 縱向追蹤輪距離機器人旋轉中心的橫向偏移量。
* **`HORIZONTAL_OFFSET`**: 橫向追蹤輪距離機器人旋轉中心的縱向偏移量。

## 3. 數學模型：Arc Integration (弧線積分)

為了提高機器人在高速過彎時的定位精度，底層必須採用**弦長 (Chord Length)** 或**弧線積分**來計算增量，而非單純的直線近似。

```cpp
double halfTheta = deltaTheta / 2.0;
double chordY, chordX;

if (fabs(deltaTheta) < 0.0001) { // 直線行駛 (無旋轉)
    chordY = localY;
    chordX = localX;
} else { // 轉彎時，計算實際弦長
    chordY = 2.0 * (localY / deltaTheta) * sin(halfTheta);
    chordX = 2.0 * (localX / deltaTheta) * sin(halfTheta);
}

// 將弦長向量依據平均角度旋轉至全域座標系
x += chordY * sin(avgTheta) + chordX * cos(avgTheta);
y += chordY * cos(avgTheta) - chordX * sin(avgTheta);
```
這能顯著減少連續過彎時的里程計漂移。
