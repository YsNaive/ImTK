# 佈局與排版引擎 (UI Layout)

本模組探討 ImTK 如何在 Immediate Mode (ImGui) 的基礎上，建構一套高效的四階段管線 (Four-Phase Pipeline) 與輕量級 Flexbox (Yoga-lite) 排版引擎，以及負責全域視窗切割的 `Panel` 佈局管理。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 核心排版引擎與管線 (Pipeline)

*   **[`RenderEngine`](../../../UI/RenderEngine.cs)**: 負責全域的四階段生命週期。
    *   **Build Phase**: 建立 O(1) 渲染快取。
    *   **Measure Phase**: Top-Down 遞迴計算元件理想尺寸 (`desiredSize`)，處理文字換行。
    *   **Arrange Phase**: 設定元件最終絕對座標 (`layoutRect`)。
    *   **Render Phase**: 實際呼叫 ImGui 進行純繪圖。

### 2. 頂層佈局管理器 (Top-Level Layout)

*   **[`Panel`](../../../UI/Panel.cs)**: 管理全域 ImGui Viewport，支援自訂邊緣選單與中央停靠區 (DockSpace)。
    *   `RequireArea(int priority, Func<Rect, Rect> reservedFunc)`: (初始化階段呼叫) 註冊切割函式，要求並保留特定長寬的空間 (例如給 MenuBar 使用)。

### 3. Flexbox 排版屬性 (Style Properties)

所有 `VisualElement` 皆可透過 `style.xxx = ...` 設定以下排版屬性（屬性名稱採小寫駝峰）：

*   **尺寸與空間 (Box Model)**
    *   `width`, `height`, `minWidth`, `maxWidth`, `minHeight`, `maxHeight` (型別：`StyleValue<float>`)
    *   `padding`, `margin` (型別：`StyleThickness`)
*   **容器佈局 (Flex Container)**
    *   `flexDirection`: 子節點排列方向 (`Row`, `Column`)。
    *   `flexWrap`: 是否允許自動換行 (`NoWrap`, `Wrap`)。
    *   `justifyContent`: 主軸對齊 (`FlexStart`, `Center`, `FlexEnd`, `SpaceBetween`)。
    *   `alignItems`: 交叉軸對齊 (`FlexStart`, `Center`, `FlexEnd`, `Stretch`)。
*   **子節點彈性與絕對定位 (Flex Item & Position)**
    *   `flexGrow`: 剩餘空間的等比例分配權重。
    *   `alignSelf`: 單獨覆寫交叉軸的對齊方式。
    *   `positionType`: `Relative` (依賴 Flex 排版), `Absolute` (脫離排版，手動定位)。
    *   `top`, `bottom`, `left`, `right`: 絕對定位的偏移值。
*   **顯示與隱藏 (Visibility)**
    *   `display`: `Flex` (顯示), `None` (隱藏且不佔用空間，具有 O(1) 濾除效能)。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討排版底層演算法與設計模式的技術文件：

*   **[`LayoutEngineArchitecture.md`](LayoutEngineArchitecture.md)**：深入詳解四階段 Pipeline 的設計理念、`MeasureMode` 空間約束機制、與 ImGui 的佔位交握 (Dummy) 策略，以及隔離髒標記 (Dirty Flag) 的優化。
*   **[`PanelLayout.md`](PanelLayout.md)**：解析頂層 `Panel` 如何利用 `Reserved Area API` 放棄原生的 `BeginMainMenuBar`，實作出完全客製化且安全的邊緣空間佈局。
