# ImTK Sample & Overview

`ImTK.Sample` 不僅僅是一個範例程式，它是用來展示 ImTK 各項功能並做為開發者「實作參考書」的綜合應用。

## 範例架構規範

為了讓總覽面板 (`OverviewWindow`) 能夠自動抓取並顯示所有範例，開發者新增展示單元時，必須遵守以下規範：

### 1. 實作 ISampleScenario 介面

每一個展示單元（Scenario）都必須實作 `ISampleScenario` 介面。這能讓系統自動發現並註冊該範例。

```csharp
using ImTK.Sample.Framework;
using ImTK.UI;

namespace ImTK.Sample.Scenarios.MyFeature
{
    public class MyFeatureScenario : ISampleScenario
    {
        public string ScenarioName => "My Awesome Feature";
        public string Description => "Demonstrates how to use MyFeature in a Window.";

        // 指向附帶的 Markdown 說明文件 (相對路徑)
        public string DocumentationPath => "Scenarios/MyFeature/README.md";

        public void Open()
        {
            // 當使用者在面板上點擊「Open Demo」時執行的動作
            Window.Open<MyFeatureDemoWindow>();
        }
    }

    public class MyFeatureDemoWindow : Window
    {
        public MyFeatureDemoWindow() : base("Demo Window") { }
        protected override void OnRenderSelf() { ... }
    }
}
```

### 2. 資料夾與文檔附帶規定

每個範例應該被封裝在 `ImTK.Sample/Scenarios/` 下獨立的資料夾中（如 `Scenarios/CustomElement/`）。
該資料夾內**必須**附帶一份 `README.md` (或以功能命名的 markdown)，用來詳細解說此範例的設計思路與使用方式。

`ISampleScenario` 的 `DocumentationPath` 屬性必須指向這份 Markdown 文件的相對路徑，讓 UI 可以在使用者點擊「View Source Doc」時提供路徑指引。

## 執行方式

你可以透過 `dotnet run --project ImTK.Sample` 直接啟動。
啟動後會出現 "ImTK Sample Overview" 視窗，裡面列出了所有實作了 `ISampleScenario` 的功能模組，你可以點擊按鈕來開啟各別的 Demo 視窗進行測試。
