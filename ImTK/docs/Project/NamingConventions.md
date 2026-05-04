# 命名與開發規範 (Naming Conventions)

本專案遵循 C# 社群的標準命名習慣，並針對 ImTK 的特性與 UI 元件分類制定了嚴格的後綴與大小寫規範，以確保開發者能從變數與函式名稱中直覺辨識其可見度與用途。

## 命名規範矩陣 (Naming Conventions Table)

| 元素類型 (Element) | 存取修飾 (Access) | 靜態/實例 | 命名規則 (Case) | 範例 (Example) | 備註 (Notes) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Class / Struct** | Any | Any | PascalCase | `ImTKModule`, `Window` | |
| **Interface** | Any | Any | IPascalCase | `IDrawer`, `ILogSink` | 字首大寫 `I` |
| **Enum** | Any | Any | PascalCase | `ApplicationState` | |
| **Method (Public)**| Public | Any | PascalCase | `Initialize()`, `RegisterObject()` | 對外的公開 API |
| **Method (Override)**| Public / Protected | Any | PascalCase | `OnLogicUpdate()` | 覆寫生命週期或公開合約 |
| **Method (Util)** | **Protected** | Any | **camelCase** | `setupDefaults()`, `calculateBounds()`| **[特例]** 供子類呼叫的繼承鍊工具函式 |
| **Method (Util)** | **Private** | Any | **_camelCase** | `_updateInternalState()` | **[特例]** 僅限自身的私有工具函式，前綴 `_` |
| **Property / Field**| Public | Any | PascalCase | `DisplayName`, `CurrentState` | 一般的公開資料 |
| **Property / Field**| Public / Protected | Instance | **camelCase** | `enabled`, `isHovered` | **[特例]** 核心狀態、開關旗標（仿造 Unity 風格） |
| **Field (Variable)**| Private / Internal | Instance | **m_camelCase** | `m_pendingAdd`, `m_enabled` | 私有/內部成員變數，字首 `m_` |
| **Field (Variable)**| Private / Internal | Static | **s_camelCase** | `s_minAllowedState` | 私有/內部靜態變數，字首 `s_` |
| **Constant** | Any | Static | PascalCase | `MaxCount`, `DefaultPath` | |
| **Event / Delegate**| Public / Protected | Instance | **camelCase** | `onValueChanged` | 事件委派使用 `on` 開頭 |
| **Local Variable** | N/A | N/A | camelCase | `targetObj`, `count` | 區域變數 |
| **Parameter** | N/A | N/A | camelCase | `deltaTime`, `isEnabled` | 方法參數 |

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

3. **資料交互與繪製元件 (Drawer)**
   - **格式**：`{name}Drawer`
   - **說明**：負責資料展示與輸入交互的元件，通常繼承自 `RuntimeDrawer` 或 `RuntimeDrawer<T>`，支援 Unity 風格的縮排與標籤對齊。
   - **範例**：`IntDrawer`, `Vector3Drawer`, `TextDrawer`, `FoldoutDrawer`

4. **視窗層級元件 (Window)**
   - **格式**：`{name}Window`
   - **說明**：凡是繼承自基底類別 `Window`，代表一個獨立浮動的 OS/ImGui 級別視窗的類別。
   - **範例**：`ToolWindow`, `DebugWindow`
