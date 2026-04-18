# Motion Profiling (梯形速度曲線) 解析與重構指南

## 1. 概念與目的

在點到點移動與不同馬力情境的參數調整中，業內（如 VEX 競賽、FRC、自動駕駛）通常選擇**速度曲線 (Motion Profiling / 梯形速度控制)**。

### 1.1 為什麼選擇 Motion Profiling？
純 PID 無論怎麼縮放，其數學本質是**「以終點為唯一目標」**。
* **起步暴衝**：目標在遠方，誤差極大，P 項瞬間給出極大的輸出。
* **減速震盪**：接近目標時，速度必須精準降到 0，純 PID 很難在「快速到達」和「不煞不住（Overshoot）」間取得平衡。

**Motion Profiling 的本質是「規劃一個理想的物理過程」：**
1. **規劃階段**：給定機器人物理極限（最大加速度 $A_{max}$、減速度 $D_{max}$、目標最高速度 $V_{max}$）。演算法規劃出每一個時間點 $t$，機器人「應該」在哪裡 ($P_{ref}$) 和速度「應該」是多少 ($V_{ref}$)。
2. **跟隨階段 (前饋 F + 修正 PID)**：
   - **前饋 (Feedforward)**：直接計算出需要的理論電壓：$V_{out} = K_v \cdot V_{ref} + K_a \cdot A_{ref} + K_s$（$K_s$ 是克服靜摩擦的最小電壓）。
   - **修正 (PID)**：PID 的任務是去追「當前時間點的理想位置 $P_{ref}$」。理想位置一直在動，PID 的誤差永遠很小。

### 1.2 純數學模型：`TrapezoidalProfile`
`TrapezoidalProfile` 本身應是一個純粹的數學計算器，完全不依賴狀態機的 Tick 頻率。

1. **建構與規劃 (Plan)**：
   傳入物理限制 ($V_{max}, A_{max}, D_{max}$) 和總距離 $D$。它在內部計算好 $t_a, t_c, t_d$ 以及對應的距離，這是一次性的數學運算。

2. **查詢 (Query/Calculate)**：
   提供一個數學查詢介面，例如 `ProfileState calculate(double current_time_t)`。
   當傳入時間 $t$，它回傳在該時間點理想的：
   - 位置 ($P_{ref}$)
   - 速度 ($V_{ref}$)
   - 加速度 ($A_{ref}$)

---

## 2. 數學模型探討

### 2.1 核心輸入參數 (Constraints)
* $V_{max}$：最大巡航速度 (例如 50 in/s)
* $A_{max}$：最大加速度 (例如 100 in/s²)
* $D_{max}$：最大減速度 (通常與 $A_{max}$ 相同或略大，例如 120 in/s²)
* $Distance$：總移動距離

### 2.2 三個運動階段 (Phases) 的計算
給定總距離 $D$，計算出三個階段的時間點與距離：

* **階段 1：加速期 (Acceleration)**
  - 速度從 0 等加速到 $V_{max}$。
  - 所需時間：$t_a = \frac{V_{max}}{A_{max}}$
  - 加速距離：$d_a = \frac{1}{2} \cdot A_{max} \cdot t_a^2$

* **階段 2：減速期 (Deceleration)**
  - 速度從 $V_{max}$ 等減速到 0。
  - 所需時間：$t_d = \frac{V_{max}}{D_{max}}$
  - 減速距離：$d_d = \frac{1}{2} \cdot D_{max} \cdot t_d^2$

* **邊界條件處理 (短距離移動)**：
  - 如果總距離 $D < d_a + d_d$，代表**「還沒加速到最高速，就必須開始減速了」**（這會變成一個三角形曲線）。
  - 此時必須重新計算最高速度：$V_{peak} = \sqrt{\frac{2 \cdot D \cdot A_{max} \cdot D_{max}}{A_{max} + D_{max}}}$，並用 $V_{peak}$ 重新計算 $t_a$ 和 $t_d$。

* **階段 3：巡航期 (Cruise)**
  - 如果距離夠長 ($D \ge d_a + d_d$)，就會有一段等速巡航的距離。
  - 巡航距離：$d_c = D - d_a - d_d$
  - 巡航時間：$t_c = \frac{d_c}{V_{max}}$

### 2.3 執行階段 (即時計算參考點)
計算出總時間 $(t_a + t_c + t_d)$ 後，在時間 $t$ 時刻，給出當下**「理想位置 $P_{ref}$」**和**「理想速度 $V_{ref}$」**：

* 如果 $t \le t_a$ (正在加速)：
  $V_{ref} = A_{max} \cdot t$
  $P_{ref} = \frac{1}{2} \cdot A_{max} \cdot t^2$
