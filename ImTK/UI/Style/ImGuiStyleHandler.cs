using System.Collections.Generic;
using ImGuiNET;

namespace ImTK.UI
{
    public class ImGuiStyleHandler
    {
        private readonly StyleProperty[] m_colors = new StyleProperty[(int)ImGuiCol.COUNT];
        private readonly StyleProperty[] m_vars = new StyleProperty[(int)ImGuiStyleVar.COUNT];

        private StyleProperty m_fontFamily;
        private StyleProperty m_fontSize;

        private readonly List<int> m_activeColors = new List<int>();
        private readonly List<int> m_activeVars = new List<int>();
        private bool m_hasFontFamily = false;
        private bool m_hasFontSize = false;

        public ImGuiStyleHandler()
        {
            Clear();
        }

        public void Clear()
        {
            m_activeColors.Clear();
            m_activeVars.Clear();
            m_hasFontFamily = false;
            m_hasFontSize = false;

            for (int i = 0; i < m_colors.Length; i++) m_colors[i].dataType = StyleDataType.Null;
            for (int i = 0; i < m_vars.Length; i++) m_vars[i].dataType = StyleDataType.Null;
            m_fontFamily.dataType = StyleDataType.Null;
            m_fontSize.dataType = StyleDataType.Null;
        }

        public bool TrySetProperty(StyleProperty prop)
        {
            if (prop.category != StyleCategory.ImGuiStyle) return false;

            if (prop.dataType == StyleDataType.Color)
            {
                if (prop.key < 0 || prop.key >= (int)ImGuiCol.COUNT) return false;
                m_colors[prop.key] = prop;
                if (!m_activeColors.Contains(prop.key)) m_activeColors.Add(prop.key);
                return true;
            }
            else if (prop.dataType == StyleDataType.Float || prop.dataType == StyleDataType.Vector2)
            {
                if (prop.key < 0 || prop.key >= (int)ImGuiStyleVar.COUNT) return false;
                m_vars[prop.key] = prop;
                if (!m_activeVars.Contains(prop.key)) m_activeVars.Add(prop.key);
                return true;
            }
            else if (prop.dataType == StyleDataType.HashedString && prop.key == "fontFamily".GetHashCode())
            {
                m_fontFamily = prop;
                m_hasFontFamily = true;
                return true;
            }
            else if (prop.dataType == StyleDataType.Float && prop.key == "fontSize".GetHashCode())
            {
                m_fontSize = prop;
                m_hasFontSize = true;
                return true;
            }

            return false;
        }

        public IEnumerable<StyleProperty> GetActiveProperties()
        {
            foreach (var colIdx in m_activeColors) yield return m_colors[colIdx];
            foreach (var varIdx in m_activeVars) yield return m_vars[varIdx];
            if (m_hasFontFamily) yield return m_fontFamily;
            if (m_hasFontSize) yield return m_fontSize;
        }

        public void CopyFrom(ImGuiStyleHandler parent)
        {
            if (parent == null) return;
            foreach (var prop in parent.GetActiveProperties())
            {
                if (prop.isInheritable) TrySetProperty(prop);
            }
        }

        public static void Diff(ImGuiStyleHandler parent, ImGuiStyleHandler current, ImGuiStyleHandler output)
        {
            output.Clear();

            // All properties present in 'current' that differ from 'parent' must be pushed.
            foreach (var colIdx in current.m_activeColors)
            {
                var curProp = current.m_colors[colIdx];
                if (parent == null || !parent.m_activeColors.Contains(colIdx) || parent.m_colors[colIdx].colorValue != curProp.colorValue)
                {
                    output.TrySetProperty(curProp);
                }
            }

            foreach (var varIdx in current.m_activeVars)
            {
                var curProp = current.m_vars[varIdx];
                if (parent == null || !parent.m_activeVars.Contains(varIdx))
                {
                    output.TrySetProperty(curProp);
                }
                else
                {
                    var pProp = parent.m_vars[varIdx];
                    if (curProp.dataType == StyleDataType.Float && curProp.floatValue != pProp.floatValue)
                        output.TrySetProperty(curProp);
                    else if (curProp.dataType == StyleDataType.Vector2 && curProp.vector2Value != pProp.vector2Value)
                        output.TrySetProperty(curProp);
                }
            }

            // To handle non-inheritable properties: if the parent pushed them, but current didn't explicitly override them (thus dropping them in CopyFrom),
            // current needs to explicitly PUSH the global default value to prevent ImGui's stack from bleeding them downwards.
            // Since we can't easily query ImGui's default value for everything here dynamically, a robust approach is to ensure
            // non-inheritable styles are always completely specified if an element cares about them,
            // OR we ensure RenderEngine applies GlobalTheme defaults as a baseline.
            // For now, the existing Diff correctly isolates the delta of what 'current' explicitly holds vs 'parent'.

            if (current.m_hasFontFamily)
            {
                if (parent == null || !parent.m_hasFontFamily || parent.m_fontFamily.tokenHash != current.m_fontFamily.tokenHash)
                    output.TrySetProperty(current.m_fontFamily);
            }
            if (current.m_hasFontSize)
            {
                if (parent == null || !parent.m_hasFontSize || parent.m_fontSize.floatValue != current.m_fontSize.floatValue)
                    output.TrySetProperty(current.m_fontSize);
            }
        }

        public unsafe void Push()
        {
            foreach (var colIdx in m_activeColors) ImGui.PushStyleColor((ImGuiCol)colIdx, m_colors[colIdx].colorValue);
            foreach (var varIdx in m_activeVars)
            {
                var prop = m_vars[varIdx];
                if (prop.dataType == StyleDataType.Float) ImGui.PushStyleVar((ImGuiStyleVar)varIdx, prop.floatValue);
                else if (prop.dataType == StyleDataType.Vector2) ImGui.PushStyleVar((ImGuiStyleVar)varIdx, prop.vector2Value);
            }
            if (m_hasFontFamily)
            {
                RenderingContext.PushFontState(m_fontFamily.tokenHash);
                var fontPtr = ImTKFontManager.GetFont(m_fontFamily.tokenHash, ImTK.UI.FontSize.Normal);
                if (fontPtr.NativePtr != null)
                {
                    ImGui.PushFont(fontPtr);
                }
            }
            if (m_hasFontSize) ImTKFontManager.PushFontScale(m_fontSize.floatValue);
        }

        public void Pop()
        {
            if (m_hasFontSize) ImTKFontManager.PopFontScale();
            if (m_hasFontFamily)
            {
                ImGui.PopFont();
                RenderingContext.PopFontState();
            }
            if (m_activeVars.Count > 0) ImGui.PopStyleVar(m_activeVars.Count);
            if (m_activeColors.Count > 0) ImGui.PopStyleColor(m_activeColors.Count);
        }

        public float? GetFloat(ImTK.Core.HashedString key, float fallback = 0f) { return fallback; }
        public float? GetFloat(ImTK.Core.HashedString key) { return null; }
        public Color? GetColor(ImTK.Core.HashedString key) { return null; }
    }
}
