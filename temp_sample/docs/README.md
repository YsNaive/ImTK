# GCVex Documentation (專案文檔庫)

歡迎來到 GCVex 專案的技術知識庫。這裡包含了整個 VEX Robotics 框架的架構設計、通訊協定與開發規範。

我們採用了模組化的文檔結構。每一個子資料夾都代表一個獨立的系統，並包含該系統的規格書 (`README.md`) 與歷史設計決策 (`CHANGELOG.md` / `DESIGN_NOTES.md`)。

---

## 📂 專案管理與規範 (Project-level)
所有開發者與 AI 參與開發前，必須了解的最高指導原則：

* [AI 代理開發指南 (AGENT)](./Project/AGENT.md)
* [當前開發任務 (TODO)](./Project/TODO.md)
* [命名與開發規範 (Naming Conventions)](./Project/NamingConventions.md)
* [文檔撰寫與結構規範 (Doc Standards)](./Project/DocStandards.md)
* [合併前最終檢查清單 (Development Wrap-Up SOP)](./Project/DevelopmentWrapUp.md)

---

## 🛠️ 核心框架與模組 (Core Framework & Modules)

* **[Application](./Application/README.md)**：事件驅動的生命週期總管與 `ISubSystem` 介面。
* **[Debug](./Debug/README.md)**：高效能、解耦的統一日誌與除錯渲染系統。
* **[Dashboard](./Dashboard/README.md)**：外部 C# 連線通訊協定、二進位序列化與 Entity 代理系統。
* **[Chassis](./Chassis/README.md)**：底盤運動協調器與動力系統抽象介面。
* **[Odometry](./Odometry/README.md)**：里程計、絕對座標定位與弧線積分數學模型。
* **[PID Controller](./PID/README.md)**：Time-based 非阻塞 PIDF 閉迴路控制器。
* **[Motion Profiling](./MotionProfiling/README.md)**：梯形速度曲線規劃與動態容錯模式。
* **[Auto Flow](./AutoFlow/README.md)**：自動階段的平行狀態機與非阻塞任務腳本架構。
