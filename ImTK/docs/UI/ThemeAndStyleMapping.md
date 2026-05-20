# ImTK 主題與樣式映射規範 (Theme & Style Mapping Architecture)

本文件定義了 ImTK 框架的高階樣式 API (`VisualElement.Style`) 與全域主題系統 (`ImTKTheme`) 映射至底層 Immediate Mode (ImGui) 樣式的設計規範與原則。

---

## 1. 架構分層與職責釐清

ImTK 的樣式系統分為兩大層級，它們各自負責不同的渲染範圍：

### 1.1 全域主題映射 (`ImTKTheme.ApplyToImGui`)
*   **職責**：負責定義整個應用程式的「基底樣式」。
*   **運作方式**：在每幀渲染起點，直接將 `ImTKTheme` 中定義的語意化 Token (`ColorFamily`) 寫入 `ImGui.GetStyle().Colors`。
*   **影響範圍**：所有未被局部覆寫的標準 ImGui 元素（包含 Scrollbar, Tab, MenuBar 等）。

### 1.2 局部樣式覆寫 (`VisualElement.Style`)
*   **職責**：提供特定節點及其子節點的「局部特化 (Local Specialization)」或「覆寫 (Override)」。
*   **運作方式**：透過 C# 屬性 (如 `element.style.backgroundColor`) 或 StyleSheet 設定，在該元素的 `Render()` 階段，透過 `PushToImGui()` 發出 `ImGui.PushStyleColor` 堆疊指令，渲染結束後 `PopFromImGui()`。
*   **影響範圍**：僅限於該 `VisualElement` 節點範圍內的 ImGui 繪製。

---

## 2. 全新語意化色彩家族 (The Semantic ColorFamily)

為了精確對應 UI 介面的實體結構，不再使用模糊的 bg/fg，`ImTKTheme` 的 `ColorFamily` (包含 normal, success, info, warning, danger) 統一採用以下 14 個核心 Token：

### 2.1 容器與版塊底色 (Surface & Container)
用來區分不同的 UI 區塊層級。
*   **`surface`**: 最基礎的表面底色，例如 Window、Panel 的大面積背景。
    *   *映射至*: `WindowBg`, `ChildBg`
*   **`container`**: 放在 Surface 上的次級容器底色，例如 ScrollView 背景、Group 背景、或是標題列 (TitleBar)。
    *   *映射至*: `PopupBg`, `TitleBg` 系列, `MenuBarBg`

### 2.2 元件本體與互動狀態 (Component Base & Interaction)
專注於按鈕、輸入框、選單項目等「可互動元件」的填色。
*   **`component`**: 互動元件的預設底色。
    *   *映射至*: `FrameBg` (輸入框/勾選框底色), `Button` (預設按鈕底色), `Header`, `Tab`
*   **`componentHover`**: 元件被滑鼠懸停時的狀態。
    *   *映射至*: `FrameBgHovered`, `ButtonHovered`, `HeaderHovered`, `TabHovered`
*   **`componentActive`**: 元件被按住 (Active/Pressed) 時的狀態。
    *   *映射至*: `FrameBgActive`, `ButtonActive`, `HeaderActive`

### 2.3 強調色與選中狀態 (Accent & Selection)
代表當前情境的核心品牌色或高亮色。
*   **`accent`**: 視覺焦點與強調。
    *   *映射至*: `CheckMark`, `SliderGrab`, `ScrollbarGrab`
*   **`accentHover`**: 強調色被懸停時的狀態。
    *   *映射至*: `ScrollbarGrabHovered`
*   **`accentActive`**: 強調色被按下或正在拖拽時的狀態。
    *   *映射至*: `ScrollbarGrabActive`, `SliderGrabActive`
*   **`selection`**: 元件被持續選中 (Selected)、或者是反白文字的底色。通常是帶有透明度的 `accent` 或是較為柔和的強調色。
    *   *映射至*: `TabSelected`, `TabDimmedSelected`, `TextSelectedBg`

### 2.4 輪廓與分隔 (Borders & Dividers)
*   **`border`**: 元件或容器的實體邊界線。
    *   *映射至*: `Border`
*   **`divider`**: 分隔線，用來切分內容區塊。
    *   *映射至*: `Separator`, `SeparatorHovered`, `SeparatorActive` (若無特別強調需求，維持統一柔和色)

### 2.5 文字與內容 (Content)
*   **`text`**: 主要文字。
    *   *映射至*: `Text`
*   **`subText`**: 次要、弱化的說明文字 (目前保留供 VisualElement.Style 覆寫使用)。
*   **`disabledText`**: 禁用狀態的文字。
    *   *映射至*: `TextDisabled`

