# UI 視覺元件庫 (UI Elements)

本目錄存放 ImTK 框架內建的標準 UI 元件說明與規格設計。
所有的元件均繼承自 `VisualElement`，並透過整合 `ImGui` 的底層渲染邏輯與 `ImTK` 的 UI 事件系統 (`EventDispatcher`)，提供安全、易用且支援延遲派發 (Deferred Dispatching) 的高階控制項。

## 目錄

* [**Button (按鈕)**](Button.md)

## 設計規範 (Design Guidelines)

當開發者為 ImTK 新增自定義的元件時，應遵守以下原則：
1. **獨立渲染層：** 元件的渲染邏輯必須實作於覆寫的 `OnRenderSelf()` 中。
2. **事件轉換：** 絕不直接在 `GuiRender` 階段同步執行回呼委派 (Callback) 或修改 UI 樹。所有的使用者互動（如點擊、拖曳），皆應呼叫 `SendEvent(EventPool<T>.Get())` 將其轉換為事件。
3. **語法糖：** 為了方便宣告式 (Declarative-style) 的 UI 構建，建議在建構函式中提供屬性預設值，並提供諸如 `public event Action<TEvent> onXXX` 的包裝屬性，以自動轉發至 `RegisterCallback` 與 `UnregisterCallback`。
