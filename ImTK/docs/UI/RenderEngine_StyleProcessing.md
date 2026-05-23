# RenderEngine: Style Processing Architecture

This document describes the completely redesigned ImTK Style System architecture, focusing on the pipeline and separation of concerns between data, components, state management, and the render engine.

## 1. Core Data Layer (`StyleProperty`)
The foundation of the system is the `StyleProperty` struct, an explicit 16-byte structure designed to prevent GC allocations.
- **StyleCategory**: Defines the current state of the token (`HighLevelToken`, `ThemeToken`, `ImGuiStyle`).
- **StyleDataType**: Defines the payload (`Null`, `Float`, `Vector2`, `Color`, `HashedString`, `Int`, `Enum`).
- **StyleFlags**: Handles control flags like `Inheritable`.

## 2. Component & Composition Layer (`VisualElement.Style`)
Components declare their required styling through multiple mechanisms:
- `element.theme`: Specific theme override.
- `element.styleSheet` / `element.classList`: CSS-like class matching.
- `element.style` (Inline Styles): The highest priority container of `HighLevelToken` properties.
Components like `Button` can override `ComputeHighlevelToken` to translate concepts like "background-color" into "ButtonBg" ImGui styles.

## 3. Resolution & State Management (`ImGuiStyleHandler`)
The system caches resolved styles and differences in `ImGuiStyleHandler`.
- Features O(1) performance using internal fixed arrays corresponding to ImGui Enums.
- Manages `Push` and `Pop` of properties, including specialized lifecycle management for Font sizes and families via `RenderingContext`.
- Provides `Diff` calculations to compute minimal necessary ImGui state changes.

## 4. Pipeline Execution (`RenderEngine`)
Before rendering, if `element.m_isStyleDirty` is true, the `ComputeStyleRecursive` algorithm executes:
1. **Compose**: Dynamically merge Theme, StyleSheet, and Inline styles into a single list based on priority.
2. **Inherit**: Copy `Inheritable` properties from the parent's `resolvedStyle`.
3. **Translate**: Evaluate High-level tokens via Component overriders, fallback to `ImTKTheme` dictionary lookups for Theme tokens, and yield pure `ImGuiStyle` elements into `resolvedStyle`.
4. **Diff**: Compare the `resolvedStyle` against the parent's `resolvedStyle` and output the exact commands needed into `requiredStyle`.

During the render pass, `RenderEngine` merely calls `node.requiredStyle.Push()` before drawing and `Pop()` afterward.
