using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public class ImGuiStyleHandler
    {
        internal static readonly HashedString s_fontFamilyImGuiKey = new HashedString("fontFamily");
        internal static readonly HashedString s_fontSizeImGuiKey   = new HashedString("fontSize");

        private readonly List<StyleProperty> m_properties = new List<StyleProperty>();
        private bool m_fontWasPushed = false;
        private float m_currentDpiScale = 1.0f;

        public void Clear()
        {
            m_properties.Clear();
            m_fontWasPushed = false;
            m_currentDpiScale = 1.0f;
        }

        public void Scale(float scaleFactor)
        {
            m_currentDpiScale = scaleFactor;
            for (int i = 0; i < m_properties.Count; i++)
            {
                var prop = m_properties[i];
                if (prop.category == StyleCategory.ImGuiStyle)
                {
                    if (prop.dataType == StyleDataType.Float && prop.key < (int)ImGuiStyleVar.Count)
                    {
                        ImGuiStyleVar varIdx = (ImGuiStyleVar)prop.key;
                        if (varIdx != ImGuiStyleVar.Alpha && varIdx != ImGuiStyleVar.DisabledAlpha)
                        {
                            prop.floatValue *= scaleFactor;
                            m_properties[i] = prop;
                        }
                    }
                    else if (prop.dataType == StyleDataType.Vector2 && prop.key < (int)ImGuiStyleVar.Count)
                    {
                        prop.vector2Value *= scaleFactor;
                        m_properties[i] = prop;
                    }
                }
            }
        }

        public bool TrySetProperty(StyleProperty prop)
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i].key == prop.key && m_properties[i].category == prop.category)
                {
                    m_properties[i] = prop;
                    return true;
                }
            }
            m_properties.Add(prop);
            return true;
        }

        public bool TryGetVector2(int varIdx, out Vector2 value)
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i].category == StyleCategory.ImGuiStyle && m_properties[i].key == varIdx && m_properties[i].dataType == StyleDataType.Vector2)
                {
                    value = m_properties[i].vector2Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public bool HasColor(int colIdx)
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i].category == StyleCategory.ImGuiStyle && m_properties[i].key == colIdx && m_properties[i].dataType == StyleDataType.Color)
                    return true;
            }
            return false;
        }

        public bool TryGetFloat(int varIdx, out float value)
        {
            for (int i = 0; i < m_properties.Count; i++)
            {
                if (m_properties[i].category == StyleCategory.ImGuiStyle && m_properties[i].key == varIdx && m_properties[i].dataType == StyleDataType.Float)
                {
                    value = m_properties[i].floatValue;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public IReadOnlyList<StyleProperty> GetActiveProperties()
        {
            return m_properties;
        }

        public void CopyFrom(ImGuiStyleHandler parent)
        {
            if (parent == null) return;
            foreach (var prop in parent.m_properties)
            {
                if (prop.isInheritable) TrySetProperty(prop);
            }
        }

        private bool TryGetFontFamily(out StyleProperty prop) {
            for(int i=0; i<m_properties.Count; i++) {
                if (m_properties[i].category == StyleCategory.ImGuiStyle && m_properties[i].key == s_fontFamilyImGuiKey.Hash) { prop = m_properties[i]; return true; }
            }
            prop = default; return false;
        }

        private bool TryGetFontSize(out StyleProperty prop) {
            for(int i=0; i<m_properties.Count; i++) {
                if (m_properties[i].category == StyleCategory.ImGuiStyle && m_properties[i].key == s_fontSizeImGuiKey.Hash) { prop = m_properties[i]; return true; }
            }
            prop = default; return false;
        }

        public static void Diff(ImGuiStyleHandler parent, ImGuiStyleHandler current, ImGuiStyleHandler output)
        {
            output.Clear();
            output.m_currentDpiScale = current.m_currentDpiScale;

            // 1. For properties in current, if not in parent (or different), push.
            // We ignore HighLevelToken for Diff since they are not pushed to ImGui.
            foreach (var curProp in current.m_properties)
            {
                if (curProp.category != StyleCategory.ImGuiStyle) continue;
                if (curProp.key == s_fontFamilyImGuiKey.Hash || curProp.key == s_fontSizeImGuiKey.Hash) continue; // Handled separately

                bool foundMatch = false;
                if (parent != null)
                {
                    foreach (var pProp in parent.m_properties)
                    {
                        if (pProp.category == curProp.category && pProp.key == curProp.key)
                        {
                            if (curProp.dataType == StyleDataType.Color && curProp.colorValue == pProp.colorValue) foundMatch = true;
                            else if (curProp.dataType == StyleDataType.Float && curProp.floatValue == pProp.floatValue) foundMatch = true;
                            else if (curProp.dataType == StyleDataType.Vector2 && curProp.vector2Value == pProp.vector2Value) foundMatch = true;
                            else if (curProp.dataType == StyleDataType.HashedString && curProp.tokenHash == pProp.tokenHash) foundMatch = true;
                            else if (curProp.dataType == StyleDataType.Int && curProp.intValue == pProp.intValue) foundMatch = true;
                            else if (curProp.dataType == StyleDataType.Enum && curProp.enumValue == pProp.enumValue) foundMatch = true;
                            break;
                        }
                    }
                }
                
                if (!foundMatch)
                {
                    output.TrySetProperty(curProp);
                }
            }

            // 2. For properties in parent, if NOT INHERITABLE, and NOT IN CURRENT, we MUST revert to default!
            if (parent != null)
            {
                foreach (var pProp in parent.m_properties)
                {
                    if (pProp.category != StyleCategory.ImGuiStyle) continue;
                    if (pProp.key == s_fontFamilyImGuiKey.Hash || pProp.key == s_fontSizeImGuiKey.Hash) continue; // Font cannot be reverted individually this way
                    
                    if (!pProp.isInheritable)
                    {
                        bool overridenByCurrent = false;
                        foreach (var curProp in current.m_properties)
                        {
                            if (curProp.category == pProp.category && curProp.key == pProp.key)
                            {
                                overridenByCurrent = true;
                                break;
                            }
                        }

                        if (!overridenByCurrent)
                        {
                            if (pProp.dataType == StyleDataType.Color)
                            {
                                var revertProp = pProp;
                                revertProp.colorValue = ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[pProp.key]);
                                output.TrySetProperty(revertProp);
                            }
                            else if (pProp.dataType == StyleDataType.Float || pProp.dataType == StyleDataType.Vector2)
                            {
                                var revertProp = pProp;
                                GetDefaultStyleVar(pProp.key, out revertProp.floatValue, out revertProp.vector2Value);
                                output.TrySetProperty(revertProp);
                            }
                        }
                    }
                }
            }

            // 3. Fonts
            bool fontChanged = false;
            StyleProperty curFamily = default;
            bool curHasFamily = current.TryGetFontFamily(out curFamily);
            StyleProperty parFamily = default;
            bool parHasFamily = parent != null && parent.TryGetFontFamily(out parFamily);
            
            StyleProperty curSize = default;
            bool curHasSize = current.TryGetFontSize(out curSize);
            StyleProperty parSize = default;
            bool parHasSize = parent != null && parent.TryGetFontSize(out parSize);

            if (curHasFamily) {
                if (!parHasFamily || parFamily.tokenHash != curFamily.tokenHash) fontChanged = true;
            }
            if (curHasSize) {
                if (!parHasSize || curSize.dataType != parSize.dataType ||
                    (curSize.dataType == StyleDataType.Float && curSize.floatValue != parSize.floatValue) ||
                    (curSize.dataType == StyleDataType.Int && curSize.intValue != parSize.intValue) ||
                    (curSize.dataType == StyleDataType.Enum && curSize.enumValue != parSize.enumValue)) fontChanged = true;
            }

            if (fontChanged) {
                if (curHasFamily) output.TrySetProperty(curFamily);
                if (curHasSize) output.TrySetProperty(curSize);
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
            bool hasFamily = TryGetFontFamily(out var fontFamily);
            bool hasSize = TryGetFontSize(out var fontSize);

            if (hasFamily || hasSize)
            {
                int familyHash = hasFamily ? fontFamily.tokenHash : RenderEngine.Context.CurrentFontFamilyHash;
                
                if (hasFamily)
                {
                    RenderEngine.Context.PushFontState(familyHash);
                }

                ImFontPtr fontPtr = ImTKFontManager.GetFont(familyHash);
                float targetSize = ImTKTheme.GlobalTheme.fontSizeNormal;

                if (hasSize)
                {
                    if (fontSize.dataType == StyleDataType.Int)
                    {
                        targetSize = fontSize.intValue;
                    }
                    else if (fontSize.dataType == StyleDataType.Enum)
                    {
                        var fontSizeEnum = (ImTK.UI.FontSize)fontSize.enumValue;
                        targetSize = ImTKTheme.GlobalTheme.GetFontSizes()[fontSizeEnum];
                    }
                }

                targetSize *= m_currentDpiScale;

                m_fontWasPushed = fontPtr.Handle != null;
                if (m_fontWasPushed)
                {
                    ImGui.PushFont((Hexa.NET.ImGui.ImFont*)fontPtr.Handle, targetSize);
                }
            }
        }

        public uint GetLayoutHash()
        {
            unchecked
            {
                uint hash = 17;
                foreach (var prop in m_properties)
                {
                    if (prop.category != StyleCategory.ImGuiStyle) continue;

                    if ((prop.flags & StyleFlags.LayoutAffecting) != 0)
                    {
                        hash = hash * 23 + (uint)prop.key;
                        hash = hash * 23 + (uint)prop.dataType;
                        if (prop.dataType == StyleDataType.Float) hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.floatValue);
                        else if (prop.dataType == StyleDataType.Vector2) {
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.vector2Value.X);
                            hash = hash * 23 + BitConverter.SingleToUInt32Bits(prop.vector2Value.Y);
                        }
                    }
                    if (prop.key == s_fontFamilyImGuiKey.Hash) hash = hash * 23 + (uint)prop.tokenHash;
                    if (prop.key == s_fontSizeImGuiKey.Hash) {
                        if (prop.dataType == StyleDataType.Enum) hash = hash * 23 + (uint)prop.enumValue;
                        else if (prop.dataType == StyleDataType.Int) hash = hash * 23 + (uint)prop.intValue;
                    }
                }
                return hash;
            }
        }

        public void PopFontOnly()
        {
            bool hasFamily = TryGetFontFamily(out var fontFamily);
            bool hasSize = TryGetFontSize(out var fontSize);

            if (hasFamily || hasSize)
            {
                if (m_fontWasPushed) ImGui.PopFont();
                if (hasFamily) RenderEngine.Context.PopFontState();
            }
        }

        public unsafe void Push()
        {
            int colorCount = 0;
            int varCount = 0;

            foreach (var prop in m_properties)
            {
                if (prop.category != StyleCategory.ImGuiStyle) continue;

                if (prop.dataType == StyleDataType.Color)
                {
                    ImGui.PushStyleColor((ImGuiCol)prop.key, prop.colorValue);
                    colorCount++;
                }
                else if (prop.dataType == StyleDataType.Float && prop.key < (int)ImGuiStyleVar.Count)
                {
                    ImGui.PushStyleVar((ImGuiStyleVar)prop.key, prop.floatValue);
                    varCount++;
                }
                else if (prop.dataType == StyleDataType.Vector2 && prop.key < (int)ImGuiStyleVar.Count)
                {
                    ImGui.PushStyleVar((ImGuiStyleVar)prop.key, prop.vector2Value);
                    varCount++;
                }
            }

            PushFontOnly();
        }

        public void Pop()
        {
            PopFontOnly();

            int colorCount = 0;
            int varCount = 0;
            foreach (var prop in m_properties)
            {
                if (prop.category != StyleCategory.ImGuiStyle) continue;
                if (prop.dataType == StyleDataType.Color) colorCount++;
                else if ((prop.dataType == StyleDataType.Float || prop.dataType == StyleDataType.Vector2) && prop.key < (int)ImGuiStyleVar.Count) varCount++;
            }

            if (varCount > 0) ImGui.PopStyleVar(varCount);
            if (colorCount > 0) ImGui.PopStyleColor(colorCount);
        }
    }
}
