using System.IO;

string file = "CHANGELOG.md";
string text = File.ReadAllText(file);

string newEntry = @"## [0.1.0-alpha] - Unreleased

### Added
- `ImTK.UI.Button` 元件，提供基本的按鈕渲染能力與建構子語法糖 (`text`, `onClicked`)。
- `ImTK.UI.ClickEvent` 延遲事件，允許按鈕點擊事件透過 `EventDispatcher` 推遲至安全的 `LogicUpdate` 階段執行。
- `ImTK/docs/UI/Element/` 文檔目錄，用於收納未來新增的各類 UI 元件規格。

### Fixed
- 修正 `TestRunnerModule` 內的 `TestReportWindow` 統計狀態異常問題（修正 Pending 顯示判斷與顏色標示）。
- 修正 `EventBubbleTest` 測試失敗問題：將原有的 `ImGui.Button` 替換為 `ImTK.UI.Button` 以解決在 `GuiRender` 階段因直接修改視覺樹而觸發的生命週期保護機制 (CheckSafeState)。

";

text = text.Replace("## [0.1.0-alpha] - Unreleased", newEntry);
File.WriteAllText(file, text);
