---
description: 開發新的自定義 VisualElement
---

### 🛠️ 實作 `VisualElement` 的標準 SOP

#### 1. 決定元件類型與繼承策略
*   **一般元件**：直接繼承 `VisualElement`。
*   **自訂樣式元件**：如果該元件有自己專屬的 CSS 屬性（如 ScrollView 的 Scrollbar 顏色），需要先定義一個繼承自 `IVisualElementStyle` 的子類別，然後使用泛型基底 `VisualElement<TStyle>`。
*   **容器轉發 (Shadow Tree)**：如果你的元件是一個複合容器（例如 `ScrollView` 內部有一個實際負責裝載內容的 `contentContainer`），則必須覆寫 `public virtual VisualElement contentContainer => ...;`，讓外部呼叫 `Add()` 時能正確將子節點轉發到內部的物理層。

#### 2. 封裝 ImGui Flags (若有需要)
根據 **開發規範 1.5** (ImGui Flags 封裝)，如果元件依賴 ImGui 的 Flag（例如 `ImGuiChildFlags`、`ImGuiWindowFlags`）：
*   不要直接暴露底層的位元遮罩給外部。
*   應建立一個繼承自 `ElementFlags<TEnum>` 的內部類別。
*   將特定功能封裝為直覺的布林屬性 (Property)，並在內部呼叫 `GetFlag/SetFlag` 進行位元運算。

#### 3. 實作三層渲染生命週期 (Template Methods)
這是最核心的步驟，必須嚴格遵守職責分離：
*   **`OnBeginRender()`**：
    *   **用途**：宣告 ImGui 的作用域（如呼叫 `ImGui.BeginChild`, `ImGui.BeginGroup`）。
    *   **回傳值**：必須回傳 `bool`。回傳 `true` 框架才會繼續走訪並渲染其內部子元件；回傳 `false` 則會跳過子元件渲染（可用於優化被遮擋或折疊的元件）。
    *   **注意**：預設的 `base.OnBeginRender()` 會自動呼叫 `ImGui.SetCursorScreenPos(layoutRect.position)`，將游標對齊到 Flexbox 計算好的絕對位置。
*   **`OnRender()`**：
    *   **用途**：呼叫純視覺的 ImGui API 繪製本體內容（如 `ImGui.Button`, `ImGui.Text`）。
    *   **尺寸與位置**：此時 ImGui 游標已經在正確的起點。如果 ImGui API 需要傳入 Size，請直接傳遞排版引擎給予的 **`this.layoutRect.size`**。
*   **`OnEndRender()`**：
    *   **用途**：與 `OnBeginRender` 成對，用於關閉作用域（如呼叫 `ImGui.EndChild`, `ImGui.EndGroup`）。

#### 4. 處理互動與狀態變更
*   **事件推派**：如果元件產生了特定的互動（例如被點擊、數值改變），不要直接執行外部邏輯，而是建立繼承自 `UIEventBase` 的事件，並透過 `SendEvent(evt)` 送入事件佇列，達到解耦。
*   **結構修改防護**：如果在互動中需要新增或刪除子節點，**絕對不可以在 Render 階段直接呼叫 `Add/Remove`**，必須包裝在 `ScheduleDeferred(() => { ... })` 中，讓框架在下一幀安全執行。

---

### ✅ 實作完成後的檢查清單 (Checklist)

在完成任何一個 VisualElement 之後，必須進行以下程式碼審查：

#### A. 排版與佈局合規性 (Layout Engine Compliance)
- [ ] **無 ImGui 排版 API**：`OnRender` 內部**絕對沒有**呼叫 `ImGui.SameLine()`、`ImGui.Indent()`、`ImGui.NewLine()` 或 `ImGui.Spacing()`。這些計算應全權交由 `resolvedLayoutState` (Flexbox 屬性) 來控制，除非是有意設計的特化元件。
- [ ] **邏輯像素獨立**：程式碼中**沒有**手動將尺寸乘以 `DpiScale`。框架會在管線末端自動根據 DPI 縮放 `layoutRect`，開發者只需處理純邏輯像素。

#### B. 記憶體與指標安全 (Memory & Pointer Safety) - *規範 1.4*
- [ ] **無 `fixed` 字串指標**：如果呼叫了 ImGui 需要傳入 `byte*` (字串指標) 的底層 API，**絕對沒有**使用 `fixed` 語句。
- [ ] **指標生命週期**：如果有分配 Unmanaged 記憶體（如 `Marshal.StringToCoTaskMemUTF8`），是否有在適當的生命週期確保呼叫 `Marshal.FreeCoTaskMem` 防止 Memory Leak。

#### C. 架構封裝 (Architecture & Encapsulation) - *規範 1.5*
- [ ] **屬性優先 (Property over Field)**：所有影響元件狀態或外觀的變數（如 `isEnabled`, `text`），是否都實作為 C# Property，而非 `public` field。
- [ ] **自動 ID 防護**：除非是繪製內部的虛擬清單（如非 VisualElement 的純資料迴圈），否則沒有手動呼叫 `ImGui.PushID`。框架已經自動利用 `m_elementId` 進行防撞處理。
- [ ] **樣式讀取正確性**：手動繪製顏色或尺寸時，是否優先從 `resolvedStyle` 讀取快取值，而非寫死顏色。

---