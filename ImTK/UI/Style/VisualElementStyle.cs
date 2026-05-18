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
            RemoveEntry((int)key);

            if (value.IsNull) return;

            var prop = new StyleProperty { key = (int)key, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.ColorValue };
            if (value.IsToken) prop.tokenHash = value.Token.Hash;
            else prop.colorValue = value.Value.u32;

            m_overrideStyles.Add(prop);
        }

        public void SetFloat(ImTKStyleKey key, StyleValue<float> value)
        {
            EnsureOverrideStyles();
            RemoveEntry((int)key);

            if (value.IsNull) return;

            var prop = new StyleProperty { key = (int)key, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.FloatValue };
            if (value.IsToken) prop.tokenHash = value.Token.Hash;
            else prop.floatValue = value.Value;

            m_overrideStyles.Add(prop);
        }

        public void SetVector2(ImTKStyleKey key, StyleValue<Vector2> value)
        {
            EnsureOverrideStyles();
            RemoveEntry((int)key);

            if (value.IsNull) return;

            var prop = new StyleProperty { key = (int)key, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.Vector2Value };
            if (value.IsToken) prop.tokenHash = value.Token.Hash;
            else prop.vector2Value = value.Value;

            m_overrideStyles.Add(prop);
        }

        // --- Low-level Override Clearers ---

        public void Clear(ImTKStyleKey key)
        {
            RemoveEntry((int)key);
        }

        // --- High-level Property Syntax Sugar ---

        public StyleValue<Color>? backgroundColor
        {
            get => GetOverrideColor(ImTKStyleKey.BackgroundColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.BackgroundColor, value.Value);
                else Clear(ImTKStyleKey.BackgroundColor);
            }
        }

        public StyleValue<Color>? textColor
        {
            get => GetOverrideColor(ImTKStyleKey.TextColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.TextColor, value.Value);
                else Clear(ImTKStyleKey.TextColor);
            }
        }

        public StyleValue<Color>? disabledTextColor
        {
            get => GetOverrideColor(ImTKStyleKey.DisabledTextColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.DisabledTextColor, value.Value);
                else Clear(ImTKStyleKey.DisabledTextColor);
            }
        }

        public StyleValue<Color>? hoverColor
        {
            get => GetOverrideColor(ImTKStyleKey.HoverColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.HoverColor, value.Value);
                else Clear(ImTKStyleKey.HoverColor);
            }
        }

        public StyleValue<Color>? activeColor
        {
            get => GetOverrideColor(ImTKStyleKey.ActiveColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.ActiveColor, value.Value);
                else Clear(ImTKStyleKey.ActiveColor);
            }
        }

        public StyleValue<Color>? borderColor
        {
            get => GetOverrideColor(ImTKStyleKey.BorderColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.BorderColor, value.Value);
                else Clear(ImTKStyleKey.BorderColor);
            }
        }

        public StyleValue<Color>? checkMarkColor
        {
            get => GetOverrideColor(ImTKStyleKey.CheckMarkColor);
            set
            {
                if (value.HasValue) SetColor(ImTKStyleKey.CheckMarkColor, value.Value);
                else Clear(ImTKStyleKey.CheckMarkColor);
            }
        }

        public StyleValue<Vector2>? padding
        {
            get => GetOverrideVector2(ImTKStyleKey.Padding);
            set
            {
                if (value.HasValue) SetVector2(ImTKStyleKey.Padding, value.Value);
                else Clear(ImTKStyleKey.Padding);
            }
        }

        public StyleValue<Vector2>? itemSpacing
        {
            get => GetOverrideVector2(ImTKStyleKey.ItemSpacing);
            set
            {
                if (value.HasValue) SetVector2(ImTKStyleKey.ItemSpacing, value.Value);
                else Clear(ImTKStyleKey.ItemSpacing);
            }
        }

        public StyleValue<Vector2>? itemInnerSpacing
        {
            get => GetOverrideVector2(ImTKStyleKey.ItemInnerSpacing);
            set
            {
                if (value.HasValue) SetVector2(ImTKStyleKey.ItemInnerSpacing, value.Value);
                else Clear(ImTKStyleKey.ItemInnerSpacing);
            }
        }

        public StyleValue<float>? borderWidth
        {
            get => GetOverrideFloat(ImTKStyleKey.BorderWidth);
            set
            {
                if (value.HasValue) SetFloat(ImTKStyleKey.BorderWidth, value.Value);
                else Clear(ImTKStyleKey.BorderWidth);
            }
        }

        public StyleValue<float>? borderRadius
        {
            get => GetOverrideFloat(ImTKStyleKey.BorderRadius);
            set
            {
                if (value.HasValue) SetFloat(ImTKStyleKey.BorderRadius, value.Value);
                else Clear(ImTKStyleKey.BorderRadius);
            }
        }

        public StyleValue<float>? alpha
        {
            get => GetOverrideFloat(ImTKStyleKey.Alpha);
            set
            {
                if (value.HasValue) SetFloat(ImTKStyleKey.Alpha, value.Value);
                else Clear(ImTKStyleKey.Alpha);
            }
        }

        public StyleValue<float>? disabledAlpha
        {
            get => GetOverrideFloat(ImTKStyleKey.DisabledAlpha);
            set
            {
                if (value.HasValue) SetFloat(ImTKStyleKey.DisabledAlpha, value.Value);
                else Clear(ImTKStyleKey.DisabledAlpha);
            }
        }

        // --- Internal Helpers ---

        private void EnsureOverrideStyles()
        {
            if (m_overrideStyles == null) m_overrideStyles = new List<StyleProperty>();
        }

        private void RemoveEntry(int key)
        {
            if (m_overrideStyles == null) return;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == key)
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
                if (m_overrideStyles[i].key == (int)key)
                {
                    if (m_overrideStyles[i].isToken)
                        return new StyleValue<Color> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<Color> { Value = (Color)m_overrideStyles[i].colorValue };
                }
            }
            return null;
        }

        private StyleValue<float>? GetOverrideFloat(ImTKStyleKey key)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == (int)key)
                {
                    if (m_overrideStyles[i].isToken)
                        return new StyleValue<float> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<float> { Value = m_overrideStyles[i].floatValue };
                }
            }
            return null;
        }

        private StyleValue<Vector2>? GetOverrideVector2(ImTKStyleKey key)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == (int)key)
                {
                    if (m_overrideStyles[i].isToken)
                        return new StyleValue<Vector2> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<Vector2> { Value = m_overrideStyles[i].vector2Value };
                }
            }
            return null;
        }
    }
}
