using System;
using System.Collections.Generic;
using ImGuiNET;
using ImTK.Core;
using ImTK.Log;

namespace ImTK.UI.Style
{
    public class ResolvedStyle
    {
        private static readonly LogContext s_log = new LogContext("ResolvedStyle");

        private StyleProperty[] m_properties;
        private VisualElement m_owner;

        // Pushed counts stored to later pop ImGui correctly without recounting
        private int m_pushedColors = 0;
        private int m_pushedVars = 0;

        public ResolvedStyle(VisualElement owner)
        {
            m_owner = owner;
            m_properties = new StyleProperty[(int)ImTKStyleKey.MaxCount];
        }

        // Retrieve properties via indexer for ease of access
        public StyleProperty this[ImTKStyleKey key] => m_properties[(int)key];

        public Color backgroundColor => m_properties[(int)ImTKStyleKey.BackgroundColor].type == StylePropertyType.ColorValue ? (Color)m_properties[(int)ImTKStyleKey.BackgroundColor].colorValue : new Color(0,0,0,0);
        public Color textColor => m_properties[(int)ImTKStyleKey.TextColor].type == StylePropertyType.ColorValue ? (Color)m_properties[(int)ImTKStyleKey.TextColor].colorValue : Color.White;
        public System.Numerics.Vector2 padding => m_properties[(int)ImTKStyleKey.Padding].type == StylePropertyType.Vector2Value ? m_properties[(int)ImTKStyleKey.Padding].vector2Value : System.Numerics.Vector2.Zero;
        public System.Numerics.Vector2 itemSpacing => m_properties[(int)ImTKStyleKey.ItemSpacing].type == StylePropertyType.Vector2Value ? m_properties[(int)ImTKStyleKey.ItemSpacing].vector2Value : System.Numerics.Vector2.Zero;
        public float borderRadius => m_properties[(int)ImTKStyleKey.BorderRadius].type == StylePropertyType.FloatValue ? m_properties[(int)ImTKStyleKey.BorderRadius].floatValue : 0f;

        public void Compute()
        {
            // 1. Inherit from Parent
            if (m_owner.parent != null && m_owner.parent.resolvedStyle != null)
            {
                Array.Copy(m_owner.parent.resolvedStyle.m_properties, m_properties, m_properties.Length);
            }
            else
            {
                // Root node: start with nulls
                Array.Clear(m_properties, 0, m_properties.Length);
            }

            // 2. We don't traverse up looking for local sheets! We rely on the cascade from the parent.
            // But wait, our requirements for classes:
            // "We search from global down to local (so local overrides global).
            // Actually, if we apply our classes using the closest style sheets:
            // The cascade is: Inherited -> Global Classes -> Local Ancestor Classes -> Inline

            // Actually, to make it fully correct according to standard CSS cascading, the matched style rules for THIS element's classes
            // should be resolved.

            var classes = new List<HashedString>(m_owner.classList.GetClasses());

            if (classes.Count > 0)
            {
                // Global styles
                foreach (var cls in classes)
                {
                    if (StyleSheet.Global.TryGetBlock(cls, out var block))
                    {
                        ApplyBlock(block);
                    }
                }

                // Local styles from Ancestors
                // Note: instead of finding sheets every time, could we cache `ancestorSheets` on the element?
                // For now, let's keep it simple to ensure correctness. Traversing up is quite fast in C#.
                var ancestorSheets = new List<StyleSheet>();
                var current = m_owner;
                while (current != null)
                {
                    if (current.localStyleSheet != null)
                    {
                        ancestorSheets.Add(current.localStyleSheet);
                    }
                    current = current.parent;
                }

                for (int i = ancestorSheets.Count - 1; i >= 0; i--)
                {
                    foreach (var cls in classes)
                    {
                        if (ancestorSheets[i].TryGetBlock(cls, out var block))
                        {
                            ApplyBlock(block);
                        }
                    }
                }
            }

            // Inline styles
            if (m_owner.style.m_overrideStyles != null)
            {
                foreach (var prop in m_owner.style.m_overrideStyles)
                {
                    if (prop.isNull)
                    {
                        m_properties[prop.key].type = StylePropertyType.Null; // clears the cascade for this prop
                    }
                    else
                    {
                        m_properties[prop.key] = prop;
                    }
                }
            }

            // Resolve Tokens
            var activeTheme = m_owner.theme;
            for (int i = 0; i < m_properties.Length; i++)
            {
                ref var prop = ref m_properties[i];
                if (prop.isToken)
                {
                    if (i <= (int)ImTKStyleKey.CheckMarkColor)
                    {
                        if (activeTheme.TryGetColorToken(prop.tokenHash, out var color))
                        {
                            prop.type = StylePropertyType.ColorValue;
                            prop.colorValue = color.u32;
                        }
                        else
                        {
                            s_log.Warning($"Style System: Missing color token hash '{prop.tokenHash}'. Falling back to Magenta.");
                            prop.type = StylePropertyType.ColorValue;
                            prop.colorValue = Color.Magenta.u32;
                        }
                    }
                    else if (i == (int)ImTKStyleKey.BorderWidth || i == (int)ImTKStyleKey.BorderRadius || i == (int)ImTKStyleKey.Alpha || i == (int)ImTKStyleKey.DisabledAlpha)
                    {
                        if (activeTheme.TryGetFloatToken(prop.tokenHash, out var val))
                        {
                            prop.type = StylePropertyType.FloatValue;
                            prop.floatValue = val;
                        }
                        else
                        {
                            s_log.Warning($"Style System: Missing float token hash '{prop.tokenHash}'. Falling back to 0.");
                            prop.type = StylePropertyType.FloatValue;
                            prop.floatValue = 0f;
                        }
                    }
                    else if (i == (int)ImTKStyleKey.Padding || i == (int)ImTKStyleKey.ItemSpacing || i == (int)ImTKStyleKey.ItemInnerSpacing)
                    {
                        if (activeTheme.TryGetVector2Token(prop.tokenHash, out var val))
                        {
                            prop.type = StylePropertyType.Vector2Value;
                            prop.vector2Value = val;
                        }
                        else
                        {
                            s_log.Warning($"Style System: Missing Vector2 token hash '{prop.tokenHash}'. Falling back to Zero.");
                            prop.type = StylePropertyType.Vector2Value;
                            prop.vector2Value = System.Numerics.Vector2.Zero;
                        }
                    }
                }
            }
        }

