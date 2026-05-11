# 合併前最終檢查清單 (Development Wrap-Up SOP)

在提交任何程式碼或發起 Pull Request 前，請**必定**確認以下事項已完成：

1. **編譯無誤 (Build Success)**
   - 確保執行 `dotnet build ImTK`, `dotnet build ImTK.Silk`, `dotnet build ImTK.Test` 皆無任何錯誤 (Errors) 或警告 (Warnings)。
2. **代碼清理 (Clean Up)**
   - 移除所有除錯用的 `Console.WriteLine` (非正式日誌)、未使用的變數、被註解掉的測試用死代碼，以及不必要的空檔案。
3. **命名規範檢查 (Naming Conventions)**
   - 確保所有新增或修改的類別、屬性、變量與 UI 元件皆符合 [命名規範](./NamingConventions.md)。
4. **日誌完整性檢查 (Log Completeness)**
   - 確認核心邏輯、模組生命週期、全域管理系統等皆已加入適當的日誌追蹤 (`s_log.Trace`/`Debug`)。
   - 確認高頻率呼叫的底層機制避免加入過量日誌以免影響效能。
   - 確認例外與錯誤邊界皆有 `Error` 或 `Warning` 日誌紀錄。
5. **文檔同步更新 (Documentation Sync)**
   - 如果有修改現有架構或新增功能，確認已同步更新對應子模組（如 `Core/`, `UI/` 等）的 `README.md` 或 Markdown 技術文檔。
6. **更新 Changelog (Update Changelog)**
   - 確保本次提交的新功能、修復或架構變更，已經如實且精簡地記錄到根目錄 `CHANGELOG.md` 的 `[Unreleased]` 區塊中。
