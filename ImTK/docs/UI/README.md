# 視覺與佈局 (UI)

本目錄包含 ImTK 將 ImGui (Immediate Mode) 封裝為 Retained Mode 物件樹的核心設計。

## 包含的文件：

* **[`VisualTree.md`](VisualTree.md)**：詳述 `VisualElement` 的邏輯樹與物理樹分離架構 (`contentContainer`)，以及為避免「迭代中修改集合」而設計的延遲同步機制。
