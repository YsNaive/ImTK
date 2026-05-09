# 更新日誌 (Changelog)

本專案的所有重要變更都將記錄於此文件中。

格式基於 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.1.0/)，
且本專案遵循 [語意化版本控制 (Semantic Versioning)](https://semver.org/lang/zh-TW/).

## [Unreleased]
### Added (新增)
- 實作了核心的雙層生命週期架構 (`ImTKApplication`, `ImTKModule`, `ImTKObject`)。
- 引入了嚴格的 `ApplicationState` 狀態機，以防止主迴圈邏輯重入或順序錯誤。
- 新增 `Time` 靜態工具類別，自動管理全域的真實與縮放延遲時間 (Delta Time)。
- 新增 `ImTKSilk.Run()` 驅動程式，用於初始化 Silk.NET 並將 ImGui 控制器與 ImTK 生命週期完美整合。
- 透過 `ImTKSilkConstant` 啟用 ImGui Viewports，原生支援多視窗佈局。
- 於 `docs/` 目錄下建立基礎文檔結構，依子系統劃分 (`Core`, `UI`, `Database`, `Log`, `Event`, `Project`)。
- 新增嚴格的 `NamingConventions.md` 命名規範，確立大小寫、前綴以及元件後綴的規則。

### Changed (變更)
- 將原有的 `Architecture/` 目錄下的架構設計文件重組並移至對應的子系統資料夾。
- 將 `AGENT.md` 移至專案根目錄，並擴充了 AI 代理的條列式探勘與開發指引。
- 修改 `DevelopmentWrapUp.md`，制定了合併程式碼前必須遵守的五步驟最終檢查清單 (SOP)。
