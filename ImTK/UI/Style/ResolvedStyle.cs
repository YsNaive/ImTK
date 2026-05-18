using System;
using System.Collections.Generic;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public class ResolvedStyle
    {
        private VisualElement m_element;
        private Dictionary<int, StyleProperty> m_properties;

        public ResolvedStyle(VisualElement element)
        {
            m_element = element;
            m_properties = new Dictionary<int, StyleProperty>();
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
                    m_properties[prop.key] = prop;
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
                            m_properties[prop.key] = prop;
                        }
                    }
                }
            }
        }

        private static readonly ImTK.Log.LogContext s_log = new ImTK.Log.LogContext("StyleSystem");

        public Color? GetColor(HashedString key)
        {
            if (m_properties.TryGetValue(key.Hash, out var prop))
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
             if (m_properties.TryGetValue(key.Hash, out var prop))
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
             if (m_properties.TryGetValue(key.Hash, out var prop))
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
    }
}
