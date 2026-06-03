# 合併前最終檢查清單 (Development Wrap-Up SOP)

在結束本次功能開發前，請**必定**確認以下事項已完成：

1. **代碼清理 (Clean Up)**
   - 移除所有除錯用的 `Console.WriteLine` (非正式日誌)、未使用的變數、被註解掉的測試用死代碼，以及不必要的空檔案。
2. **命名規範檢查 (Naming Conventions)**
   - 確保所有新增或修改的類別、屬性、變量與 UI 元件皆符合 [命名規範](./NamingConventions.md)。
3. **日誌完整性檢查 (Log Completeness)**
   - 確認核心邏輯、模組生命週期、全域管理系統等皆已加入適當的日誌追蹤 (`ImTKLog.Trace`/`Debug`)。
   - 確認高頻率呼叫的底層機制避免加入過量日誌以免影響效能。
   - 確認例外與錯誤邊界皆有 `Error` 或 `Warning` 日誌紀錄。
5. **維護檢查 ( Maintenance)**
   - 檢查本次更動範圍，若牽涉到既存的 Test 與 Sample，需同步檢查相關內容是否需要同步更新。
6. **文檔同步更新 (Documentation Sync)**
   - 如果有修改現有架構或新增功能，確認已按照[專案文檔規範](./DocumentationSpecifications.md)同步更新或新增對應子模組（如 `Core/`, `UI/` 等）的 `README.md` 或 Markdown 技術文檔。
7. **更新 Changelog (Update Changelog)**
   - 確保本次提交的新功能、修復或架構變更，已經如實且精簡地記錄到根目錄 `CHANGELOG.md` 的 `[Unreleased]` 區塊中。
