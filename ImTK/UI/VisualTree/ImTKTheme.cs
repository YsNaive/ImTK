using System;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class ImTKTheme
    {
        public ImTKTheme parent { get; set; }

        private System.Collections.Generic.Dictionary<int, Color> m_colorTokens = new System.Collections.Generic.Dictionary<int, Color>();
        private System.Collections.Generic.Dictionary<int, float> m_floatTokens = new System.Collections.Generic.Dictionary<int, float>();
        private System.Collections.Generic.Dictionary<int, Vector2> m_vector2Tokens = new System.Collections.Generic.Dictionary<int, Vector2>();

        public void SetColorToken(HashedString token, Color color)
        {
            m_colorTokens[token.Hash] = color;
        }

        public void SetFloatToken(HashedString token, float value)
        {
            m_floatTokens[token.Hash] = value;
        }

        public void SetVector2Token(HashedString token, Vector2 value)
        {
            m_vector2Tokens[token.Hash] = value;
        }

        public bool TryGetColorToken(int tokenHash, out Color color)
        {
            if (m_colorTokens.TryGetValue(tokenHash, out color))
                return true;
            if (parent != null)
                return parent.TryGetColorToken(tokenHash, out color);
            color = default;
            return false;
        }

        public bool TryGetFloatToken(int tokenHash, out float value)
        {
            if (m_floatTokens.TryGetValue(tokenHash, out value))
                return true;
            if (parent != null)
                return parent.TryGetFloatToken(tokenHash, out value);
            value = default;
            return false;
        }

        public bool TryGetVector2Token(int tokenHash, out Vector2 value)
        {
            if (m_vector2Tokens.TryGetValue(tokenHash, out value))
                return true;
            if (parent != null)
                return parent.TryGetVector2Token(tokenHash, out value);
            value = default;
            return false;
        }

        // --- Helper for Syntax Sugar ---
        private Color GetColor(HashedString token, Color fallback = default)
        {
            return TryGetColorToken(token.Hash, out Color c) ? c : fallback;
        }
        private void SetColor(HashedString token, Color value)
        {
            SetColorToken(token, value);
        }

        private float GetFloat(HashedString token, float fallback = 0f)
        {
            return TryGetFloatToken(token.Hash, out float f) ? f : fallback;
        }
        private void SetFloat(HashedString token, float value)
        {
            SetFloatToken(token, value);
        }

        // --- Cached Token Keys ---
        public static class Tokens
        {
            public static readonly HashedString BackgroundColor = new HashedString("--background-color");
            public static readonly HashedString SubBackgroundColor = new HashedString("--sub-background-color");
            public static readonly HashedString ForegroundColor = new HashedString("--foreground-color");
            public static readonly HashedString SubForegroundColor = new HashedString("--sub-foreground-color");

            public static readonly HashedString SuccessColor = new HashedString("--success-color");
            public static readonly HashedString SuccessTextColor = new HashedString("--success-text-color");
            public static readonly HashedString WarningColor = new HashedString("--warning-color");
            public static readonly HashedString WarningTextColor = new HashedString("--warning-text-color");
            public static readonly HashedString DangerColor = new HashedString("--danger-color");
            public static readonly HashedString DangerTextColor = new HashedString("--danger-text-color");
            public static readonly HashedString HintColor = new HashedString("--hint-color");
            public static readonly HashedString HintTextColor = new HashedString("--hint-text-color");

            public static readonly HashedString LabelWidth = new HashedString("--label-width");
            public static readonly HashedString EdgePadding = new HashedString("--edge-padding");

            // Temporary backwards compatibility tokens
            public static readonly HashedString Background1 = new HashedString("--background-1");
            public static readonly HashedString Background2 = new HashedString("--background-2");
            public static readonly HashedString TextPrimary = new HashedString("--text-primary");
            public static readonly HashedString PrimaryColor = new HashedString("--primary-color");
            public static readonly HashedString ButtonHovered = new HashedString("--button-hovered");
            public static readonly HashedString ButtonActive = new HashedString("--button-active");

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

        // --- Base Colors ---
        public Color backgroundColor { get => GetColor(Tokens.BackgroundColor); set => SetColor(Tokens.BackgroundColor, value); }
        public Color subBackgroundColor { get => GetColor(Tokens.SubBackgroundColor); set => SetColor(Tokens.SubBackgroundColor, value); }
        public Color foregroundColor { get => GetColor(Tokens.ForegroundColor); set => SetColor(Tokens.ForegroundColor, value); }
        public Color subForegroundColor { get => GetColor(Tokens.SubForegroundColor); set => SetColor(Tokens.SubForegroundColor, value); }

        // --- Semantic Colors ---
        public Color successColor { get => GetColor(Tokens.SuccessColor); set => SetColor(Tokens.SuccessColor, value); }
        public Color successTextColor { get => GetColor(Tokens.SuccessTextColor); set => SetColor(Tokens.SuccessTextColor, value); }
        public Color warningColor { get => GetColor(Tokens.WarningColor); set => SetColor(Tokens.WarningColor, value); }
        public Color warningTextColor { get => GetColor(Tokens.WarningTextColor); set => SetColor(Tokens.WarningTextColor, value); }
        public Color dangerColor { get => GetColor(Tokens.DangerColor); set => SetColor(Tokens.DangerColor, value); }
        public Color dangerTextColor { get => GetColor(Tokens.DangerTextColor); set => SetColor(Tokens.DangerTextColor, value); }
        public Color hintColor { get => GetColor(Tokens.HintColor); set => SetColor(Tokens.HintColor, value); }
        public Color hintTextColor { get => GetColor(Tokens.HintTextColor); set => SetColor(Tokens.HintTextColor, value); }

        // --- Layout Tokens ---
        public float labelWidth { get => GetFloat(Tokens.LabelWidth, 120f); set => SetFloat(Tokens.LabelWidth, value); }
        public float edgePadding { get => GetFloat(Tokens.EdgePadding, 10f); set => SetFloat(Tokens.EdgePadding, value); }

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


        // --- Default Themes ---

        private static ImTKTheme s_defaultDark;
        public static ImTKTheme DefaultDark
        {
            get
            {
                if (s_defaultDark == null)
                {
                    s_defaultDark = new ImTKTheme();
                    // Original Dark mapping mapped to new Base syntax
                    s_defaultDark.backgroundColor = new Color(0.06f, 0.06f, 0.06f, 1.0f);
                    s_defaultDark.subBackgroundColor = new Color(0.11f, 0.11f, 0.11f, 1.0f);
                    s_defaultDark.foregroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    s_defaultDark.subForegroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

                    // Keeping original tokens for backwards compatibility temporarily
                    s_defaultDark.SetColorToken(Tokens.Background1, s_defaultDark.backgroundColor);
                    s_defaultDark.SetColorToken(Tokens.Background2, s_defaultDark.subBackgroundColor);
                    s_defaultDark.SetColorToken(Tokens.TextPrimary, s_defaultDark.foregroundColor);
                    s_defaultDark.SetColorToken(Tokens.PrimaryColor, new Color(0.15f, 0.35f, 0.65f, 1.0f));

                    Color hover = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    hover.v = Math.Min(1.0f, hover.v + 0.1f);
                    s_defaultDark.SetColorToken(Tokens.ButtonHovered, hover);

                    Color active = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    active.v = Math.Max(0.0f, active.v - 0.1f);
                    s_defaultDark.SetColorToken(Tokens.ButtonActive, active);

                    // Semantic
                    s_defaultDark.successColor = new Color(0.2f, 0.7f, 0.2f, 1.0f);
                    s_defaultDark.warningColor = new Color(0.8f, 0.8f, 0.2f, 1.0f);
                    s_defaultDark.dangerColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                    s_defaultDark.hintColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

                    s_defaultDark.successTextColor = new Color(0.1f, 0.9f, 0.1f, 1.0f);
                    s_defaultDark.warningTextColor = new Color(0.9f, 0.9f, 0.1f, 1.0f);
                    s_defaultDark.dangerTextColor = new Color(0.9f, 0.1f, 0.1f, 1.0f);
                    s_defaultDark.hintTextColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);

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

                    // Layout
                    s_defaultDark.labelWidth = 120.0f;
                    s_defaultDark.edgePadding = 10.0f;
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
                    // Original Light mapping mapped to new Base syntax
                    s_defaultLight.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
                    s_defaultLight.subBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                    s_defaultLight.foregroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                    s_defaultLight.subForegroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

                    // Keeping original tokens for backwards compatibility temporarily
                    s_defaultLight.SetColorToken(Tokens.Background1, s_defaultLight.backgroundColor);
                    s_defaultLight.SetColorToken(Tokens.Background2, s_defaultLight.subBackgroundColor);
                    s_defaultLight.SetColorToken(Tokens.TextPrimary, s_defaultLight.foregroundColor);
                    s_defaultLight.SetColorToken(Tokens.PrimaryColor, new Color(0.4f, 0.6f, 0.9f, 1.0f));

                    Color hover = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    hover.v = Math.Min(1.0f, hover.v + 0.1f);
                    s_defaultLight.SetColorToken(Tokens.ButtonHovered, hover);

                    Color active = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    active.v = Math.Max(0.0f, active.v - 0.1f);
                    s_defaultLight.SetColorToken(Tokens.ButtonActive, active);

                    // Semantic
                    s_defaultLight.successColor = new Color(0.1f, 0.6f, 0.1f, 1.0f);
                    s_defaultLight.warningColor = new Color(0.7f, 0.7f, 0.1f, 1.0f);
                    s_defaultLight.dangerColor = new Color(0.7f, 0.1f, 0.1f, 1.0f);
                    s_defaultLight.hintColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

                    s_defaultLight.successTextColor = new Color(0.05f, 0.5f, 0.05f, 1.0f);
                    s_defaultLight.warningTextColor = new Color(0.6f, 0.6f, 0.05f, 1.0f);
                    s_defaultLight.dangerTextColor = new Color(0.6f, 0.05f, 0.05f, 1.0f);
                    s_defaultLight.hintTextColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);

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

                    // Layout
                    s_defaultLight.labelWidth = 120.0f;
                    s_defaultLight.edgePadding = 10.0f;
                }
                return s_defaultLight;
            }
        }
    }
}
