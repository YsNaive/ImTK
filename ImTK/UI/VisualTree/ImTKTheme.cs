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
        private Color GetColor(string tokenName, Color fallback = default)
        {
            return TryGetColorToken(new HashedString(tokenName).Hash, out Color c) ? c : fallback;
        }
        private void SetColor(string tokenName, Color value)
        {
            SetColorToken(new HashedString(tokenName), value);
        }

        private float GetFloat(string tokenName, float fallback = 0f)
        {
            return TryGetFloatToken(new HashedString(tokenName).Hash, out float f) ? f : fallback;
        }
        private void SetFloat(string tokenName, float value)
        {
            SetFloatToken(new HashedString(tokenName), value);
        }

        // --- Base Colors ---
        public Color backgroundColor { get => GetColor("--background-color"); set => SetColor("--background-color", value); }
        public Color subBackgroundColor { get => GetColor("--sub-background-color"); set => SetColor("--sub-background-color", value); }
        public Color foregroundColor { get => GetColor("--foreground-color"); set => SetColor("--foreground-color", value); }
        public Color subForegroundColor { get => GetColor("--sub-foreground-color"); set => SetColor("--sub-foreground-color", value); }

        // --- Semantic Colors ---
        public Color successColor { get => GetColor("--success-color"); set => SetColor("--success-color", value); }
        public Color successTextColor { get => GetColor("--success-text-color"); set => SetColor("--success-text-color", value); }
        public Color warningColor { get => GetColor("--warning-color"); set => SetColor("--warning-color", value); }
        public Color warningTextColor { get => GetColor("--warning-text-color"); set => SetColor("--warning-text-color", value); }
        public Color dangerColor { get => GetColor("--danger-color"); set => SetColor("--danger-color", value); }
        public Color dangerTextColor { get => GetColor("--danger-text-color"); set => SetColor("--danger-text-color", value); }
        public Color hintColor { get => GetColor("--hint-color"); set => SetColor("--hint-color", value); }
        public Color hintTextColor { get => GetColor("--hint-text-color"); set => SetColor("--hint-text-color", value); }

        // --- Layout Tokens ---
        public float labelWidth { get => GetFloat("--label-width", 120f); set => SetFloat("--label-width", value); }
        public float edgePadding { get => GetFloat("--edge-padding", 10f); set => SetFloat("--edge-padding", value); }

        // --- Syntax Highlight Wrapper ---
        public class SyntaxTheme
        {
            private ImTKTheme _theme;
            public SyntaxTheme(ImTKTheme theme) { _theme = theme; }

            public Color funcColor { get => _theme.GetColor("--syntax-func-color"); set => _theme.SetColor("--syntax-func-color", value); }
            public Color argsColor { get => _theme.GetColor("--syntax-args-color"); set => _theme.SetColor("--syntax-args-color", value); }
            public Color typeColor { get => _theme.GetColor("--syntax-type-color"); set => _theme.SetColor("--syntax-type-color", value); }
            public Color valueTypeColor { get => _theme.GetColor("--syntax-value-type-color"); set => _theme.SetColor("--syntax-value-type-color", value); }
            public Color prefixColor { get => _theme.GetColor("--syntax-prefix-color"); set => _theme.SetColor("--syntax-prefix-color", value); }
            public Color stringColor { get => _theme.GetColor("--syntax-string-color"); set => _theme.SetColor("--syntax-string-color", value); }
            public Color numberColor { get => _theme.GetColor("--syntax-number-color"); set => _theme.SetColor("--syntax-number-color", value); }
            public Color controlColor { get => _theme.GetColor("--syntax-control-color"); set => _theme.SetColor("--syntax-control-color", value); }
            public Color commentsColor { get => _theme.GetColor("--syntax-comments-color"); set => _theme.SetColor("--syntax-comments-color", value); }
            public Color codeBackgroundColor { get => _theme.GetColor("--syntax-code-bg-color"); set => _theme.SetColor("--syntax-code-bg-color", value); }
            public Color codeTextColor { get => _theme.GetColor("--syntax-code-text-color"); set => _theme.SetColor("--syntax-code-text-color", value); }
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
                    s_defaultDark.SetColorToken(new HashedString("--background-1"), s_defaultDark.backgroundColor);
                    s_defaultDark.SetColorToken(new HashedString("--background-2"), s_defaultDark.subBackgroundColor);
                    s_defaultDark.SetColorToken(new HashedString("--text-primary"), s_defaultDark.foregroundColor);
                    s_defaultDark.SetColorToken(new HashedString("--primary-color"), new Color(0.15f, 0.35f, 0.65f, 1.0f));

                    Color hover = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    hover.v = Math.Min(1.0f, hover.v + 0.1f);
                    s_defaultDark.SetColorToken(new HashedString("--button-hovered"), hover);

                    Color active = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    active.v = Math.Max(0.0f, active.v - 0.1f);
                    s_defaultDark.SetColorToken(new HashedString("--button-active"), active);

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
                    s_defaultLight.SetColorToken(new HashedString("--background-1"), s_defaultLight.backgroundColor);
                    s_defaultLight.SetColorToken(new HashedString("--background-2"), s_defaultLight.subBackgroundColor);
                    s_defaultLight.SetColorToken(new HashedString("--text-primary"), s_defaultLight.foregroundColor);
                    s_defaultLight.SetColorToken(new HashedString("--primary-color"), new Color(0.4f, 0.6f, 0.9f, 1.0f));

                    Color hover = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    hover.v = Math.Min(1.0f, hover.v + 0.1f);
                    s_defaultLight.SetColorToken(new HashedString("--button-hovered"), hover);

                    Color active = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    active.v = Math.Max(0.0f, active.v - 0.1f);
                    s_defaultLight.SetColorToken(new HashedString("--button-active"), active);

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
