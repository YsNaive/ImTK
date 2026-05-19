using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class ImTKTheme
    {
        public ImTKTheme parent { get; set; }

        private Dictionary<int, Color> m_colorTokens = new Dictionary<int, Color>();
        private Dictionary<int, float> m_floatTokens = new Dictionary<int, float>();
        private Dictionary<int, Vector2> m_vector2Tokens = new Dictionary<int, Vector2>();

        public void SetColorToken(HashedString token, Color color) => m_colorTokens[token.Hash] = color;
        public void SetFloatToken(HashedString token, float value) => m_floatTokens[token.Hash] = value;
        public void SetVector2Token(HashedString token, Vector2 value) => m_vector2Tokens[token.Hash] = value;

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

        internal Color GetColor(HashedString token, Color fallback = default) => TryGetColorToken(token.Hash, out Color c) ? c : fallback;
        internal void SetColor(HashedString token, Color value) => SetColorToken(token, value);

        internal float GetFloat(HashedString token, float fallback = 0f) => TryGetFloatToken(token.Hash, out float f) ? f : fallback;
        internal void SetFloat(HashedString token, float value) => SetFloatToken(token, value);

        internal Vector2 GetVector2(HashedString token, Vector2 fallback = default) => TryGetVector2Token(token.Hash, out Vector2 v) ? v : fallback;
        internal void SetVector2(HashedString token, Vector2 value) => SetVector2Token(token, value);

        // --- Cached Token Keys ---
        public static class Tokens
        {
            public static readonly HashedString LabelWidth = new HashedString("--label-width");
            public static readonly HashedString EdgePadding = new HashedString("--edge-padding");

            public static readonly HashedString BorderColor = new HashedString("--border-color");
            public static readonly HashedString CheckMarkColor = new HashedString("--checkmark-color");
            public static readonly HashedString Padding = new HashedString("--padding");
            public static readonly HashedString ItemSpacing = new HashedString("--item-spacing");
            public static readonly HashedString ItemInnerSpacing = new HashedString("--item-inner-spacing");
            public static readonly HashedString BorderWidth = new HashedString("--border-width");
            public static readonly HashedString BorderRadius = new HashedString("--border-radius");
            public static readonly HashedString DisabledAlpha = new HashedString("--disabled-alpha");

            public static readonly HashedString NormalBg = new HashedString("--normal-bg");
            public static readonly HashedString NormalSubBg = new HashedString("--normal-sub-bg");
            public static readonly HashedString NormalFg = new HashedString("--normal-fg");
            public static readonly HashedString NormalSubFg = new HashedString("--normal-sub-fg");
            public static readonly HashedString NormalText = new HashedString("--normal-text");

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
            private HashedString _bg, _subBg, _fg, _subFg, _text, _subText, _disabledText;

            public ColorFamily(ImTKTheme theme, string prefix)
            {
                _theme = theme;
                _bg = new HashedString(prefix + "-bg");
                _subBg = new HashedString(prefix + "-sub-bg");
                _fg = new HashedString(prefix + "-fg");
                _subFg = new HashedString(prefix + "-sub-fg");
                _text = new HashedString(prefix + "-text");
                _subText = new HashedString(prefix + "-sub-text");
                _disabledText = new HashedString(prefix + "-disabled-text");
            }

            public Color background { get => _theme.GetColor(_bg); set => _theme.SetColor(_bg, value); }
            public Color subBackground { get => _theme.GetColor(_subBg); set => _theme.SetColor(_subBg, value); }
            public Color foreground { get => _theme.GetColor(_fg); set => _theme.SetColor(_fg, value); }
            public Color subForeground { get => _theme.GetColor(_subFg); set => _theme.SetColor(_subFg, value); }
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
        public Color borderColor { get => GetColor(Tokens.BorderColor); set => SetColor(Tokens.BorderColor, value); }
        public Color checkMarkColor { get => GetColor(Tokens.CheckMarkColor); set => SetColor(Tokens.CheckMarkColor, value); }

        public float labelWidth { get => GetFloat(Tokens.LabelWidth, 120f); set => SetFloat(Tokens.LabelWidth, value); }
        public float edgePadding { get => GetFloat(Tokens.EdgePadding, 10f); set => SetFloat(Tokens.EdgePadding, value); }

        public Vector2 padding { get => GetVector2(Tokens.Padding, new Vector2(4, 3)); set => SetVector2(Tokens.Padding, value); }
        public Vector2 itemSpacing { get => GetVector2(Tokens.ItemSpacing, new Vector2(8, 4)); set => SetVector2(Tokens.ItemSpacing, value); }
        public Vector2 itemInnerSpacing { get => GetVector2(Tokens.ItemInnerSpacing, new Vector2(4, 4)); set => SetVector2(Tokens.ItemInnerSpacing, value); }
        public float borderWidth { get => GetFloat(Tokens.BorderWidth, 1f); set => SetFloat(Tokens.BorderWidth, value); }
        public float borderRadius { get => GetFloat(Tokens.BorderRadius, 3f); set => SetFloat(Tokens.BorderRadius, value); }
        public float disabledAlpha { get => GetFloat(Tokens.DisabledAlpha, 0.6f); set => SetFloat(Tokens.DisabledAlpha, value); }

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


        // --- Global Theme ---
        private static ImTKTheme s_globalTheme;
        public static ImTKTheme GlobalTheme
        {
            get => s_globalTheme ?? DefaultDark;
            set
            {
                if (s_globalTheme != value)
                {
                    s_globalTheme = value;
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

                    // Normal (Base)
                    s_defaultDark.normalColor.background = new Color(0.06f, 0.06f, 0.06f, 1.0f);
                    s_defaultDark.normalColor.subBackground = new Color(0.11f, 0.11f, 0.11f, 1.0f);
                    s_defaultDark.normalColor.foreground = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    s_defaultDark.normalColor.subForeground = new Color(0.20f, 0.40f, 0.70f, 1.0f);
                    s_defaultDark.normalColor.text = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    s_defaultDark.normalColor.subText = new Color(0.7f, 0.7f, 0.7f, 1.0f);
                    s_defaultDark.normalColor.disabledText = new Color(0.5f, 0.5f, 0.5f, 1.0f);

                    // Success
                    s_defaultDark.successColor.background = new Color(0.1f, 0.4f, 0.1f, 1.0f);
                    s_defaultDark.successColor.subBackground = new Color(0.15f, 0.5f, 0.15f, 1.0f);
                    s_defaultDark.successColor.foreground = new Color(0.2f, 0.7f, 0.2f, 1.0f);
                    s_defaultDark.successColor.subForeground = new Color(0.25f, 0.8f, 0.25f, 1.0f);
                    s_defaultDark.successColor.text = new Color(0.1f, 0.9f, 0.1f, 1.0f);
                    s_defaultDark.successColor.subText = new Color(0.6f, 0.9f, 0.6f, 1.0f);
                    s_defaultDark.successColor.disabledText = new Color(0.4f, 0.6f, 0.4f, 1.0f);

                    // Info
                    s_defaultDark.infoColor.background = new Color(0.1f, 0.3f, 0.4f, 1.0f);
                    s_defaultDark.infoColor.subBackground = new Color(0.15f, 0.4f, 0.5f, 1.0f);
                    s_defaultDark.infoColor.foreground = new Color(0.2f, 0.6f, 0.8f, 1.0f);
                    s_defaultDark.infoColor.subForeground = new Color(0.25f, 0.7f, 0.9f, 1.0f);
                    s_defaultDark.infoColor.text = new Color(0.4f, 0.8f, 1.0f, 1.0f);
                    s_defaultDark.infoColor.subText = new Color(0.6f, 0.8f, 0.9f, 1.0f);
                    s_defaultDark.infoColor.disabledText = new Color(0.4f, 0.5f, 0.6f, 1.0f);

                    // Warning
                    s_defaultDark.warningColor.background = new Color(0.4f, 0.4f, 0.1f, 1.0f);
                    s_defaultDark.warningColor.subBackground = new Color(0.5f, 0.5f, 0.15f, 1.0f);
                    s_defaultDark.warningColor.foreground = new Color(0.8f, 0.8f, 0.2f, 1.0f);
                    s_defaultDark.warningColor.subForeground = new Color(0.9f, 0.9f, 0.25f, 1.0f);
                    s_defaultDark.warningColor.text = new Color(0.9f, 0.9f, 0.1f, 1.0f);
                    s_defaultDark.warningColor.subText = new Color(0.9f, 0.9f, 0.6f, 1.0f);
                    s_defaultDark.warningColor.disabledText = new Color(0.6f, 0.6f, 0.4f, 1.0f);

                    // Danger
                    s_defaultDark.dangerColor.background = new Color(0.4f, 0.1f, 0.1f, 1.0f);
                    s_defaultDark.dangerColor.subBackground = new Color(0.5f, 0.15f, 0.15f, 1.0f);
                    s_defaultDark.dangerColor.foreground = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                    s_defaultDark.dangerColor.subForeground = new Color(0.9f, 0.25f, 0.25f, 1.0f);
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
                    s_defaultDark.borderColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
                    s_defaultDark.checkMarkColor = s_defaultDark.normalColor.foreground;
                    s_defaultDark.labelWidth = 120.0f;
                    s_defaultDark.edgePadding = 10.0f;
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

                    // Normal (Base)
                    s_defaultLight.normalColor.background = new Color(0.9f, 0.9f, 0.9f, 1.0f);
                    s_defaultLight.normalColor.subBackground = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                    s_defaultLight.normalColor.foreground = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    s_defaultLight.normalColor.subForeground = new Color(0.5f, 0.7f, 0.95f, 1.0f);
                    s_defaultLight.normalColor.text = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                    s_defaultLight.normalColor.subText = new Color(0.3f, 0.3f, 0.3f, 1.0f);
                    s_defaultLight.normalColor.disabledText = new Color(0.5f, 0.5f, 0.5f, 1.0f);

                    // Success
                    s_defaultLight.successColor.background = new Color(0.8f, 0.95f, 0.8f, 1.0f);
                    s_defaultLight.successColor.subBackground = new Color(0.7f, 0.9f, 0.7f, 1.0f);
                    s_defaultLight.successColor.foreground = new Color(0.2f, 0.7f, 0.2f, 1.0f);
                    s_defaultLight.successColor.subForeground = new Color(0.3f, 0.8f, 0.3f, 1.0f);
                    s_defaultLight.successColor.text = new Color(0.05f, 0.5f, 0.05f, 1.0f);
                    s_defaultLight.successColor.subText = new Color(0.2f, 0.6f, 0.2f, 1.0f);
                    s_defaultLight.successColor.disabledText = new Color(0.6f, 0.7f, 0.6f, 1.0f);

                    // Info
                    s_defaultLight.infoColor.background = new Color(0.8f, 0.9f, 0.95f, 1.0f);
                    s_defaultLight.infoColor.subBackground = new Color(0.7f, 0.85f, 0.9f, 1.0f);
                    s_defaultLight.infoColor.foreground = new Color(0.2f, 0.6f, 0.8f, 1.0f);
                    s_defaultLight.infoColor.subForeground = new Color(0.3f, 0.7f, 0.9f, 1.0f);
                    s_defaultLight.infoColor.text = new Color(0.05f, 0.3f, 0.5f, 1.0f);
                    s_defaultLight.infoColor.subText = new Color(0.2f, 0.5f, 0.7f, 1.0f);
                    s_defaultLight.infoColor.disabledText = new Color(0.6f, 0.7f, 0.8f, 1.0f);

                    // Warning
                    s_defaultLight.warningColor.background = new Color(0.95f, 0.95f, 0.8f, 1.0f);
                    s_defaultLight.warningColor.subBackground = new Color(0.9f, 0.9f, 0.7f, 1.0f);
                    s_defaultLight.warningColor.foreground = new Color(0.8f, 0.8f, 0.2f, 1.0f);
                    s_defaultLight.warningColor.subForeground = new Color(0.9f, 0.9f, 0.3f, 1.0f);
                    s_defaultLight.warningColor.text = new Color(0.5f, 0.5f, 0.05f, 1.0f);
                    s_defaultLight.warningColor.subText = new Color(0.6f, 0.6f, 0.2f, 1.0f);
                    s_defaultLight.warningColor.disabledText = new Color(0.7f, 0.7f, 0.6f, 1.0f);

                    // Danger
                    s_defaultLight.dangerColor.background = new Color(0.95f, 0.8f, 0.8f, 1.0f);
                    s_defaultLight.dangerColor.subBackground = new Color(0.9f, 0.7f, 0.7f, 1.0f);
                    s_defaultLight.dangerColor.foreground = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                    s_defaultLight.dangerColor.subForeground = new Color(0.9f, 0.3f, 0.3f, 1.0f);
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
                    s_defaultLight.syntax.codeBackgroundColor = new Color(0.95f, 0.95f, 0.95f, 1.0f);
                    s_defaultLight.syntax.codeTextColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

                    // Global Properties
                    s_defaultLight.borderColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
                    s_defaultLight.checkMarkColor = s_defaultLight.normalColor.foreground;
                    s_defaultLight.labelWidth = 120.0f;
                    s_defaultLight.edgePadding = 10.0f;
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
