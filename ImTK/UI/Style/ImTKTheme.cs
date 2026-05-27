using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public enum ThemeColorFamily
    {
        Normal,
        Success,
        Info,
        Warning,
        Danger
    }

    public class ImTKTheme
    {
        public ImTKTheme parent { get; set; }

        private Dictionary<int, Color> m_colorTokens = new Dictionary<int, Color>();
        private Dictionary<int, float> m_floatTokens = new Dictionary<int, float>();
        private Dictionary<int, Vector2> m_vector2Tokens = new Dictionary<int, Vector2>();
        private Dictionary<int, int> m_hashTokens = new Dictionary<int, int>();

        public void SetColorToken(HashedString token, Color color) => m_colorTokens[token.Hash] = color;
        public void SetFloatToken(HashedString token, float value) => m_floatTokens[token.Hash] = value;
        public void SetVector2Token(HashedString token, Vector2 value) => m_vector2Tokens[token.Hash] = value;
        public void SetHashToken(HashedString token, int value) => m_hashTokens[token.Hash] = value;

        public bool TryGetColorToken(int tokenHash, out Color color)
        {
            if (m_colorTokens.TryGetValue(tokenHash, out color)) return true;
            if (parent != null) return parent.TryGetColorToken(tokenHash, out color);
            color = default;
            return false;
        }

        public bool TryGetFloatToken(int tokenHash, out float value)
        {
            if (m_floatTokens.TryGetValue(tokenHash, out value)) return true;
            if (parent != null) return parent.TryGetFloatToken(tokenHash, out value);
            value = default;
            return false;
        }

        public bool TryGetVector2Token(int tokenHash, out Vector2 value)
        {
            if (m_vector2Tokens.TryGetValue(tokenHash, out value)) return true;
            if (parent != null) return parent.TryGetVector2Token(tokenHash, out value);
            value = default;
            return false;
        }

        public bool TryGetHashToken(int tokenHash, out int value)
        {
            if (m_hashTokens.TryGetValue(tokenHash, out value)) return true;
            if (parent != null) return parent.TryGetHashToken(tokenHash, out value);
            value = default;
            return false;
        }

        internal Color GetColor(HashedString token, Color fallback = default) => TryGetColorToken(token.Hash, out Color c) ? c : fallback;
        internal void SetColor(HashedString token, Color value) => SetColorToken(token, value);

        internal float GetFloat(HashedString token, float fallback = 0f) => TryGetFloatToken(token.Hash, out float f) ? f : fallback;
        internal void SetFloat(HashedString token, float value) => SetFloatToken(token, value);

        internal Vector2 GetVector2(HashedString token, Vector2 fallback = default) => TryGetVector2Token(token.Hash, out Vector2 v) ? v : fallback;
        internal void SetVector2(HashedString token, Vector2 value) => SetVector2Token(token, value);

        internal int GetHash(HashedString token, int fallback = 0) => TryGetHashToken(token.Hash, out int h) ? h : fallback;
        internal void SetHash(HashedString token, int value) => SetHashToken(token, value);

        // --- Cached Token Keys ---
        public static class Tokens
        {
            public static readonly HashedString LabelWidth = new HashedString("--label-width");
            public static readonly HashedString EdgePadding = new HashedString("--edge-padding");
            public static readonly HashedString IndentWidth = new HashedString("--indent-width");

            public static readonly HashedString Padding = new HashedString("--padding");
            public static readonly HashedString ItemSpacing = new HashedString("--item-spacing");
            public static readonly HashedString ItemInnerSpacing = new HashedString("--item-inner-spacing");
            public static readonly HashedString BorderWidth = new HashedString("--border-width");
            public static readonly HashedString BorderRadius = new HashedString("--border-radius");
            public static readonly HashedString DisabledAlpha = new HashedString("--disabled-alpha");
            public static readonly HashedString FontFamily = new HashedString("--font-family");

            public static class Syntax
            {
                public static readonly HashedString FuncColor = new HashedString("--syntax-func-color");
                public static readonly HashedString ArgsColor = new HashedString("--syntax-args-color");
                public static readonly HashedString TypeColor = new HashedString("--syntax-type-color");
                public static readonly HashedString ValueTypeColor = new HashedString("--syntax-value-type-color");
                public static readonly HashedString PrefixColor = new HashedString("--syntax-prefix-color");
                public static readonly HashedString StringColor = new HashedString("--syntax-string-color");
                public static readonly HashedString NumberColor = new HashedString("--syntax-number-color");
                public static readonly HashedString ControlColor = new HashedString("--syntax-control-color");
                public static readonly HashedString CommentsColor = new HashedString("--syntax-comments-color");
                public static readonly HashedString CodeBgColor = new HashedString("--syntax-code-bg-color");
                public static readonly HashedString CodeTextColor = new HashedString("--syntax-code-text-color");
            }
        }

        // --- Color Family Subsystem ---
        public class ColorFamily
        {
            private ImTKTheme _theme;
            private HashedString _surface, _container, _component, _componentHover, _componentActive, _accent, _accentHover, _accentActive, _selection, _border, _divider, _text, _subText, _disabledText;

            public ColorFamily(ImTKTheme theme, string prefix)
            {
                _theme = theme;
                _surface = new HashedString(prefix + "-surface");
                _container = new HashedString(prefix + "-container");
                _component = new HashedString(prefix + "-component");
                _componentHover = new HashedString(prefix + "-component-hover");
                _componentActive = new HashedString(prefix + "-component-active");
                _accent = new HashedString(prefix + "-accent");
                _accentHover = new HashedString(prefix + "-accent-hover");
                _accentActive = new HashedString(prefix + "-accent-active");
                _selection = new HashedString(prefix + "-selection");
                _border = new HashedString(prefix + "-border");
                _divider = new HashedString(prefix + "-divider");
                _text = new HashedString(prefix + "-text");
                _subText = new HashedString(prefix + "-sub-text");
                _disabledText = new HashedString(prefix + "-disabled-text");
            }

            public Color surface { get => _theme.GetColor(_surface); set => _theme.SetColor(_surface, value); }
            public Color container { get => _theme.GetColor(_container); set => _theme.SetColor(_container, value); }
            public Color component { get => _theme.GetColor(_component); set => _theme.SetColor(_component, value); }
            public Color componentHover { get => _theme.GetColor(_componentHover); set => _theme.SetColor(_componentHover, value); }
            public Color componentActive { get => _theme.GetColor(_componentActive); set => _theme.SetColor(_componentActive, value); }
            public Color accent { get => _theme.GetColor(_accent); set => _theme.SetColor(_accent, value); }
            public Color accentHover { get => _theme.GetColor(_accentHover); set => _theme.SetColor(_accentHover, value); }
            public Color accentActive { get => _theme.GetColor(_accentActive); set => _theme.SetColor(_accentActive, value); }
            public Color selection { get => _theme.GetColor(_selection); set => _theme.SetColor(_selection, value); }
            public Color border { get => _theme.GetColor(_border); set => _theme.SetColor(_border, value); }
            public Color divider { get => _theme.GetColor(_divider); set => _theme.SetColor(_divider, value); }
            public Color text { get => _theme.GetColor(_text); set => _theme.SetColor(_text, value); }
            public Color subText { get => _theme.GetColor(_subText); set => _theme.SetColor(_subText, value); }
            public Color disabledText { get => _theme.GetColor(_disabledText); set => _theme.SetColor(_disabledText, value); }
        }

        private ColorFamily m_normal, m_success, m_info, m_warning, m_danger;
        public ColorFamily normalColor => m_normal ??= new ColorFamily(this, "--normal");
        public ColorFamily successColor => m_success ??= new ColorFamily(this, "--success");
        public ColorFamily infoColor => m_info ??= new ColorFamily(this, "--info");
        public ColorFamily warningColor => m_warning ??= new ColorFamily(this, "--warning");
        public ColorFamily dangerColor => m_danger ??= new ColorFamily(this, "--danger");


        // --- Global Shared Properties ---
        public float labelWidth { get => GetFloat(Tokens.LabelWidth, 180f); set => SetFloat(Tokens.LabelWidth, value); }
        public float edgePadding { get => GetFloat(Tokens.EdgePadding, 10f); set => SetFloat(Tokens.EdgePadding, value); }
        public float indentWidth { get => GetFloat(Tokens.IndentWidth, 16f); set => SetFloat(Tokens.IndentWidth, value); }

        public Vector2 padding { get => GetVector2(Tokens.Padding, new Vector2(4, 3)); set => SetVector2(Tokens.Padding, value); }
        public Vector2 itemSpacing { get => GetVector2(Tokens.ItemSpacing, new Vector2(8, 4)); set => SetVector2(Tokens.ItemSpacing, value); }
        public Vector2 itemInnerSpacing { get => GetVector2(Tokens.ItemInnerSpacing, new Vector2(4, 4)); set => SetVector2(Tokens.ItemInnerSpacing, value); }
        public float borderWidth { get => GetFloat(Tokens.BorderWidth, 1f); set => SetFloat(Tokens.BorderWidth, value); }
        public float borderRadius { get => GetFloat(Tokens.BorderRadius, 3f); set => SetFloat(Tokens.BorderRadius, value); }
                public float disabledAlpha { get => GetFloat(Tokens.DisabledAlpha, 0.6f); set => SetFloat(Tokens.DisabledAlpha, value); }

        private float m_globalFontScale = 1.0f;
        public float globalFontScale
        {
            get => m_globalFontScale;
            set
            {
                if (m_globalFontScale != value)
                {
                    m_globalFontScale = value;
                    ImTKFontManager.MarkFontDirty();
                }
            }
        }
                private string m_fontFamily = "ImGuiDefault";
        public string fontFamily
        {
            get => m_fontFamily;
            set { m_fontFamily = value; SetHash(Tokens.FontFamily, new HashedString(value).Hash); ImTKFontManager.MarkFontDirty(); }
        }

        internal int fontFamilyHash => GetHash(Tokens.FontFamily, ImTKFontManager.DefaultFontFamilyHash);

        // --- Font System ---
        private Dictionary<FontSize, float> m_fontSizes = new Dictionary<FontSize, float>
        {
            { FontSize.Small, 14f },
            { FontSize.Normal, 18f },
            { FontSize.H3, 24f },
            { FontSize.H2, 32f },
            { FontSize.H1, 48f }
        };

        internal IReadOnlyDictionary<FontSize, float> GetFontSizes() => m_fontSizes;

        private void SetFontSizeInternal(FontSize sizeEnum, float pixelSize)
        {
            if (m_fontSizes[sizeEnum] != pixelSize)
            {
                m_fontSizes[sizeEnum] = pixelSize;
                ImTKFontManager.MarkFontDirty();
            }
        }

        public float fontSizeSmall { get => m_fontSizes[FontSize.Small]; set => SetFontSizeInternal(FontSize.Small, value); }
        public float fontSizeNormal { get => m_fontSizes[FontSize.Normal]; set => SetFontSizeInternal(FontSize.Normal, value); }
        public float fontSizeH3 { get => m_fontSizes[FontSize.H3]; set => SetFontSizeInternal(FontSize.H3, value); }
        public float fontSizeH2 { get => m_fontSizes[FontSize.H2]; set => SetFontSizeInternal(FontSize.H2, value); }
        public float fontSizeH1 { get => m_fontSizes[FontSize.H1]; set => SetFontSizeInternal(FontSize.H1, value); }

        // --- Syntax Highlight Wrapper ---
        public class SyntaxTheme
        {
            private ImTKTheme _theme;
            public SyntaxTheme(ImTKTheme theme) { _theme = theme; }

            public Color funcColor { get => _theme.GetColor(Tokens.Syntax.FuncColor); set => _theme.SetColor(Tokens.Syntax.FuncColor, value); }
            public Color argsColor { get => _theme.GetColor(Tokens.Syntax.ArgsColor); set => _theme.SetColor(Tokens.Syntax.ArgsColor, value); }
            public Color typeColor { get => _theme.GetColor(Tokens.Syntax.TypeColor); set => _theme.SetColor(Tokens.Syntax.TypeColor, value); }
            public Color valueTypeColor { get => _theme.GetColor(Tokens.Syntax.ValueTypeColor); set => _theme.SetColor(Tokens.Syntax.ValueTypeColor, value); }
            public Color prefixColor { get => _theme.GetColor(Tokens.Syntax.PrefixColor); set => _theme.SetColor(Tokens.Syntax.PrefixColor, value); }
            public Color stringColor { get => _theme.GetColor(Tokens.Syntax.StringColor); set => _theme.SetColor(Tokens.Syntax.StringColor, value); }
            public Color numberColor { get => _theme.GetColor(Tokens.Syntax.NumberColor); set => _theme.SetColor(Tokens.Syntax.NumberColor, value); }
            public Color controlColor { get => _theme.GetColor(Tokens.Syntax.ControlColor); set => _theme.SetColor(Tokens.Syntax.ControlColor, value); }
            public Color commentsColor { get => _theme.GetColor(Tokens.Syntax.CommentsColor); set => _theme.SetColor(Tokens.Syntax.CommentsColor, value); }
            public Color codeBackgroundColor { get => _theme.GetColor(Tokens.Syntax.CodeBgColor); set => _theme.SetColor(Tokens.Syntax.CodeBgColor, value); }
            public Color codeTextColor { get => _theme.GetColor(Tokens.Syntax.CodeTextColor); set => _theme.SetColor(Tokens.Syntax.CodeTextColor, value); }
        }

        private SyntaxTheme m_syntax;
        public SyntaxTheme syntax => m_syntax ??= new SyntaxTheme(this);

        /// <summary>
        /// 將 normalColor 的所有語義 Token 映射至對應的 ImGui Color，透過 <paramref name="sink"/> 輸出。
        /// BorderShadow 為硬編碼透明黑的特例，不納入此方法，由呼叫端自行處理。
        /// </summary>
        private void BuildColorMap(System.Action<ImGuiCol, Color> sink)
        {
            sink(ImGuiCol.Text,                  normalColor.text);
            sink(ImGuiCol.TextDisabled,          normalColor.disabledText);
            sink(ImGuiCol.WindowBg,              normalColor.surface);
            sink(ImGuiCol.ChildBg,               normalColor.surface);
            sink(ImGuiCol.PopupBg,               normalColor.container);
            sink(ImGuiCol.Border,                normalColor.border);
            sink(ImGuiCol.FrameBg,               normalColor.component);
            sink(ImGuiCol.FrameBgHovered,        normalColor.componentHover);
            sink(ImGuiCol.FrameBgActive,         normalColor.componentActive);
            sink(ImGuiCol.TitleBg,               normalColor.container);
            sink(ImGuiCol.TitleBgActive,         normalColor.container);
            sink(ImGuiCol.TitleBgCollapsed,      normalColor.container);
            sink(ImGuiCol.MenuBarBg,             normalColor.container);
            sink(ImGuiCol.ScrollbarBg,           normalColor.container);
            sink(ImGuiCol.ScrollbarGrab,         normalColor.accent);
            sink(ImGuiCol.ScrollbarGrabHovered,  normalColor.accentHover);
            sink(ImGuiCol.ScrollbarGrabActive,   normalColor.accentActive);
            sink(ImGuiCol.CheckMark,             normalColor.accent);
            sink(ImGuiCol.SliderGrab,            normalColor.accent);
            sink(ImGuiCol.SliderGrabActive,      normalColor.accentActive);
            sink(ImGuiCol.Button,                normalColor.component);
            sink(ImGuiCol.ButtonHovered,         normalColor.componentHover);
            sink(ImGuiCol.ButtonActive,          normalColor.componentActive);
            sink(ImGuiCol.Header,                normalColor.component);
            sink(ImGuiCol.HeaderHovered,         normalColor.componentHover);
            sink(ImGuiCol.HeaderActive,          normalColor.componentActive);
            sink(ImGuiCol.Separator,             normalColor.divider);
            sink(ImGuiCol.SeparatorHovered,      normalColor.divider);
            sink(ImGuiCol.SeparatorActive,       normalColor.divider);
            sink(ImGuiCol.ResizeGrip,            normalColor.component);
            sink(ImGuiCol.ResizeGripHovered,     normalColor.componentHover);
            sink(ImGuiCol.ResizeGripActive,      normalColor.componentActive);
            sink(ImGuiCol.Tab,                   normalColor.component);
            sink(ImGuiCol.TabHovered,            normalColor.componentHover);
            sink(ImGuiCol.TabSelected,           normalColor.selection);
            sink(ImGuiCol.TabDimmed,             normalColor.component);
            sink(ImGuiCol.TabDimmedSelected,     normalColor.selection);
            sink(ImGuiCol.DockingPreview,        normalColor.selection);
            sink(ImGuiCol.DockingEmptyBg,        normalColor.surface);
            sink(ImGuiCol.TextSelectedBg,        normalColor.selection);
        }

        public void ApplyToImGui()
        {
            unsafe
            {
                ImGuiStylePtr style = ImGui.GetStyle();

                // Colors
                BuildColorMap((col, color) => style.Colors[(int)col] = color.rgba);
                style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0); // Special: hardcoded transparent black, not a ColorFamily token

                // Dimensions / Layout
                style.WindowPadding = padding;
                style.FramePadding = padding;
                style.ItemSpacing = itemSpacing;
                style.ItemInnerSpacing = itemInnerSpacing;

                style.WindowRounding = borderRadius;
                style.ChildRounding = borderRadius;
                style.FrameRounding = borderRadius;
                style.PopupRounding = borderRadius;
                style.ScrollbarRounding = borderRadius;
                style.GrabRounding = borderRadius;
                style.TabRounding = borderRadius;

                style.WindowBorderSize = borderWidth;
                style.ChildBorderSize = borderWidth;
                style.PopupBorderSize = borderWidth;
                style.FrameBorderSize = borderWidth;
                style.TabBorderSize = borderWidth;

                style.DisabledAlpha = disabledAlpha;
            }
        }

        /// <summary>
        /// 將此 Theme 的完整 ImGui 樣式注入到指定的 ImGuiStyleHandler，
        /// 供 ComputeStyleRecursive 在元素有局部 theme 時呼叫，實現局部樣式隔離。
        /// <para>
        /// 注入的屬性全部標記為 isInheritable = true，使子元素能透過
        /// resolvedStyle.CopyFrom() 自動繼承 theme 樣式，無需對每個子元素重複注入。
        /// Diff() 會自動計算出需要 Push/Pop 的差異，由 RenderNode 的既有機制處理。
        /// </para>
        /// <para>
        /// 注入順序與 ApplyToImGui() 保持一致，以確保語義一致性。
        /// 注意：ImGuiStyleVar.TabBorderSize 不在 ImGuiStyleVar 枚舉中，
        /// 無法透過 Push/Pop 機制處理，故不在此注入（由 ApplyToImGui() 全域設定）。
        /// </para>
        /// </summary>
        internal void InjectToStyleHandler(ImGuiStyleHandler handler)
        {
            // --- Colors ---
            BuildColorMap((col, color) => InjectColor(handler, col, color));

            // BorderShadow = transparent black (special case: hardcoded, not a ColorFamily token)
            var shadowProp = new StyleProperty
            {
                category   = StyleCategory.ImGuiStyle,
                key        = (int)ImGuiCol.BorderShadow,
                dataType   = StyleDataType.Color,
                colorValue = 0u
            };
            shadowProp.isInheritable = true;
            handler.TrySetProperty(shadowProp);

            // --- Style Vars ---
            InjectVar2(handler, ImGuiStyleVar.WindowPadding,    padding);
            InjectVar2(handler, ImGuiStyleVar.FramePadding,     padding);
            InjectVar2(handler, ImGuiStyleVar.ItemSpacing,      itemSpacing);
            InjectVar2(handler, ImGuiStyleVar.ItemInnerSpacing, itemInnerSpacing);
            InjectVar1(handler, ImGuiStyleVar.WindowRounding,   borderRadius);
            InjectVar1(handler, ImGuiStyleVar.ChildRounding,    borderRadius);
            InjectVar1(handler, ImGuiStyleVar.FrameRounding,    borderRadius);
            InjectVar1(handler, ImGuiStyleVar.PopupRounding,    borderRadius);
            InjectVar1(handler, ImGuiStyleVar.ScrollbarRounding, borderRadius);
            InjectVar1(handler, ImGuiStyleVar.GrabRounding,     borderRadius);
            InjectVar1(handler, ImGuiStyleVar.TabRounding,      borderRadius);
            InjectVar1(handler, ImGuiStyleVar.WindowBorderSize, borderWidth);
            InjectVar1(handler, ImGuiStyleVar.ChildBorderSize,  borderWidth);
            InjectVar1(handler, ImGuiStyleVar.PopupBorderSize,  borderWidth);
            InjectVar1(handler, ImGuiStyleVar.FrameBorderSize,  borderWidth);
            InjectVar1(handler, ImGuiStyleVar.DisabledAlpha,    disabledAlpha);

            // --- Font ---
            var fontProp = new StyleProperty
            {
                category  = StyleCategory.ImGuiStyle,
                key       = ImGuiStyleHandler.s_fontFamilyImGuiKey.Hash,
                dataType  = StyleDataType.HashedString,
                tokenHash = fontFamilyHash
            };
            fontProp.isInheritable = true;
            handler.TrySetProperty(fontProp);
        }

        private static void InjectColor(ImGuiStyleHandler handler, ImGuiCol col, Color color)
        {
            var prop = new StyleProperty
            {
                category   = StyleCategory.ImGuiStyle,
                key        = (int)col,
                dataType   = StyleDataType.Color,
                colorValue = color.u32
            };
            prop.isInheritable = true;
            handler.TrySetProperty(prop);
        }

        private static void InjectVar1(ImGuiStyleHandler handler, ImGuiStyleVar styleVar, float value)
        {
            var prop = new StyleProperty
            {
                category   = StyleCategory.ImGuiStyle,
                key        = (int)styleVar,
                dataType   = StyleDataType.Float,
                floatValue = value
            };
            prop.isInheritable = true;
            handler.TrySetProperty(prop);
        }

        private static void InjectVar2(ImGuiStyleHandler handler, ImGuiStyleVar styleVar, Vector2 value)
        {
            var prop = new StyleProperty
            {
                category     = StyleCategory.ImGuiStyle,
                key          = (int)styleVar,
                dataType     = StyleDataType.Vector2,
                vector2Value = value
            };
            prop.isInheritable = true;
            handler.TrySetProperty(prop);
        }

        // --- Global Theme ---
        public static bool isGlobalThemeDirty = true;

        private static ImTKTheme s_globalTheme;
        public static ImTKTheme GlobalTheme
        {
            get => s_globalTheme ?? DefaultDark;
            set
            {
                if (s_globalTheme != value)
                {
                    s_globalTheme = value;
                    isGlobalThemeDirty = true;
                    onGlobalThemeChanged?.Invoke();
                }
            }
        }

        public static event Action onGlobalThemeChanged;

        // --- Default Themes ---

        private static ImTKTheme s_defaultDark;
        public static ImTKTheme DefaultDark
        {
            get
            {
                if (s_defaultDark == null)
                {
                    s_defaultDark = new ImTKTheme();

                    // Unity Editor Dark Theme styling
                    // Normal (Base)
                    s_defaultDark.normalColor.surface = new Color(0.12f, 0.12f, 0.12f, 1.0f);        // Pure Dark Grey
                    s_defaultDark.normalColor.container = new Color(0.15f, 0.15f, 0.15f, 1.0f);
                    s_defaultDark.normalColor.component = new Color(0.18f, 0.18f, 0.18f, 1.0f);
                    s_defaultDark.normalColor.componentHover = new Color(0.25f, 0.25f, 0.25f, 1.0f); // Pure Grayscale hover
                    s_defaultDark.normalColor.componentActive = new Color(0.30f, 0.30f, 0.30f, 1.0f); 
                    s_defaultDark.normalColor.accent = new Color(0.55f, 0.55f, 0.55f, 1.0f);           // Prominent Grayscale
                    s_defaultDark.normalColor.accentHover = new Color(0.65f, 0.65f, 0.65f, 1.0f);
                    s_defaultDark.normalColor.accentActive = new Color(0.45f, 0.45f, 0.45f, 1.0f);
                    s_defaultDark.normalColor.selection = new Color(0.25f, 0.25f, 0.25f, 0.8f);      // Pure Grayscale selection
                    s_defaultDark.normalColor.border = new Color(0.25f, 0.25f, 0.25f, 1.0f);
                    s_defaultDark.normalColor.divider = new Color(0.20f, 0.20f, 0.20f, 1.0f);
                    s_defaultDark.normalColor.text = new Color(0.9f, 0.9f, 0.9f, 1.0f);              // Pure Whiteish
                    s_defaultDark.normalColor.subText = new Color(0.6f, 0.6f, 0.6f, 1.0f);
                    s_defaultDark.normalColor.disabledText = new Color(0.4f, 0.4f, 0.4f, 1.0f);

                    // Success
                    s_defaultDark.successColor.surface = new Color(0.1f, 0.4f, 0.1f, 1.0f);
                    s_defaultDark.successColor.container = new Color(0.15f, 0.5f, 0.15f, 1.0f);
                    s_defaultDark.successColor.component = new Color(0.18f, 0.55f, 0.18f, 1.0f);
                    s_defaultDark.successColor.componentHover = new Color(0.22f, 0.6f, 0.22f, 1.0f);
                    s_defaultDark.successColor.componentActive = new Color(0.12f, 0.45f, 0.12f, 1.0f);
                    s_defaultDark.successColor.accent = new Color(0.2f, 0.7f, 0.2f, 1.0f);
                    s_defaultDark.successColor.accentHover = new Color(0.3f, 0.8f, 0.3f, 1.0f);
                    s_defaultDark.successColor.accentActive = new Color(0.15f, 0.6f, 0.15f, 1.0f);
                    s_defaultDark.successColor.selection = new Color(0.15f, 0.6f, 0.15f, 1.0f);
                    s_defaultDark.successColor.border = new Color(0.25f, 0.65f, 0.25f, 1.0f);
                    s_defaultDark.successColor.divider = new Color(0.20f, 0.60f, 0.20f, 1.0f);
                    s_defaultDark.successColor.text = new Color(0.1f, 0.9f, 0.1f, 1.0f);
                    s_defaultDark.successColor.subText = new Color(0.6f, 0.9f, 0.6f, 1.0f);
                    s_defaultDark.successColor.disabledText = new Color(0.4f, 0.6f, 0.4f, 1.0f);

                    // Info
                    s_defaultDark.infoColor.surface = new Color(0.1f, 0.3f, 0.4f, 1.0f);
                    s_defaultDark.infoColor.container = new Color(0.15f, 0.4f, 0.5f, 1.0f);
                    s_defaultDark.infoColor.component = new Color(0.18f, 0.45f, 0.55f, 1.0f);
                    s_defaultDark.infoColor.componentHover = new Color(0.22f, 0.5f, 0.6f, 1.0f);
                    s_defaultDark.infoColor.componentActive = new Color(0.12f, 0.35f, 0.45f, 1.0f);
                    s_defaultDark.infoColor.accent = new Color(0.2f, 0.6f, 0.8f, 1.0f);
                    s_defaultDark.infoColor.accentHover = new Color(0.3f, 0.7f, 0.9f, 1.0f);
                    s_defaultDark.infoColor.accentActive = new Color(0.15f, 0.5f, 0.7f, 1.0f);
                    s_defaultDark.infoColor.selection = new Color(0.18f, 0.5f, 0.6f, 1.0f);
                    s_defaultDark.infoColor.border = new Color(0.25f, 0.55f, 0.65f, 1.0f);
                    s_defaultDark.infoColor.divider = new Color(0.20f, 0.50f, 0.60f, 1.0f);
                    s_defaultDark.infoColor.text = new Color(0.4f, 0.8f, 1.0f, 1.0f);
                    s_defaultDark.infoColor.subText = new Color(0.6f, 0.8f, 0.9f, 1.0f);
                    s_defaultDark.infoColor.disabledText = new Color(0.4f, 0.5f, 0.6f, 1.0f);

                    // Warning
                    s_defaultDark.warningColor.surface = new Color(0.4f, 0.4f, 0.1f, 1.0f);
                    s_defaultDark.warningColor.container = new Color(0.5f, 0.5f, 0.15f, 1.0f);
                    s_defaultDark.warningColor.component = new Color(0.55f, 0.55f, 0.18f, 1.0f);
                    s_defaultDark.warningColor.componentHover = new Color(0.6f, 0.6f, 0.22f, 1.0f);
                    s_defaultDark.warningColor.componentActive = new Color(0.45f, 0.45f, 0.12f, 1.0f);
                    s_defaultDark.warningColor.accent = new Color(0.8f, 0.8f, 0.2f, 1.0f);
                    s_defaultDark.warningColor.accentHover = new Color(0.9f, 0.9f, 0.3f, 1.0f);
                    s_defaultDark.warningColor.accentActive = new Color(0.7f, 0.7f, 0.15f, 1.0f);
                    s_defaultDark.warningColor.selection = new Color(0.6f, 0.6f, 0.18f, 1.0f);
                    s_defaultDark.warningColor.border = new Color(0.65f, 0.65f, 0.25f, 1.0f);
                    s_defaultDark.warningColor.divider = new Color(0.60f, 0.60f, 0.20f, 1.0f);
                    s_defaultDark.warningColor.text = new Color(0.9f, 0.9f, 0.1f, 1.0f);
                    s_defaultDark.warningColor.subText = new Color(0.9f, 0.9f, 0.6f, 1.0f);
                    s_defaultDark.warningColor.disabledText = new Color(0.6f, 0.6f, 0.4f, 1.0f);

                    // Danger
                    s_defaultDark.dangerColor.surface = new Color(0.4f, 0.1f, 0.1f, 1.0f);
                    s_defaultDark.dangerColor.container = new Color(0.5f, 0.15f, 0.15f, 1.0f);
                    s_defaultDark.dangerColor.component = new Color(0.55f, 0.18f, 0.18f, 1.0f);
                    s_defaultDark.dangerColor.componentHover = new Color(0.6f, 0.22f, 0.22f, 1.0f);
                    s_defaultDark.dangerColor.componentActive = new Color(0.45f, 0.12f, 0.12f, 1.0f);
                    s_defaultDark.dangerColor.accent = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                    s_defaultDark.dangerColor.accentHover = new Color(0.9f, 0.3f, 0.3f, 1.0f);
                    s_defaultDark.dangerColor.accentActive = new Color(0.7f, 0.15f, 0.15f, 1.0f);
                    s_defaultDark.dangerColor.selection = new Color(0.6f, 0.18f, 0.18f, 1.0f);
                    s_defaultDark.dangerColor.border = new Color(0.65f, 0.25f, 0.25f, 1.0f);
                    s_defaultDark.dangerColor.divider = new Color(0.60f, 0.20f, 0.20f, 1.0f);
                    s_defaultDark.dangerColor.text = new Color(1.0f, 0.4f, 0.4f, 1.0f);
                    s_defaultDark.dangerColor.subText = new Color(0.9f, 0.6f, 0.6f, 1.0f);
                    s_defaultDark.dangerColor.disabledText = new Color(0.6f, 0.4f, 0.4f, 1.0f);

                    // Syntax
                    s_defaultDark.syntax.funcColor = new Color(0.89f, 0.79f, 0.35f, 1.0f);
                    s_defaultDark.syntax.argsColor = new Color(0.65f, 0.85f, 0.95f, 1.0f);
                    s_defaultDark.syntax.typeColor = new Color(0.35f, 0.7f, 0.65f, 1.0f);
                    s_defaultDark.syntax.valueTypeColor = new Color(0.55f, 0.8f, 0.75f, 1.0f);
                    s_defaultDark.syntax.prefixColor = new Color(0.4f, 0.56f, 0.82f, 1.0f);
                    s_defaultDark.syntax.stringColor = new Color(0.79f, 0.56f, 0.36f, 1.0f);
                    s_defaultDark.syntax.numberColor = new Color(0.6f, 0.8f, 0.6f, 1.0f);
                    s_defaultDark.syntax.controlColor = new Color(0.84f, 0.45f, 0.61f, 1.0f);
                    s_defaultDark.syntax.commentsColor = new Color(0.4f, 0.6f, 0.35f, 1.0f);
                    s_defaultDark.syntax.codeBackgroundColor = new Color(0.08f, 0.08f, 0.09f, 1.0f);
                    s_defaultDark.syntax.codeTextColor = new Color(0.85f, 0.85f, 0.85f, 1.0f);

                    // Global Properties
                    s_defaultDark.labelWidth = 180.0f;
                    s_defaultDark.edgePadding = 10.0f;
                    s_defaultDark.indentWidth = 16.0f;
                    s_defaultDark.padding = new Vector2(4, 3);
                    s_defaultDark.itemSpacing = new Vector2(8, 4);
                    s_defaultDark.itemInnerSpacing = new Vector2(4, 4);
                    s_defaultDark.borderWidth = 1.0f;
                    s_defaultDark.borderRadius = 3.0f;
                    s_defaultDark.disabledAlpha = 0.6f;
                }
                return s_defaultDark;
            }
        }

        private static ImTKTheme s_defaultLight;
        public static ImTKTheme DefaultLight
        {
            get
            {
                if (s_defaultLight == null)
                {
                    s_defaultLight = new ImTKTheme();

                    // Unity Editor Light Theme styling
                    // Normal (Base)
                    s_defaultLight.normalColor.surface = new Color(0.82f, 0.82f, 0.82f, 1.0f);       // Pure Light Grey (Darkest low-level)
                    s_defaultLight.normalColor.container = new Color(0.86f, 0.86f, 0.86f, 1.0f);
                    s_defaultLight.normalColor.component = new Color(0.90f, 0.90f, 0.90f, 1.0f);
                    s_defaultLight.normalColor.componentHover = new Color(0.94f, 0.94f, 0.94f, 1.0f); // Pure Grayscale hover (Lighter)
                    s_defaultLight.normalColor.componentActive = new Color(0.98f, 0.98f, 0.98f, 1.0f); // Active (Lightest high-level)
                    s_defaultLight.normalColor.accent = new Color(0.45f, 0.45f, 0.45f, 1.0f);          // Prominent Grayscale
                    s_defaultLight.normalColor.accentHover = new Color(0.55f, 0.55f, 0.55f, 1.0f);
                    s_defaultLight.normalColor.accentActive = new Color(0.35f, 0.35f, 0.35f, 1.0f);
                    s_defaultLight.normalColor.selection = new Color(0.90f, 0.90f, 0.90f, 0.8f);      // Pure Grayscale selection
                    s_defaultLight.normalColor.border = new Color(0.70f, 0.70f, 0.70f, 1.0f);
                    s_defaultLight.normalColor.divider = new Color(0.75f, 0.75f, 0.75f, 1.0f);
                    s_defaultLight.normalColor.text = new Color(0.1f, 0.1f, 0.1f, 1.0f);              // Pure Darkish
                    s_defaultLight.normalColor.subText = new Color(0.4f, 0.4f, 0.4f, 1.0f);
                    s_defaultLight.normalColor.disabledText = new Color(0.6f, 0.6f, 0.6f, 1.0f);

                    // Success
                    s_defaultLight.successColor.surface = new Color(0.8f, 0.95f, 0.8f, 1.0f);
                    s_defaultLight.successColor.container = new Color(0.7f, 0.9f, 0.7f, 1.0f);
                    s_defaultLight.successColor.component = new Color(0.75f, 0.95f, 0.75f, 1.0f);
                    s_defaultLight.successColor.componentHover = new Color(0.85f, 0.98f, 0.85f, 1.0f);
                    s_defaultLight.successColor.componentActive = new Color(0.65f, 0.85f, 0.65f, 1.0f);
                    s_defaultLight.successColor.accent = new Color(0.2f, 0.7f, 0.2f, 1.0f);
                    s_defaultLight.successColor.accentHover = new Color(0.3f, 0.8f, 0.3f, 1.0f);
                    s_defaultLight.successColor.accentActive = new Color(0.1f, 0.6f, 0.1f, 1.0f);
                    s_defaultLight.successColor.selection = new Color(0.6f, 0.8f, 0.6f, 1.0f);
                    s_defaultLight.successColor.border = new Color(0.6f, 0.8f, 0.6f, 1.0f);
                    s_defaultLight.successColor.divider = new Color(0.55f, 0.75f, 0.55f, 1.0f);
                    s_defaultLight.successColor.text = new Color(0.05f, 0.5f, 0.05f, 1.0f);
                    s_defaultLight.successColor.subText = new Color(0.2f, 0.6f, 0.2f, 1.0f);
                    s_defaultLight.successColor.disabledText = new Color(0.6f, 0.7f, 0.6f, 1.0f);

                    // Info
                    s_defaultLight.infoColor.surface = new Color(0.8f, 0.9f, 0.95f, 1.0f);
                    s_defaultLight.infoColor.container = new Color(0.7f, 0.85f, 0.9f, 1.0f);
                    s_defaultLight.infoColor.component = new Color(0.75f, 0.9f, 0.95f, 1.0f);
                    s_defaultLight.infoColor.componentHover = new Color(0.85f, 0.95f, 0.98f, 1.0f);
                    s_defaultLight.infoColor.componentActive = new Color(0.65f, 0.8f, 0.85f, 1.0f);
                    s_defaultLight.infoColor.accent = new Color(0.2f, 0.6f, 0.8f, 1.0f);
                    s_defaultLight.infoColor.accentHover = new Color(0.3f, 0.7f, 0.9f, 1.0f);
                    s_defaultLight.infoColor.accentActive = new Color(0.1f, 0.5f, 0.7f, 1.0f);
                    s_defaultLight.infoColor.selection = new Color(0.6f, 0.75f, 0.85f, 1.0f);
                    s_defaultLight.infoColor.border = new Color(0.6f, 0.75f, 0.8f, 1.0f);
                    s_defaultLight.infoColor.divider = new Color(0.55f, 0.7f, 0.75f, 1.0f);
                    s_defaultLight.infoColor.text = new Color(0.05f, 0.3f, 0.5f, 1.0f);
                    s_defaultLight.infoColor.subText = new Color(0.2f, 0.5f, 0.7f, 1.0f);
                    s_defaultLight.infoColor.disabledText = new Color(0.6f, 0.7f, 0.8f, 1.0f);

                    // Warning
                    s_defaultLight.warningColor.surface = new Color(0.95f, 0.95f, 0.8f, 1.0f);
                    s_defaultLight.warningColor.container = new Color(0.9f, 0.9f, 0.7f, 1.0f);
                    s_defaultLight.warningColor.component = new Color(0.95f, 0.95f, 0.75f, 1.0f);
                    s_defaultLight.warningColor.componentHover = new Color(0.98f, 0.98f, 0.85f, 1.0f);
                    s_defaultLight.warningColor.componentActive = new Color(0.85f, 0.85f, 0.65f, 1.0f);
                    s_defaultLight.warningColor.accent = new Color(0.8f, 0.8f, 0.2f, 1.0f);
                    s_defaultLight.warningColor.accentHover = new Color(0.9f, 0.9f, 0.3f, 1.0f);
                    s_defaultLight.warningColor.accentActive = new Color(0.7f, 0.7f, 0.1f, 1.0f);
                    s_defaultLight.warningColor.selection = new Color(0.85f, 0.85f, 0.6f, 1.0f);
                    s_defaultLight.warningColor.border = new Color(0.8f, 0.8f, 0.6f, 1.0f);
                    s_defaultLight.warningColor.divider = new Color(0.75f, 0.75f, 0.55f, 1.0f);
                    s_defaultLight.warningColor.text = new Color(0.5f, 0.5f, 0.05f, 1.0f);
                    s_defaultLight.warningColor.subText = new Color(0.6f, 0.6f, 0.2f, 1.0f);
                    s_defaultLight.warningColor.disabledText = new Color(0.7f, 0.7f, 0.6f, 1.0f);

                    // Danger
                    s_defaultLight.dangerColor.surface = new Color(0.95f, 0.8f, 0.8f, 1.0f);
                    s_defaultLight.dangerColor.container = new Color(0.9f, 0.7f, 0.7f, 1.0f);
                    s_defaultLight.dangerColor.component = new Color(0.95f, 0.75f, 0.75f, 1.0f);
                    s_defaultLight.dangerColor.componentHover = new Color(0.98f, 0.85f, 0.85f, 1.0f);
                    s_defaultLight.dangerColor.componentActive = new Color(0.85f, 0.65f, 0.65f, 1.0f);
                    s_defaultLight.dangerColor.accent = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                    s_defaultLight.dangerColor.accentHover = new Color(0.9f, 0.3f, 0.3f, 1.0f);
                    s_defaultLight.dangerColor.accentActive = new Color(0.7f, 0.1f, 0.1f, 1.0f);
                    s_defaultLight.dangerColor.selection = new Color(0.85f, 0.6f, 0.6f, 1.0f);
                    s_defaultLight.dangerColor.border = new Color(0.8f, 0.6f, 0.6f, 1.0f);
                    s_defaultLight.dangerColor.divider = new Color(0.75f, 0.55f, 0.55f, 1.0f);
                    s_defaultLight.dangerColor.text = new Color(0.6f, 0.05f, 0.05f, 1.0f);
                    s_defaultLight.dangerColor.subText = new Color(0.7f, 0.2f, 0.2f, 1.0f);
                    s_defaultLight.dangerColor.disabledText = new Color(0.7f, 0.6f, 0.6f, 1.0f);

                    // Syntax
                    s_defaultLight.syntax.funcColor = new Color(0.6f, 0.5f, 0.1f, 1.0f);
                    s_defaultLight.syntax.argsColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
                    s_defaultLight.syntax.typeColor = new Color(0.1f, 0.5f, 0.4f, 1.0f);
                    s_defaultLight.syntax.valueTypeColor = new Color(0.2f, 0.6f, 0.5f, 1.0f);
                    s_defaultLight.syntax.prefixColor = new Color(0.2f, 0.3f, 0.6f, 1.0f);
                    s_defaultLight.syntax.stringColor = new Color(0.6f, 0.3f, 0.1f, 1.0f);
                    s_defaultLight.syntax.numberColor = new Color(0.2f, 0.6f, 0.2f, 1.0f);
                    s_defaultLight.syntax.controlColor = new Color(0.7f, 0.2f, 0.4f, 1.0f);
                    s_defaultLight.syntax.commentsColor = new Color(0.2f, 0.5f, 0.1f, 1.0f);
                    s_defaultLight.syntax.codeBackgroundColor = new Color(0.92f, 0.92f, 0.92f, 1.0f);
                    s_defaultLight.syntax.codeTextColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

                    // Global Properties
                    s_defaultLight.labelWidth = 180.0f;
                    s_defaultLight.edgePadding = 10.0f;
                    s_defaultLight.indentWidth = 16.0f;
                    s_defaultLight.padding = new Vector2(4, 3);
                    s_defaultLight.itemSpacing = new Vector2(8, 4);
                    s_defaultLight.itemInnerSpacing = new Vector2(4, 4);
                    s_defaultLight.borderWidth = 1.0f;
                    s_defaultLight.borderRadius = 3.0f;
                    s_defaultLight.disabledAlpha = 0.6f;
                }
                return s_defaultLight;
            }
        }
    }
}
