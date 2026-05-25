# TODO / Task Tracker

這裡追蹤 ImTK 專案的功能開發與重構進度。
已完成的項目將記錄於根目錄的 `CHANGELOG.md` 中。

**AI Agent 注意**：嚴禁自動標記完成，必須與使用者確認後才能移動或刪除任務狀態。

## 🟢 進行中 (In Progress)
* （無）

## 🔵 待處理 (To Do)
* **[UI 系統]** 排版引擎 (Layout Phase) 開發，承接 `RenderEngine` 的初步重構，將排版與渲染雙階段徹底分離。
* 擴充 MenuItem 的 Shortcut 功能（包含全域捷徵監聽與 ImGui 字串顯示）。

## 🟡 技術債 (Tech Debt)
* **[RenderEngine]** 移除 `RenderNode` 的隱性雙層架構。目前 `RenderNode` 是 flat 架構之外的一個局部渲染入口，存在兩個使用場景需要在 Layout Engine 完成後統一：
  1. **MenuView 手動渲染子節點**：`MenuView.OnBeginRender()` 內部依 `priority` 差值插入 `Separator` 並直接呼叫 `RenderNode(child)`，脫離主走訪。待 Layout Engine 支援排版掛勾後，可改由主 flat 走訪正確處理，`RenderNode` 呼叫可移除。
  2. **Drawer 的離樹子元件**（`Vector2Drawer`、`RectDrawer` 等）：`m_xDrawer`/`m_yDrawer` 刻意不掛入視覺樹，以 `overrideRenderRect` 進行絕對定位渲染。待 Layout Engine `Arrange()` 能正確輸出每個子元件的 `layoutRect` 後，可將這些子 Drawer 正式加入 `hierarchy`，由主走訪統一渲染，`overrideRenderRect` 機制與 `RenderNode` 皆可退場。
