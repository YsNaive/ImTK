using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Numerics;
using ImGuiNET;

namespace ImTK.UI
{
    public enum StyleVarType
    {
        Color,
        Float,
        Vector2
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct StyleEntry
    {
        [FieldOffset(0)] public StyleVarType Type;
        [FieldOffset(4)] public int Key; // ImGuiCol or ImGuiStyleVar

        [FieldOffset(8)] public uint ColorValue;
        [FieldOffset(8)] public float FloatValue;
        [FieldOffset(8)] public Vector2 Vector2Value;
    }

    public class VisualElementStyle
    {
        internal List<StyleEntry> m_themeStyles;
        internal List<StyleEntry> m_overrideStyles;

        public VisualElementStyle() { }

        // --- Low-level Override Setters ---

        public void SetColor(ImGuiCol col, Color color)
        {
            EnsureOverrideStyles();
            SetEntry(m_overrideStyles, new StyleEntry { Type = StyleVarType.Color, Key = (int)col, ColorValue = color.u32 });
        }

        public void SetVar(ImGuiStyleVar styleVar, float value)
        {
            EnsureOverrideStyles();
            SetEntry(m_overrideStyles, new StyleEntry { Type = StyleVarType.Float, Key = (int)styleVar, FloatValue = value });
        }

        public void SetVar(ImGuiStyleVar styleVar, Vector2 value)
        {
            EnsureOverrideStyles();
            SetEntry(m_overrideStyles, new StyleEntry { Type = StyleVarType.Vector2, Key = (int)styleVar, Vector2Value = value });
        }

        // --- Low-level Override Clearers ---

        public void ClearColor(ImGuiCol col)
        {
            RemoveEntry(m_overrideStyles, StyleVarType.Color, (int)col);
        }

        public void ClearVar(ImGuiStyleVar styleVar)
        {
            RemoveEntry(m_overrideStyles, StyleVarType.Float, (int)styleVar);
            RemoveEntry(m_overrideStyles, StyleVarType.Vector2, (int)styleVar);
        }

        // --- Low-level Theme Setters (internal) ---

        internal void ApplyThemeColor(ImGuiCol col, Color color)
        {
            EnsureThemeStyles();
            SetEntry(m_themeStyles, new StyleEntry { Type = StyleVarType.Color, Key = (int)col, ColorValue = color.u32 });
        }

        internal void ApplyThemeVar(ImGuiStyleVar styleVar, float value)
        {
            EnsureThemeStyles();
            SetEntry(m_themeStyles, new StyleEntry { Type = StyleVarType.Float, Key = (int)styleVar, FloatValue = value });
        }

        internal void ApplyThemeVar(ImGuiStyleVar styleVar, Vector2 value)
        {
            EnsureThemeStyles();
            SetEntry(m_themeStyles, new StyleEntry { Type = StyleVarType.Vector2, Key = (int)styleVar, Vector2Value = value });
        }

        internal void ClearThemeStyles()
        {
            if (m_themeStyles != null)
            {
                m_themeStyles.Clear();
            }
        }

        // --- High-level Property Syntax Sugar ---

        public Color? textColor
        {
            get => GetOverrideColor(ImGuiCol.Text);
            set
            {
                if (value.HasValue) SetColor(ImGuiCol.Text, value.Value);
                else ClearColor(ImGuiCol.Text);
            }
        }

        public Color? backgroundColor
        {
            get => GetOverrideColor(ImGuiCol.WindowBg);
            set
            {
                if (value.HasValue) SetColor(ImGuiCol.WindowBg, value.Value);
                else ClearColor(ImGuiCol.WindowBg);
            }
        }

        // --- Internal Helpers ---

        private void EnsureOverrideStyles()
        {
            if (m_overrideStyles == null) m_overrideStyles = new List<StyleEntry>();
        }

        private void EnsureThemeStyles()
        {
            if (m_themeStyles == null) m_themeStyles = new List<StyleEntry>();
        }

        private void SetEntry(List<StyleEntry> list, StyleEntry newEntry)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == newEntry.Type && list[i].Key == newEntry.Key)
                {
                    list[i] = newEntry;
                    return;
                }
            }
            list.Add(newEntry);
        }

        private void RemoveEntry(List<StyleEntry> list, StyleVarType type, int key)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == type && list[i].Key == key)
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        private Color? GetOverrideColor(ImGuiCol col)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == StyleVarType.Color && m_overrideStyles[i].Key == (int)col)
                {
                    return (Color)m_overrideStyles[i].ColorValue;
                }
            }
            return null;
        }

        private float? GetOverrideVarFloat(ImGuiStyleVar styleVar)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == StyleVarType.Float && m_overrideStyles[i].Key == (int)styleVar)
                {
                    return m_overrideStyles[i].FloatValue;
                }
            }
            return null;
        }

        private Vector2? GetOverrideVarVector2(ImGuiStyleVar styleVar)
        {
            if (m_overrideStyles == null) return null;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].Type == StyleVarType.Vector2 && m_overrideStyles[i].Key == (int)styleVar)
                {
                    return m_overrideStyles[i].Vector2Value;
                }
            }
            return null;
        }
    }
}
