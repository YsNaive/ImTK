using System.Collections.Generic;
using System.Numerics;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public class VisualElementStyle
    {
        internal List<StyleProperty> m_overrideStyles;

        public VisualElementStyle() { }

        // --- Low-level Override Setters ---

        public void SetColor(ImTKStyleKey key, StyleValue<Color> value)
        {
            EnsureOverrideStyles();
            RemoveEntry(StyleVarType.Color, (int)key);

            if (value.IsNull) return;

            var prop = new StyleProperty { Type = StyleVarType.Color, Key = (int)key };
            if (value.IsToken) prop.TokenHash = value.Token.Hash;
            else prop.ColorValue = value.Value.u32;

            m_overrideStyles.Add(prop);
        }

        public void SetFloat(ImTKStyleKey key, StyleValue<float> value)
        {
            EnsureOverrideStyles();
            RemoveEntry(StyleVarType.Float, (int)key);

            if (value.IsNull) return;

            var prop = new StyleProperty { Type = StyleVarType.Float, Key = (int)key };
            if (value.IsToken) prop.TokenHash = value.Token.Hash;
            else prop.FloatValue = value.Value;

            m_overrideStyles.Add(prop);
        }

        public void SetVector2(ImTKStyleKey key, StyleValue<Vector2> value)
        {
            EnsureOverrideStyles();
            RemoveEntry(StyleVarType.Vector2, (int)key);

            if (value.IsNull) return;

            var prop = new StyleProperty { Type = StyleVarType.Vector2, Key = (int)key };
            if (value.IsToken) prop.TokenHash = value.Token.Hash;
            else prop.Vector2Value = value.Value;

            m_overrideStyles.Add(prop);
        }

        // --- Low-level Override Clearers ---

        public void Clear(StyleVarType type, ImTKStyleKey key)
        {
            RemoveEntry(type, (int)key);
        }

        // --- High-level Property Syntax Sugar ---

        public StyleValue<Color>? backgroundColor
        {
            get => GetOverrideColor(ImTKStyleKey.BackgroundColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.BackgroundColor, value.Value);
                else Clear(StyleVarType.Color, ImTKStyleKey.BackgroundColor);
            }
        }

        public StyleValue<Color>? textColor
        {
            get => GetOverrideColor(ImTKStyleKey.TextColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.TextColor, value.Value);
                else Clear(StyleVarType.Color, ImTKStyleKey.TextColor);
            }
        }

        public StyleValue<Color>? hoverColor
        {
            get => GetOverrideColor(ImTKStyleKey.HoverColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.HoverColor, value.Value);
                else Clear(StyleVarType.Color, ImTKStyleKey.HoverColor);
            }
        }

        public StyleValue<Color>? activeColor
        {
            get => GetOverrideColor(ImTKStyleKey.ActiveColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.ActiveColor, value.Value);
                else Clear(StyleVarType.Color, ImTKStyleKey.ActiveColor);
            }
        }

        public StyleValue<Color>? borderColor
        {
            get => GetOverrideColor(ImTKStyleKey.BorderColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.BorderColor, value.Value);
                else Clear(StyleVarType.Color, ImTKStyleKey.BorderColor);
            }
        }

        public StyleValue<float>? borderRadius
        {
            get => GetOverrideFloat(ImTKStyleKey.BorderRadius);
            set
            {
                if (value.HasValue) SetFloat(ImTKStyleKey.BorderRadius, value.Value);
                else Clear(StyleVarType.Float, ImTKStyleKey.BorderRadius);
            }
        }

        // --- Internal Helpers ---

        private void EnsureOverrideStyles()
        {
            if (m_overrideStyles == null) m_overrideStyles = new List<StyleProperty>();
        }

        private void RemoveEntry(StyleVarType type, int key)
        {
            if (m_overrideStyles == null) return;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == type && m_overrideStyles[i].Key == key)
                {
                    m_overrideStyles.RemoveAt(i);
                    return;
                }
            }
        }

        private StyleValue<Color>? GetOverrideColor(ImTKStyleKey key)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == StyleVarType.Color && m_overrideStyles[i].Key == (int)key)
                {
                    if (m_overrideStyles[i].IsToken)
                        return new StyleValue<Color> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<Color> { Value = (Color)m_overrideStyles[i].ColorValue };
                }
            }
            return null;
        }

        private StyleValue<float>? GetOverrideFloat(ImTKStyleKey key)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == StyleVarType.Float && m_overrideStyles[i].Key == (int)key)
                {
                    if (m_overrideStyles[i].IsToken)
                        return new StyleValue<float> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<float> { Value = m_overrideStyles[i].FloatValue };
                }
            }
            return null;
        }

        private StyleValue<Vector2>? GetOverrideVector2(ImTKStyleKey key)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == StyleVarType.Vector2 && m_overrideStyles[i].Key == (int)key)
                {
                    if (m_overrideStyles[i].IsToken)
                        return new StyleValue<Vector2> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<Vector2> { Value = m_overrideStyles[i].Vector2Value };
                }
            }
            return null;
        }
    }
}