* 如果 $t_a < t \le t_a + t_c$ (正在巡航)：
  $V_{ref} = V_{max}$
  $P_{ref} = d_a + V_{max} \cdot (t - t_a)$
* 如果 $t > t_a + t_c$ (正在減速)：
  $t_{dec} = t - (t_a + t_c)$
  $V_{ref} = V_{max} - D_{max} \cdot t_{dec}$
  $P_{ref} = (d_a + d_c) + V_{max} \cdot t_{dec} - \frac{1}{2} \cdot D_{max} \cdot t_{dec}^2$

### 2.4 閉迴路控制 (結合 PIDF)
Profiler 給出理想點後，交給 PIDF 控制器：
```cpp
// 1. 前饋 (Feedforward)：預期出力
double feedforward = Kv * V_ref + Ka * A_ref;

// 2. 回饋修正 (Feedback)：修正環境干擾
double error = P_ref - Chassis.getDistance();
double feedback = pid.calculate(error);

// 3. 最終輸出
double outputVoltage = feedforward + feedback;
```
這套模型會被實作為 `TrapezoidalProfile`，由 `Chassis` 協調器呼叫。

## 3. 進階架構擴充：時間驅動 vs. 距離驅動 (Query 基點)

在實戰中，如果僅依靠時間 $t$ 作為查詢基準，機器人在受到碰撞或阻擋時，時間依然流逝，導致理想位置 $P_{ref}$ 持續增加，一旦障礙物移除，機器人會因為巨大的累積誤差而暴衝。

為了提高系統的強健性與安全性，速度分析推算模塊應提供兩種不同的實作策略供 `Chassis` 選擇：

### 3.1 時間驅動模型 (`TimeTrapezoidalProfile`)
* **Query 基點**：時間 $t$ (`calculateByTime(double t)`).
* **特徵**：數學上最精確，適合空曠場地或需要嚴格遵守時序的技能自動賽 (Skill Auto)。
* **痛點**：碰撞停滯後容易累積巨大的追蹤誤差。

### 3.2 距離/位置驅動模型 (`DistanceTrapezoidalProfile`)
* **Query 基點**：當前已行駛距離或剩餘距離 (`calculateByDistance(double current_dist, double remaining_dist)`).
* **特徵**：利用運動學公式 $V_{ref} = \sqrt{V_0^2 + 2 \cdot a \cdot \Delta x}$ 動態計算當下應該擁有的速度。
* **優勢**：如果機器人卡住（$\Delta x$ 沒有變化），Profiler 給出的 $V_{ref}$ 也會停滯不前，完美避免了障礙排除後的暴衝危險。非常適合對抗賽 (Match Play)。
* **注意點**：起步瞬間若速度為 0 且距離未變，需要設定一個最小啟動速度 ($V_{start}$) 以克服靜摩擦力。

## 4. 進階架構擴充：動態容錯模式與硬體驗證 (Dynamic Fallback & Validation)

時間驅動與距離驅動不僅是互斥的選擇，更應該設計為**相輔相成的複合模型**。

### 4.1 開發期的硬體常數驗證 (Hardware Validation)
* **機制**：在開發期以時間模式 (`Mode.Time`) 為主，同時開啟距離驗證。
* **優勢**：當狀態機計算出理論時間 $t$ 該到達的理想位置 $P_{ref}$，卻發現與實際距離 $D_{real}$ 誤差過大時，這能迅速幫助開發者抓出硬體常數設定錯誤（如：馬達最大轉速設定錯誤、追蹤輪直徑 `WHEEL_DIAMETER` 設錯、或實際加速度達不到設定的 $A_{max}$）。

### 4.2 實戰期的動態降級 (Safe Mode / Dynamic Fallback)
在實戰（如 Autonomous 階段）中，機器人可能遭遇碰撞或卡住。

* **模式定義**：可以設計如 `Mode.Time`, `Mode.Distance`, `Mode.SafeHybrid` 等模式。
* **實戰應用 (`Mode.SafeHybrid`)**：
  * **主依據**：初期以時間驅動為主，確保整套自動程式的各動作時序完美契合（例如準時到達定點發射）。
  * **觸發降級**：系統持續監控「時間算出的理想位置 $P_{ref}$」與「實際里程計位置」的誤差。一旦誤差超過安全閾值（例如 > 10 英吋），代表機器人**受阻或卡住**。
  * **無縫切換**：系統自動且瞬間降級為距離驅動 (`Mode.Distance`)，放棄追趕時間進度，轉而穩健地根據「剩餘距離」完成移動，確保最終位置正確且防止累積過大 P 項誤差而導致暴衝翻車。這為系統提供了極高的**工業級健壯性 (Robustness)**。
