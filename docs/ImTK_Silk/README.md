# ImTK_Silk 整合橋接模組

本模組負責將獨立的 ImTK 核心 UI 邏輯，掛載到 Silk.NET (OpenGL) 視窗系統與 ImGui.NET 環境之上。

## 嚴格的 .NET 平台生命週期假設

ImTK_Silk 被設計為在標準的 .NET Core / .NET 5+ 平台上運行。它假設了**單一的應用程式生命週期**。
程式進入點通常是建構一個 `ImTKSilkConstant` 設定檔，傳入 `ImTKSilk.Initialize(constant)`，然後呼叫 `ImTKSilk.Start()` 進入阻塞主迴圈。

## `ImTKSilkConstant` 集中配置

所有的視窗環境參數（例如視窗大小、標題、字型路徑、全螢幕設置）以及內部使用的 `configFolderPath` (儲存 `imgui.ini` 與 `window_state.json` 的路徑)，都透過這個類別集中管理。這避免了配置散落在各個系統模組中。

## 關鍵技術：C# 與 ImGui 的非受管記憶體指標交互

ImGui 是一個 C++ 函式庫，它要求某些持久的狀態（如 `.ini` 檔案的存放路徑 `io.IniFilename`）提供一個指向字串的 C 語言指標 (`byte*` 或 `char*`)。

### 開發者必須知道的陷阱與解法
在 C# 中，字串記憶體是由 Garbage Collector (GC) 管理的。如果直接使用 `fixed` 或是 `Encoding.UTF8.GetBytes()` 來獲取指標並交給 ImGui，**當 GC 執行回收或移動記憶體時，ImGui 內部會產生懸空指標 (Dangling Pointer) 導致應用程式崩潰。**

**本模組的標準解決方案**：
1. **分配**：使用 `Marshal.StringToCoTaskMemUTF8`，它會在非受管記憶體 (Unmanaged Memory) 區塊中分配字串，這塊記憶體**完全免疫於 GC 的影響**。
2. **生命週期管理**：我們將這個 `IntPtr` (如 `s_iniFilenamePtr`) 記錄在靜態欄位中。
3. **釋放**：在 `OnClose()` 階段，確保呼叫 `Marshal.FreeCoTaskMem()` 來釋放這塊記憶體，避免 Memory Leak。
*(註：這項技術也已明文列入 `AGENT.md` 中，作為後續維護的最高原則。)*

## Dockspace 算圖架構

在 `ImTKSilk.OnRender` 中，它自動接管了 `ImGui.GetMainViewport()`，並強制推入一個充滿整個視窗、沒有標題欄與邊框的隱形 `main-dock-space`。
緊接著它呼叫 `ImTKModule.RenderAll()`。這代表開發者撰寫的任何 `Window` 預設都會渲染在這個支持 Docking 的強大根容器之內。
