# 視覺控制項與視窗 (UI Elements)

本模組存放了 ImTK 框架內建的標準 UI 元件、視窗系統、選單系統以及用於屬性面板的 Drawer 系統。所有的元件皆繼承自最底層的 [`VisualElement`](../../../UI/Element/Basic/VisualElement.cs)。

---

## ⚡ 快速速查表 (Quick Reference)

### 1. 基礎控制項 (Basic Elements)

*   **[`VisualElement`](../../../UI/Element/Basic/VisualElement.cs)**: 所有 UI 元件的基底類別。提供排版 (`style`) 與事件系統。
*   **[`Button`](../../../UI/Element/Basic/Button.cs)**: 標準按鈕。支援 `clicked` 事件回呼。
*   **[`Label`](../../../UI/Element/Basic/Label.cs)**: 單行純文字標籤。
*   **[`TextElement`](../../../UI/Element/Basic/TextElement.cs)**: 進階文字元件，支援自動折行 (`wordWrap`)。
*   **[`ScrollView`](../../../UI/Element/Basic/ScrollView.cs)**: 滾動視圖容器。支援 `Horizontal` 與 `Vertical` 滾動。
*   **[`SplitView`](../../../UI/Element/Basic/SplitView.cs)**: 提供可拖曳分隔線的佈局容器 (如左右分屏)。
*   **[`TreeView`](../../../UI/Element/Basic/TreeView.cs) / [`TreeNode`](../../../UI/Element/Basic/TreeNode.cs)**: 樹狀階層視圖與節點。
*   **[`IconElement`](../../../UI/Element/Basic/IconElement.cs)**: 顯示 FontAwesome 或其他字體圖示的元件。

### 2. 視窗系統 (Windows)

*   **[`Window`](../../../UI/Element/Basic/Window.cs)**: 浮動視窗的基底類別。支援 Docking、停靠狀態還原、以及透過 `OnGuiRender` 覆寫客製化內容。

### 3. 選單系統 (Menu)

*   **[`MenuView`](../../../UI/Element/Menu/MenuView.cs)**: 選單容器（如 `File`），提供 `AddItem` 路徑語法糖。
*   **[`MenuItem`](../../../UI/Element/Menu/MenuItem.cs)**: 選單中的末端可點擊項目。
*   **[`MainMenuAttribute`](../../../UI/Element/Menu/MainMenuAttribute.cs)**: 用於靜態方法，自動掛載全域選單 (如 `[MainMenu("File/Save", 10)]`)。

### 4. 資料綁定繪製器 (Drawer System)

用於自動生成屬性面板 (Inspector) 的控制項，均繼承自 `FieldDrawer<T>`。

*   **[`FieldDrawer<T>`](../../../UI/Element/Drawer/FieldDrawer.cs)**: 雙向資料綁定的基底類別。支援 `value` 屬性與 `RegisterValueChangedCallback`。
*   **基礎型別 Drawer**: `IntDrawer`, `FloatDrawer`, `StringDrawer`, `BoolDrawer`, `ColorDrawer`, `EnumDropdownDrawer` 等。
*   **向量型別 Drawer**: `Vector2Drawer`, `Vector3Drawer`, `RectDrawer` 等。
*   **[`ObjectDrawer`](../../../UI/Element/Drawer/Drawers/ObjectDrawer.cs)**: 支援遞迴展開 C# 巢狀物件的反射面板。
*   **特殊屬性標籤**: `[SliderInt]`, `[SliderFloat]`, `[CustomFieldDrawer]` 用於客製化面板呈現。

---

## 📖 技術架構文件導覽 (Technical Documents)

本目錄下包含以下探討元件設計與底層機制的技術文件：

*   **[`BasicElements.md`](BasicElements.md)**：設計新 UI 元件的規範 (包含獨立渲染層與事件轉發)。
*   **[`Button.md`](Button.md)**：按鈕實作細節與狀態機說明。
*   **[`FieldDrawer.md`](FieldDrawer.md)**：探討 Drawer 的雙向綁定機制、工廠模式 (Factory) 與攔截更新 (`SetValueWithoutNotify`) 技巧。
*   **[`../MenuSystem.md`](../MenuSystem.md)**：選單系統的排序機制、自動分隔線與防衝突設計。
*   **[`../Window/README.md`](../Window/README.md)**：視窗系統的工作區持久化機制與生命週期防呆。
