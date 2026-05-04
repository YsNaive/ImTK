# ImTK (Immediate Toolkit)

ImTK 是一個基於 C# / .NET 的 Retained Mode UI 框架，底層封裝了強大且高效的 ImGui (Immediate Mode GUI)。
本框架的設計初衷在於提供開發者一套具有物件導向設計、安全生命週期管理、且支援 Unity-like 編輯器開發體驗的工具集，以加速小型應用工具的開發。

## 專案結構 (Project Structure)

* **`ImTK/`**: 框架核心庫，包含架構介面、生命週期管理與底層 UI 元素，此庫為**無圖形渲染依賴 (Graphics-Agnostic)**。
* **`ImTK.Silk/`**: ImTK 針對 `Silk.NET` 與 OpenGL 的預設實作橋接層，包含程式主迴圈啟動器 (`ImTKSilk`)。
* **`ImTK.Sample/`**: 範例專案，展示如何使用 ImTK 開發各種視窗與元件。
* **`ImTK.Test/`**: 框架的內部單元與整合測試。

## 尋找開發文檔 (Documentation)

所有的技術文檔、架構設計藍圖與開發規範皆收納於 `ImTK/docs/` 目錄中。
文檔已根據功能子系統 (Subsystem) 進行模組化分類。

請前往 [`ImTK/docs/README.md`](ImTK/docs/README.md) 查閱完整的子系統文檔索引。

## 給開發者與 AI 代理的注意事項

如果你是準備參與開發本專案的工程師或 AI 代理，請**務必**優先閱讀根目錄下的 [`AGENT.md`](AGENT.md) 了解最高指導原則，並查看 [`ImTK/docs/Project/TODO.md`](ImTK/docs/Project/TODO.md) 掌握當前開發進度。
