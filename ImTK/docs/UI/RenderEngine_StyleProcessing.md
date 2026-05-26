# RenderEngine: Style Processing Architecture

This document describes the ImTK Style System architecture, focusing on the pipeline and separation of concerns between data, components, state management, and the render engine.

## 1. Core Data Layer (`StyleProperty`)
The foundation of the system is the `StyleProperty` struct, an explicit 16-byte structure designed to prevent GC allocations.
- **StyleCategory**: Defines the current state of the token (`HighLevelToken`, `ThemeToken`, `ImGuiStyle`).
- **StyleDataType**: Defines the payload (`Null`, `Float`, `Vector2`, `Color`, `HashedString`, `Int`, `Enum`). (Note: `Thickness` has been removed in favor of 4 distinct float properties).
- **StyleFlags**: Handles control flags like `Inheritable`.

## 2. Component & Composition Layer (`VisualElement.Style`)
Components declare their required styling through multiple mechanisms:
- `element.theme`: 局部 Theme 覆蓋，實現完整的 ImGui 樣式隔離（見第 4 節）。
- `element.localStyleSheet` / `element.classList`: CSS-like class matching。Token 層級的局部覆蓋應使用此機制，而非 `element.theme`。
- `element.style` (Inline Styles): The highest priority container of `HighLevelToken` properties.
Components like `Button` can override `ComputeHighlevelToken` to translate concepts like "background-color" into "ButtonBg" ImGui styles.

## 3. Resolution & State Management (`ImGuiStyleHandler`)
The system caches resolved styles and differences in `ImGuiStyleHandler`.
- Features O(1) performance using internal fixed arrays corresponding to ImGui Enums.
- Manages `Push` and `Pop` of properties, including specialized lifecycle management for Font sizes and families via `RenderingContext`.
- Provides `Diff` calculations to compute minimal necessary ImGui state changes.

## 4. Pipeline Execution (`RenderEngine`)

### 4.1 Style Computation (`ComputeStyleRecursive`)

Before rendering, if `element.m_isStyleDirty` is true, the `ComputeStyleRecursiveInternal` algorithm executes the following steps in order:

1. **Inherit**: Copy `Inheritable` properties from the parent's `resolvedStyle` via `CopyFrom()`.
2. **Theme Inject** *(新增)*: If `element.m_theme != null`，呼叫 `element.m_theme.InjectToStyleHandler(element.resolvedStyle)`，將該 Theme 的完整 ImGui 樣式（顏色、StyleVar、字型）以 `isInheritable = true` 的 `StyleProperty` 注入 `resolvedStyle`。此步驟**覆蓋** Inherit 所得的樣式，實現局部樣式隔離。子元素透過 `CopyFrom()` 自動繼承，無需重複注入。
3. **Compose**: Merge `StyleSheet.Global`, ancestor's `localStyleSheet`, and inline styles into a composed list. Higher-priority styles (inline > local sheet > global sheet) override lower-priority ones by key.
4. **Translate**: Evaluate High-level tokens via Component overriders; resolve Theme tokens via `element.theme` dictionary lookups; yield pure `ImGuiStyle` elements into `resolvedStyle`. StyleSheet properties applied in this step can **further override** the theme baseline from step 2.
5. **Diff**: Compare the `resolvedStyle` against the parent's `resolvedStyle` and output the exact push commands needed into `requiredStyle`.

### 4.2 Render Pass

During the render pass, `RenderEngine.RenderNode()` calls `node.requiredStyle.Push()` before drawing and `Pop()` afterward. This mechanism transparently handles theme isolation: when an element with a local theme is entered, all differing ImGui styles are pushed; when it exits, they are popped.

### 4.3 Static Buffer Safety

`ComputeStyleRecursiveInternal` uses two static buffers (`s_composedProps`, `s_translatedProps`) to avoid per-element List allocations. The method is **not re-entrant**: calling `MarkStyleDirty()` or triggering style recomputation from within `ComputeHighlevelToken` or theme token resolution is forbidden. In Debug builds, a `s_isComputing` guard enforces this contract and throws `InvalidOperationException` on violation.

### 4.4 Global Theme Baseline

`ImTKTheme.GlobalTheme.ApplyToImGui()` sets the global ImGui style baseline once per frame when `isGlobalThemeDirty` is true. This baseline applies to all elements that do **not** have a local `m_theme` set. Elements with a local theme push overrides on top of this baseline via the `requiredStyle` mechanism.

### 4.5 Layout-Aware Padding Translation

In the final step of the Style Pipeline, the system automatically intercepts Padding properties. If any Padding (Left/Top/Right/Bottom) is modified via styles or layout constraints, the pipeline uses the precisely calculated `resolvedLayoutState.padding` to synthesize `Vector2` values. These are directly injected back into `ImGuiStyleVar.WindowPadding` and `FramePadding` on the `resolvedStyle`, ensuring that the bottom-level ImGui rendering borders perfectly align with the Layout Engine's bounding boxes, all while seamlessly falling back to the theme's default padding if unmodified.
