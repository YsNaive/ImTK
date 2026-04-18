# 開發收尾最終防線 (Development Wrap-Up)

任何一個 Task 或功能模組在宣告完成並準備 Merge (合併) 之前，開發者或 AI Agent 必須確認以下所有檢查項目皆已達成。這份清單確保了程式碼品質、文檔同步以及專案歷史的完整性。

---

## ✅ 維度一：程式碼品質與清理 (Code Cleanliness)

* [ ] **無遺留程式碼**：確保清除了所有迭代開發過程中遺留的廢棄註解、未使用的變數、以及過期的除錯程式碼 (如 `printf` 或 `std::cout`，需換成正規 `gcvex::Debug::log`)。
* [ ] **完整註解**：確保所有新增的 Class、Method、複雜邏輯，都具備完整且清晰的 Doxygen / XML 註解。
* [ ] **命名與架構**：檢查是否嚴格遵守 `NamingConventions.md` (如 `_ms` 單位、`m_` 前綴) 及非阻塞架構原則 (嚴禁在 `loop` 中惡意阻塞主線程)。

## ✅ 維度二：文檔同步 (Documentation Consistency)

* [ ] **新建/更新模組規格**：更新/建立該模組目錄下的 `README.md`，並確保 `docs/README.md` 中的全局索引連結正確。
* [ ] **歷史軌跡留存**：如果是針對舊邏輯的重構，將「為什麼這樣改」及「舊版痛點」記錄至該模組的 `CHANGELOG.md` 中。
* [ ] **特殊決策紀錄**：若有為了效能、物理限制而作的工程妥協，是否已寫入該模組的 `DESIGN_NOTES.md`？

## ✅ 維度三：專案追蹤與 AI 協作 (Tracking & Agent Workflow)

* [ ] **使用者確認 (AI Agent 必備)**：是否已透過 `request_user_input` 和使用者確認本次功能開發確實完成，可以進入收尾階段？
* [ ] **TODO 狀態更新**：是否已將 `docs/Project/TODO.md` 中的相關任務正確標記並移至 Completed (✅ 近期完成) 區塊？
