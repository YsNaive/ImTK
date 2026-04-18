# ImTK Documentation (專案文檔庫)

歡迎來到 ImTK 專案的技術知識庫。ImTK 是一個基於 C# 與 .NET 平台的 Retained Mode 介面框架，底層封裝了高效能的 ImGui (Immediate Mode GUI)，並提供了現代化的物件導向 UI 樹狀結構。

本知識庫包含了整個 ImTK 框架的架構設計、開發規範與核心機制的深入解析。

---

## 📂 專案管理與規範 (Project-level)
所有開發者與 AI 參與開發前，必須了解的最高指導原則：

* [AI 代理開發指南 (AGENT)](./Project/AGENT.md)
* [當前開發任務 (TODO)](./Project/TODO.md)
* [命名與開發規範 (Naming Conventions)](./Project/NamingConventions.md)
* [文檔撰寫與結構規範 (Doc Standards)](./Project/DocStandards.md)
* [合併前最終檢查清單 (Development Wrap-Up SOP)](./Project/DevelopmentWrapUp.md)

---

## 🛠️ 核心框架與子模組 (Core Framework & Modules)

* **[ImTK 核心機制](./ImTK/README.md)**：系統級功能核心，涵蓋 `ImTKModule` 的自動註冊與生命週期。
* **[ImTK.Silk 整合橋接](./ImTK_Silk/README.md)**：Silk.NET 視窗整合與生命週期驅動，以及 ImGui.NET 的記憶體指標管理。
* **[VisualElement 基礎架構](./VisualElement/README.md)**：核心 UI 架構設計，解析邏輯/物理雙層樹狀結構與安全走訪機制。
* **[Window 視窗系統](./Window/README.md)**：視窗管理元件，支援單例工具面板與動態多實例的序列化狀態持久化設計。
* **[RuntimeDrawer 繪製架構](./RuntimeDrawer/README.md)**：資料交互與呈現元件，支援 Unity 風格的縮排排版、型別綁定與事件冒泡機制。
