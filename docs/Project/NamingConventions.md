# 命名與開發規範 (Naming Conventions)

本專案遵循 C# 社群的標準命名習慣，並針對 ImTK 的 UI 元件分類制定了嚴格的後綴規範，以確保開發者能從型別名稱中直覺辨識該元件的用途。

| 元素類型 (Element Type) | 命名規則 (Naming Rule) | 範例 (Example) |
| :--- | :--- | :--- |
| **類別 / 結構體 (Class / Struct)** | `PascalCase` | `VisualElement`, `Button` |
| **介面 (Interface)** | `I` + `PascalCase` | `IFieldElement`, `IVisualElementHierarchy` |
| **公開屬性 / 方法 (Public Property / Method)** | `camelCase` | `enableDocking`, `RenderVisualTree()` |
| **私有 / 保護欄位 (Private / Protected Field)**| `m_` + `camelCase` | `m_children`, `m_iterationCount` |
| **靜態私有欄位 (Private Static Field)** | `s_` + `camelCase` | `s_usedWindowNames` |
| **區域變數 / 參數 (Local Variable / Parameter)**| `camelCase` | `deltaTime`, `targetElement` |

---

## UI 元件後綴規範 (UI Element Suffix Conventions)

為了維持框架架構的清晰性，繼承自 `VisualElement` 的各類 UI 元件必須嚴格遵守以下命名規範：

1. **一般互動元件 (無後綴)**
   - **格式**：`{name}`
   - **說明**：基礎的、不可再細分的互動或展示元件。
   - **範例**：`Button`, `Toggle`, `TextElement`

2. **排版與佈局元件 (View)**
   - **格式**：`{name}View`
   - **說明**：主要職責為容器，負責管理內部子元件的空間排列或滾動等佈局行為。
   - **範例**：`HorizontalView`, `ScrollView`

3. **資料交互元件 (Field / Slider)**
   - **格式**：`{name}Field` 或 `{name}Slider`
   - **說明**：允許使用者輸入或編輯資料的欄位，通常實作 `IFieldElement` 或繼承自 `FieldElement<T>`。
   - **範例**：`TextField`, `IntSlider`, `FloatField`

4. **視窗層級元件 (Window)**
   - **格式**：`{name}Window`
   - **說明**：凡是繼承自基底類別 `Window`，代表一個獨立浮動的 OS/ImGui 級別視窗的類別。
   - **範例**：`ToolWindow`, `DebugWindow`