        private void ApplyBlock(StyleBlock block)
        {
            foreach (var prop in block.Properties)
            {
                if (prop.isNull)
                {
                    m_properties[prop.key].type = StylePropertyType.Null;
                }
                else
                {
                    m_properties[prop.key] = prop;
                }
            }
        }

        public void PushToImGui()
        {
            m_pushedColors = 0;
            m_pushedVars = 0;

            var mapping = m_owner.styleMapping;
            var parentProps = m_owner.parent?.resolvedStyle?.m_properties;

            for (int i = 0; i < m_properties.Length; i++)
            {
                ref var myProp = ref m_properties[i];
                if (myProp.isNull) continue;

                // Diff with parent to avoid redundant pushes
                if (parentProps != null)
                {
                    ref var parentProp = ref parentProps[i];
                    if (!DiffersFrom(in myProp, in parentProp))
                    {
                        continue;
                    }
                }

                if (myProp.type == StylePropertyType.ColorValue)
                {
                    int imguiTarget = mapping.colorTargets[i];
                    if (imguiTarget != -1)
                    {
                        ImGui.PushStyleColor((ImGuiCol)imguiTarget, myProp.colorValue);
                        m_pushedColors++;
                    }
                }
                else if (myProp.type == StylePropertyType.FloatValue)
                {
                    int imguiTarget = mapping.floatTargets[i];
                    if (imguiTarget != -1)
                    {
                        ImGui.PushStyleVar((ImGuiStyleVar)imguiTarget, myProp.floatValue);
                        m_pushedVars++;
                    }
                }
                else if (myProp.type == StylePropertyType.Vector2Value)
                {
                    int imguiTarget = mapping.vector2Targets[i];
                    if (imguiTarget != -1)
                    {
                        ImGui.PushStyleVar((ImGuiStyleVar)imguiTarget, myProp.vector2Value);
                        m_pushedVars++;
                    }
                }
            }
        }

        public void PopFromImGui()
        {
            if (m_pushedColors > 0) ImGui.PopStyleColor(m_pushedColors);
            if (m_pushedVars > 0) ImGui.PopStyleVar(m_pushedVars);
        }

        private static bool DiffersFrom(in StyleProperty a, in StyleProperty b)
        {
            if (a.type != b.type) return true;
            if (a.type == StylePropertyType.Null) return false;

            if (a.type == StylePropertyType.Token) return a.tokenHash != b.tokenHash;
            if (a.type == StylePropertyType.ColorValue) return a.colorValue != b.colorValue;
            if (a.type == StylePropertyType.FloatValue) return a.floatValue != b.floatValue;
            return a.vector2Value != b.vector2Value;
        }
    }
}
