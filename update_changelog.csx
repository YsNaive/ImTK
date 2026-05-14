using System.IO;

var lines = File.ReadAllLines("CHANGELOG.md");
var newLines = new System.Collections.Generic.List<string>();

foreach(var line in lines)
{
    newLines.Add(line);
    if(line == "### Added (新增)")
    {
        newLines.Add("- 實作了 `FieldDrawer` 系統，支援動態綁定資料型別至 UI 元件 (類似 Unity PropertyDrawer)。");
        newLines.Add("- 實作了雙向資料流與安全攔截機制 (`SetValueWithoutNotify`, `SetValueWithChanged`)，避免 DataBinding 循環。");
        newLines.Add("- 新增了 `ValueChangedEvent<T>` (繼承自輕量級 `IValueChangedEvent`)，支援 `isInternalChange` 判斷，且預設不冒泡。");
        newLines.Add("- 實作了 `FieldDrawerRegistry`，支援基於 `[CustomFieldDrawer]` 的型別映射、繼承遞迴回退與 `requiredModifier` 修飾器過濾。");
        newLines.Add("- 實作了 `FieldDrawerFactory` 提供 Fluent API 動態建置 Drawer 與注入修飾器屬性。");
        newLines.Add("- 實作了 `ObjectDrawer`，利用反射與 `IValueChangedEvent` 將 UI 的複雜嵌套修改雙向寫回原始物件。");
        newLines.Add("- 在 `ImTKTheme` 引入 `DrawerLabelWidth`，並在 `FieldDrawer` 中實作 `Inline` / `Expand` 自動對齊排版。");
    }
    if(line == "### Fixed (修復)")
    {
        newLines.Add("- 修復了無頭測試中全域 `EventDispatcher` 駐列污染的架構漏洞，實作 `ClearQueue` 強制在每次測試前後隔離環境。");
    }
}
File.WriteAllLines("CHANGELOG.md", newLines);
