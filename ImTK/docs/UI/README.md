# 視覺與佈局 (UI)

本目錄包含 ImTK 將 ImGui (Immediate Mode) 封裝為 Retained Mode 物件樹的核心設計。

## 包含的文件：

* **[`VisualTree.md`](VisualTree.md)**：詳述 `VisualElement` 的邏輯樹與物理樹分離架構 (`contentContainer` 與 `IVisualElementHierarchy`)，以及避免 ID 衝突的自動 Hash ID 封裝。
* **[`EventSystem.md`](EventSystem.md)**：說明事件解耦延遲派發、事件冒泡路由、Object Pool 設計，以及利用 ImGui 原生狀態推導滑鼠事件的混合架構。
* **[`PanelLayout.md`](PanelLayout.md)**：記載全域佈局管理者 (`Panel`) 透過註冊 Rect 切割函式實作 MenuBar / StatusBar 等預留空間演算法的設計。
* **[`MenuSystem.md`](MenuSystem.md)**：選單系統設計，包含 `MenuView` 與 `MenuItem` 職責拆分、Priority 自動排序與分隔線，以及路徑衝突處理機制。

* [**Element/**](Element/)
  存放並記錄各種內建的高階 UI 元件 (如 `Button`) 規格與事件綁定模式。
