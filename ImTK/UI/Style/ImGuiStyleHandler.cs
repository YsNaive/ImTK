using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class ImGuiStyleHandler
    {
        // 這兩個是 ImGui 層級的 internal key（小寫），與 VisualElement.StyleKey 的 "FontFamily"/"FontSize" 不同。
        // 使用 HashedString 快取 hash，避免每次使用裸字串 GetHashCode()。
        internal static readonly HashedString s_fontFamilyImGuiKey = new HashedString("fontFamily");
        internal static readonly HashedString s_fontSizeImGuiKey   = new HashedString("fontSize");

        private readonly StyleProperty[] m_colors = new StyleProperty[(int)ImGuiCol.COUNT];
        private readonly StyleProperty[] m_vars = new StyleProperty[(int)ImGuiStyleVar.COUNT];
        private StyleProperty m_fontFamily;
        private StyleProperty m_fontSize;

        private readonly List<int> m_activeColors = new List<int>();
        private readonly List<int> m_activeVars = new List<int>();
        private bool m_hasFontFamily = false;
        private bool m_hasFontSize = false;
        private bool m_fontWasPushed = false;

        public void Clear()
        {
            m_activeColors.Clear();
            m_activeVars.Clear();
            m_hasFontFamily = false;
            m_hasFontSize = false;
            m_fontWasPushed = false;
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
            else if (prop.dataType == StyleDataType.HashedString && prop.key == s_fontFamilyImGuiKey.Hash)
            {
                m_fontFamily = prop;
                m_hasFontFamily = true;
                return true;
            }
            else if (prop.dataType == StyleDataType.Float && prop.key == s_fontSizeImGuiKey.Hash)
            {
                m_fontSize = prop;
                m_hasFontSize = true;
                return true;
            }
            else if (prop.dataType == StyleDataType.Float || prop.dataType == StyleDataType.Vector2 || prop.dataType == StyleDataType.Thickness)
            {
                if (prop.key < 0 || prop.key >= (int)ImGuiStyleVar.COUNT) return false;
                m_vars[prop.key] = prop;
                if (!m_activeVars.Contains(prop.key)) m_activeVars.Add(prop.key);
                return true;
            }

            return false;
        }

        public bool TryGetVector2(int varIdx, out Vector2 value)
        {
            if (m_activeVars.Contains(varIdx))
            {
                value = m_vars[varIdx].vector2Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool HasColor(int colIdx)
        {
            return m_activeColors.Contains(colIdx);
        }

        public bool TryGetFloat(int varIdx, out float value)
        {
            if (m_activeVars.Contains(varIdx))
            {
                value = m_vars[varIdx].floatValue;
                return true;
            }
            value = default;
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
                    else if (curProp.dataType == StyleDataType.Thickness && curProp.thicknessValue != pProp.thicknessValue)
                        output.TrySetProperty(curProp);
                }
            }

            if (parent != null)
            {
                // For non-inheritable properties pushed by parent, we MUST override them with default values
                // if 'current' did not explicitly set them, because ImGui's stack leaks downward implicitly.
                foreach (var varIdx in parent.m_activeVars)
                {
                    var pProp = parent.m_vars[varIdx];
                    if (!pProp.isInheritable && !current.m_activeVars.Contains(varIdx))
                    {
                        var revertProp = pProp;
                        revertProp.dataType = pProp.dataType;
                        GetDefaultStyleVar(varIdx, out revertProp.floatValue, out revertProp.vector2Value);
                        output.TrySetProperty(revertProp);
                    }
                }

                foreach (var colIdx in parent.m_activeColors)
                {
                    var pProp = parent.m_colors[colIdx];
                    if (!pProp.isInheritable && !current.m_activeColors.Contains(colIdx))
                    {
                        var revertProp = pProp;
                        revertProp.colorValue = ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[colIdx]);
                        output.TrySetProperty(revertProp);
                    }
                }
            }

            bool fontChanged = false;

            if (current.m_hasFontFamily)
            {
                if (parent == null || !parent.m_hasFontFamily || parent.m_fontFamily.tokenHash != current.m_fontFamily.tokenHash)
                    fontChanged = true;
            }
            
            if (current.m_hasFontSize)
            {
                if (parent == null || !parent.m_hasFontSize || parent.m_fontSize.floatValue != current.m_fontSize.floatValue)
                    fontChanged = true;
            }

            if (fontChanged)
            {
                if (current.m_hasFontFamily) output.TrySetProperty(current.m_fontFamily);
                if (current.m_hasFontSize) output.TrySetProperty(current.m_fontSize);
            }
        }

        private static unsafe void GetDefaultStyleVar(int varIdx, out float fVal, out Vector2 vVal)
        {
            fVal = 0;
            vVal = System.Numerics.Vector2.Zero;
            ImGuiStylePtr style = ImGui.GetStyle();
            switch ((ImGuiStyleVar)varIdx)
            {
                case ImGuiStyleVar.Alpha: fVal = style.Alpha; break;
                case ImGuiStyleVar.DisabledAlpha: fVal = style.DisabledAlpha; break;
                case ImGuiStyleVar.WindowPadding: vVal = style.WindowPadding; break;
                case ImGuiStyleVar.WindowRounding: fVal = style.WindowRounding; break;
                case ImGuiStyleVar.WindowBorderSize: fVal = style.WindowBorderSize; break;
                case ImGuiStyleVar.WindowMinSize: vVal = style.WindowMinSize; break;
                case ImGuiStyleVar.WindowTitleAlign: vVal = style.WindowTitleAlign; break;
                case ImGuiStyleVar.ChildRounding: fVal = style.ChildRounding; break;
                case ImGuiStyleVar.ChildBorderSize: fVal = style.ChildBorderSize; break;
                case ImGuiStyleVar.PopupRounding: fVal = style.PopupRounding; break;
                case ImGuiStyleVar.PopupBorderSize: fVal = style.PopupBorderSize; break;
                case ImGuiStyleVar.FramePadding: vVal = style.FramePadding; break;
                case ImGuiStyleVar.FrameRounding: fVal = style.FrameRounding; break;
                case ImGuiStyleVar.FrameBorderSize: fVal = style.FrameBorderSize; break;
                case ImGuiStyleVar.ItemSpacing: vVal = style.ItemSpacing; break;
                case ImGuiStyleVar.ItemInnerSpacing: vVal = style.ItemInnerSpacing; break;
                case ImGuiStyleVar.IndentSpacing: fVal = style.IndentSpacing; break;
                case ImGuiStyleVar.CellPadding: vVal = style.CellPadding; break;
                case ImGuiStyleVar.ScrollbarSize: fVal = style.ScrollbarSize; break;
                case ImGuiStyleVar.ScrollbarRounding: fVal = style.ScrollbarRounding; break;
                case ImGuiStyleVar.GrabMinSize: fVal = style.GrabMinSize; break;
                case ImGuiStyleVar.GrabRounding: fVal = style.GrabRounding; break;
                case ImGuiStyleVar.TabRounding: fVal = style.TabRounding; break;
                case ImGuiStyleVar.ButtonTextAlign: vVal = style.ButtonTextAlign; break;
                case ImGuiStyleVar.SelectableTextAlign: vVal = style.SelectableTextAlign; break;
            }
        }

        public unsafe void PushFontOnly()
        {
            if (m_hasFontFamily || m_hasFontSize)
            {
                int familyHash = m_hasFontFamily ? m_fontFamily.tokenHash : RenderingContext.CurrentFontFamilyHash;
                var fontSize = m_hasFontSize ? (ImTK.UI.FontSize)m_fontSize.floatValue : ImTK.UI.FontSize.Normal;

                if (m_hasFontFamily)
                {
                    RenderingContext.PushFontState(familyHash);
                }

                var fontPtr = ImTKFontManager.GetFont(familyHash, fontSize);
                m_fontWasPushed = fontPtr.NativePtr != null;
                if (m_fontWasPushed)
                {
                    ImGui.PushFont(fontPtr);
                }
            }
        }

        public uint GetLayoutHash()
        {
            unchecked
            {
                uint hash = 17;
                foreach (var varIdx in m_activeVars)
                {
                    var prop = m_vars[varIdx];
                    if ((prop.flags & StyleFlags.LayoutAffecting) != 0)
                    {
                        hash = hash * 23 + (uint)varIdx;
                        hash = hash * 23 + (uint)prop.dataType;
                        if (prop.dataType == StyleDataType.Float) hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.floatValue);
                        else if (prop.dataType == StyleDataType.Vector2) {
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.vector2Value.X);
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.vector2Value.Y);
                        }
                        else if (prop.dataType == StyleDataType.Thickness) {
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.thicknessValue.left);
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.thicknessValue.top);
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.thicknessValue.right);
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.thicknessValue.bottom);
                        }
                    }
                }
                if (m_hasFontFamily) hash = hash * 23 + (uint)m_fontFamily.tokenHash;
                if (m_hasFontSize) hash = hash * 23 + BitConverter.SingleToUInt32Bits(m_fontSize.floatValue);
                return hash;
            }
        }

        public void PopFontOnly()
        {
            if (m_hasFontFamily || m_hasFontSize)
            {
                if (m_fontWasPushed) ImGui.PopFont();
                if (m_hasFontFamily) RenderingContext.PopFontState();
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
                else if (prop.dataType == StyleDataType.Thickness) ImGui.PushStyleVar((ImGuiStyleVar)varIdx, new Vector2(prop.thicknessValue.left, prop.thicknessValue.top));
            }

            if (m_hasFontFamily || m_hasFontSize)
            {
                int familyHash = m_hasFontFamily ? m_fontFamily.tokenHash : RenderingContext.CurrentFontFamilyHash;
                var fontSize = m_hasFontSize ? (ImTK.UI.FontSize)m_fontSize.floatValue : ImTK.UI.FontSize.Normal;

                if (m_hasFontFamily)
                {
                    RenderingContext.PushFontState(familyHash);
                }

                var fontPtr = ImTKFontManager.GetFont(familyHash, fontSize);
                m_fontWasPushed = fontPtr.NativePtr != null;
                if (m_fontWasPushed)
                {
                    ImGui.PushFont(fontPtr);
                }
            }
        }

        public void Pop()
        {
            if (m_hasFontFamily || m_hasFontSize)
            {
                if (m_fontWasPushed) ImGui.PopFont();
                if (m_hasFontFamily) RenderingContext.PopFontState();
            }

            if (m_activeVars.Count > 0) ImGui.PopStyleVar(m_activeVars.Count);
            if (m_activeColors.Count > 0) ImGui.PopStyleColor(m_activeColors.Count);
        }
    }
}