---

## 3. 調色與對比度規範 (Color Constraints)

為確保使用者體驗與 Accessibility，自訂主題或覆寫顏色時必須遵守以下規範：

1.  **核心對比度保證**：
    *   `text` 和 `subText` 必須能夠在同一個 ColorFamily 內的 `surface`, `container` 以及 `component` 上清晰可見（建議對比度 > 4.5:1）。
    *   **特例**：若某個情境（如 danger）的底色過深，該 Family 的 `text` 必須特化（如設為純白），同一個 Family 的文字必須對其底色負責。
2.  **層級疊加規範 (Z-Stacking Order)**：
    *   在 Dark Theme 下，Z軸越高的元素明度越高：`surface` (暗) < `container` < `component` < `componentHover` (亮)。
    *   在 Light Theme 下，Z軸越高的元素通常越亮（白）或使用相同的明度。
3.  **Accent 的使用限制**：
    *   `accent` 系列專用於吸引眼球的極小面積高亮（如勾勾、把手）。**嚴禁**將高純度的 `accent` 大面積用作包含主要文字的底色（若有需求，應使用柔和的 `selection` 或在 VisualElement 層級特別覆寫文字色為反白）。
4.  **邊界靜默原則**：
    *   `border` 和 `divider` 的對比度應剛好高於其相鄰底色，不可搶過 `text` 或 `accent` 的視覺層級。

---

## 4. VisualElement.Style 映射策略與特殊元件設計

`VisualElement.Style` 透過定義 `StyleKey` (HashedString) 來暴露可覆寫的屬性。

### 4.1 基礎 Style 覆寫原則
基底 `VisualElement.Style` 提供的 Key 影響的是該區域的「基礎屬性」：
*   `BackgroundColor` -> 覆寫 `ImGuiCol.WindowBg` & `ChildBg` (改變 Surface)
*   `BorderColor` -> 覆寫 `ImGuiCol.Border`
*   `TextColor` -> 覆寫 `ImGuiCol.Text`
*   `DisabledTextColor` -> 覆寫 `ImGuiCol.TextDisabled`
*   `SelectionColor` -> 覆寫 `ImGuiCol.TextSelectedBg`

### 4.2 特殊元件 (Specialized Components)
當 ImTK 的封裝元件在 ImGui 層面對應了多種複合狀態（如 Button、Window），應建立專屬的 `[Element].Style` 子類別，並定義特有的 `StyleKey`，最後覆寫 `PushToImGui` 邏輯。

#### Button
作為一個「主動發光體」，按鈕不應被視為普通容器。
*   `BackgroundColor` 應攔截並重新導向至 `ImGuiCol.Button` (覆寫 Component 底色，而非 WindowBg)。
*   提供特化 Key：`HoverColor` (`ImGuiCol.ButtonHovered`) 與 `ActiveColor` (`ImGuiCol.ButtonActive`)。

#### Window
視窗是一個複合容器，具有標題列與可縮放邊界。
建立 `Window.Style` 擴充以下 Key：
*   `TitleBarColor` -> `ImGuiCol.TitleBg`, `TitleBgCollapsed`
*   `TitleBarActiveColor` -> `ImGuiCol.TitleBgActive`
*   *(未來擴充)*: `ResizeGripColor` 系列。
這允許開發者實現「無邊框且標題列透明」的客製化視窗，而不影響全域 Theme 的 `container` 設定。

#### TextField & CheckBox
作為一般的輸入與互動組件，其背景映射至 ImGuiCol.FrameBg。
建立 `TextField.Style` 與 `CheckBox.Style` 擴充/攔截以下 Key：
*   `BackgroundColor` -> 攔截並重新導向至 `ImGuiCol.FrameBg`
*   `HoverColor` -> `ImGuiCol.FrameBgHovered`
*   `ActiveColor` -> `ImGuiCol.FrameBgActive`
*   *(CheckBox 特有)* `CheckMarkColor` -> `ImGuiCol.CheckMark`

#### MenuView & MenuItem
選單系統擁有不同的複合底層 ImGui 變數。
*   **`MenuView.Style`**: `BackgroundColor` 映射至 `ImGuiCol.PopupBg` 以及 `ImGuiCol.MenuBarBg`。
*   **`MenuItem.Style`**: 作為點擊項目，`BackgroundColor` 映射至 `ImGuiCol.Header`。並提供特化 `HoverColor` (`ImGuiCol.HeaderHovered`) 與 `ActiveColor` (`ImGuiCol.HeaderActive`)。
