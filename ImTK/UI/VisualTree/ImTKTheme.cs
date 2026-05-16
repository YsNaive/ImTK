using System;
using System.Numerics;
using ImGuiNET;

namespace ImTK.UI
{
    public class ImTKTheme
    {
        public static readonly int DrawerLabelWidthToken = new ImTK.Core.HashedString("--drawer-label-width").Hash;

        public ImTKTheme parent { get; set; }

        private System.Collections.Generic.Dictionary<int, Color> m_colorTokens = new System.Collections.Generic.Dictionary<int, Color>();
        private System.Collections.Generic.Dictionary<int, float> m_floatTokens = new System.Collections.Generic.Dictionary<int, float>();
        private System.Collections.Generic.Dictionary<int, Vector2> m_vector2Tokens = new System.Collections.Generic.Dictionary<int, Vector2>();

        public void SetColorToken(ImTK.Core.HashedString token, Color color)
        {
            m_colorTokens[token.Hash] = color;
        }

        public void SetFloatToken(ImTK.Core.HashedString token, float value)
        {
            m_floatTokens[token.Hash] = value;
        }

        public void SetVector2Token(ImTK.Core.HashedString token, Vector2 value)
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

        // --- Default Themes ---

        private static ImTKTheme s_defaultDark;
        public static ImTKTheme DefaultDark
        {
            get
            {
                if (s_defaultDark == null)
                {
                    s_defaultDark = new ImTKTheme();
                    s_defaultDark.SetColorToken("--background-1", new Color(0.06f, 0.06f, 0.06f, 1.0f));
                    s_defaultDark.SetColorToken("--background-2", new Color(0.11f, 0.11f, 0.11f, 1.0f));
                    s_defaultDark.SetColorToken("--text-primary", new Color(1.0f, 1.0f, 1.0f, 1.0f));
                    s_defaultDark.SetColorToken("--primary-color", new Color(0.15f, 0.35f, 0.65f, 1.0f));

                    Color hover = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    hover.v = Math.Min(1.0f, hover.v + 0.1f);
                    s_defaultDark.SetColorToken("--button-hovered", hover);

                    Color active = new Color(0.15f, 0.35f, 0.65f, 1.0f);
                    active.v = Math.Max(0.0f, active.v - 0.1f);
                    s_defaultDark.SetColorToken("--button-active", active);

                    s_defaultDark.SetFloatToken("--drawer-label-width", 150.0f);
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
                    s_defaultLight.SetColorToken("--background-1", new Color(0.9f, 0.9f, 0.9f, 1.0f));
                    s_defaultLight.SetColorToken("--background-2", new Color(0.8f, 0.8f, 0.8f, 1.0f));
                    s_defaultLight.SetColorToken("--text-primary", new Color(0.0f, 0.0f, 0.0f, 1.0f));
                    s_defaultLight.SetColorToken("--primary-color", new Color(0.4f, 0.6f, 0.9f, 1.0f));

                    Color hover = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    hover.v = Math.Min(1.0f, hover.v + 0.1f);
                    s_defaultLight.SetColorToken("--button-hovered", hover);

                    Color active = new Color(0.4f, 0.6f, 0.9f, 1.0f);
                    active.v = Math.Max(0.0f, active.v - 0.1f);
                    s_defaultLight.SetColorToken("--button-active", active);

                    s_defaultLight.SetFloatToken("--drawer-label-width", 150.0f);
                }
                return s_defaultLight;
            }
        }
    }
}
