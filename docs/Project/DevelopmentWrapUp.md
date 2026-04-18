# 合併前最終檢查清單 (Development Wrap-Up SOP)

在提交程式碼前，請確認以下事項已完成：

1. **編譯無誤**：確保 `dotnet build ImTK`, `dotnet build ImTK.Silk`, `dotnet build ImTK.Test` 皆無錯誤或警告。
2. **命名規範檢查**：確保新增的類別有正確的後綴 (`View`, `Window`, `Field` 等)，私有變數使用 `m_` 或 `s_` 前綴。
3. **文檔同步更新**：如果有修改或新增功能，是否已同步更新對應子模組的 `README.md`？
4. **指標記憶體安全**：若有新增與 ImGui 的字串指標操作，是否使用了 `Marshal.StringToCoTaskMemUTF8` 且確保了正確的釋放生命週期？
5. **UI 元件屬性封裝**：影響功能狀態的變數是否已實作為 Property 而非 Field？
