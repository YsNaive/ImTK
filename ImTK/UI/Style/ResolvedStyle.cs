using System;
using System.Collections.Generic;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public class ResolvedStyle
    {
        private VisualElement m_element;
        private List<StyleProperty> m_properties;

        public ResolvedStyle(VisualElement element)
        {
            m_element = element;
            m_properties = new List<StyleProperty>();
        }

        private void SetOrUpdateProperty(StyleProperty newProp)
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i].key == newProp.key)
                {
                    m_properties[i] = newProp;
                    return;
                }
            }
            m_properties.Add(newProp);
        }

        private bool TryGetProperty(int key, out StyleProperty property)
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i].key == key)
                {
                    property = m_properties[i];
                    return true;
                }
            }
            property = default;
            return false;
        }

        public void Compute()
        {
            m_properties.Clear();

            // 1. Theme Fallback (handled at retrieval time now)

            // 2. Global Style Sheet
            ApplyStyleSheet(StyleSheet.Global);

            // 3. Local Ancestor Style Sheets (from root to element)
            ApplyAncestorStyleSheets(m_element);

            // 4. Inline Style Overrides
            if (m_element.internalStyle is VisualElement.Style styleBase && styleBase.m_overrideStyles != null)
            {
                foreach (var prop in styleBase.m_overrideStyles)
                {
                    SetOrUpdateProperty(prop);
                }
            }
        }

        private void ApplyAncestorStyleSheets(VisualElement current)
        {
            if (current == null) return;
            ApplyAncestorStyleSheets(current.parent);

            if (current.localStyleSheet != null)
            {
                ApplyStyleSheet(current.localStyleSheet);
            }
        }

        private void ApplyStyleSheet(StyleSheet sheet)
        {
            if (sheet == null || sheet.Blocks == null) return;

            foreach (var block in sheet.Blocks)
            {
                if (m_element.classList.Has(block.ClassName))
                {
                    if (block.Properties != null)
                    {
                        foreach (var prop in block.Properties)
                        {
                            SetOrUpdateProperty(prop);
                        }
                    }
                }
            }
        }

        private static readonly ImTK.Log.LogContext s_log = new ImTK.Log.LogContext("StyleSystem");

        public Color? GetColor(HashedString key)
        {
            if (TryGetProperty(key.Hash, out var prop))
            {
                if (prop.type == StylePropertyType.ColorValue)
                    return (Color)prop.colorValue;
                else if (prop.type == StylePropertyType.Token && m_element.theme != null)
                {
                    if (m_element.theme.TryGetColorToken(prop.tokenHash, out var color)) return color;
                    s_log.Warning($"Theme token '{prop.tokenHash}' not found, falling back to Magenta.");
                    return Color.Magenta;
                }
            }
            return null;
        }

        public float? GetFloat(HashedString key)
        {
             if (TryGetProperty(key.Hash, out var prop))
            {
                if (prop.type == StylePropertyType.FloatValue)
                    return prop.floatValue;
                else if (prop.type == StylePropertyType.Token && m_element.theme != null)
                {
                    if (m_element.theme.TryGetFloatToken(prop.tokenHash, out var val)) return val;
                }
            }
            return null;
        }

        public System.Numerics.Vector2? GetVector2(HashedString key)
        {
             if (TryGetProperty(key.Hash, out var prop))
            {
                if (prop.type == StylePropertyType.Vector2Value)
                    return prop.vector2Value;
                else if (prop.type == StylePropertyType.Token && m_element.theme != null)
                {
                    if (m_element.theme.TryGetVector2Token(prop.tokenHash, out var val)) return val;
                }
            }
            return null;
        }

        public int? GetInt(HashedString key)
        {
             if (TryGetProperty(key.Hash, out var prop))
            {
                if (prop.type == StylePropertyType.IntValue)
                    return prop.intValue;
                // Currently Int doesn't have token fallback from theme in ImTKTheme, but reserved for future
            }
            return null;
        }

        public int? GetTokenHash(HashedString key)
        {
             if (TryGetProperty(key.Hash, out var prop))
            {
                if (prop.type == StylePropertyType.Token)
                {
                    return prop.tokenHash;
                }
            }
            return null;
        }
    }
}
