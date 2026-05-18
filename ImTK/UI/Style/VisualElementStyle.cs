using System.Collections.Generic;
using System.Numerics;
using ImTK.Core;
using ImGuiNET;

namespace ImTK.UI.Style
{
    public class VisualElementStyle : IVisualElementStyle
    {
        internal List<StyleProperty> m_overrideStyles;

        public VisualElementStyle() { }

        // --- Low-level Override Setters ---

        public void SetColor(HashedString key, StyleValue<Color> value)
        {
            EnsureOverrideStyles();
            RemoveEntry(key.Hash);

            if (value.IsNull) return;

            var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.ColorValue };
            if (value.IsToken) prop.tokenHash = value.Token.Hash;
            else prop.colorValue = value.Value.u32;

            m_overrideStyles.Add(prop);
        }

        public void SetFloat(HashedString key, StyleValue<float> value)
        {
            EnsureOverrideStyles();
            RemoveEntry(key.Hash);

            if (value.IsNull) return;

            var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.FloatValue };
            if (value.IsToken) prop.tokenHash = value.Token.Hash;
            else prop.floatValue = value.Value;

            m_overrideStyles.Add(prop);
        }

        public void SetVector2(HashedString key, StyleValue<Vector2> value)
        {
            EnsureOverrideStyles();
            RemoveEntry(key.Hash);

            if (value.IsNull) return;

            var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.Vector2Value };
            if (value.IsToken) prop.tokenHash = value.Token.Hash;
            else prop.vector2Value = value.Value;

            m_overrideStyles.Add(prop);
        }

        // --- Low-level Override Clearers ---

        public void Clear(HashedString key)
        {
            RemoveEntry(key.Hash);
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

        private void RemoveEntry(int keyHash)
        {
            if (m_overrideStyles == null) return;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == keyHash)
                {
                    m_overrideStyles.RemoveAt(i);
                    return;
                }
            }
        }

        private StyleValue<Color>? GetOverrideColor(HashedString key)
        {
            if (m_overrideStyles == null) return null;
            int keyHash = key.Hash;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == keyHash)
                {
                    if (m_overrideStyles[i].isToken)
                        return new StyleValue<Color> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<Color> { Value = (Color)m_overrideStyles[i].colorValue };
                }
            }
            return null;
        }

        private StyleValue<float>? GetOverrideFloat(HashedString key)
        {
            if (m_overrideStyles == null) return null;
            int keyHash = key.Hash;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == keyHash)
                {
                    if (m_overrideStyles[i].isToken)
                        return new StyleValue<float> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<float> { Value = m_overrideStyles[i].floatValue };
                }
            }
            return null;
        }

        private StyleValue<Vector2>? GetOverrideVector2(HashedString key)
        {
            if (m_overrideStyles == null) return null;
            int keyHash = key.Hash;
            for (int i = 0; i < m_overrideStyles.Count; i++)
            {
                if (m_overrideStyles[i].key == keyHash)
                {
                    if (m_overrideStyles[i].isToken)
                        return new StyleValue<Vector2> { Keyword = StyleKeyword.Undefined };
                    return new StyleValue<Vector2> { Value = m_overrideStyles[i].vector2Value };
                }
            }
            return null;
        }

        // --- IVisualElementStyle Implementation ---

        private int m_pushedColors = 0;
        private int m_pushedVars = 0;

        public virtual void PushToImGui(ResolvedStyle resolvedStyle)
        {
            m_pushedColors = 0;
            m_pushedVars = 0;

            Color? bgColor = resolvedStyle.GetColor(ImTKStyleKey.BackgroundColor);
            if (bgColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor.Value.u32);
                ImGui.PushStyleColor(ImGuiCol.ChildBg, bgColor.Value.u32);
                m_pushedColors += 2;
            }

            Color? textColor = resolvedStyle.GetColor(ImTKStyleKey.TextColor);
            if (textColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.u32);
                m_pushedColors++;
            }

            Color? borderColor = resolvedStyle.GetColor(ImTKStyleKey.BorderColor);
            if (borderColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, borderColor.Value.u32);
                m_pushedColors++;
            }

            float? borderRadius = resolvedStyle.GetFloat(ImTKStyleKey.BorderRadius);
            if (borderRadius.HasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, borderRadius.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, borderRadius.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, borderRadius.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, borderRadius.Value);
                m_pushedVars += 4;
            }

            float? borderWidth = resolvedStyle.GetFloat(ImTKStyleKey.BorderWidth);
            if (borderWidth.HasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, borderWidth.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, borderWidth.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, borderWidth.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, borderWidth.Value);
                m_pushedVars += 4;
            }

            Vector2? padding = resolvedStyle.GetVector2(ImTKStyleKey.Padding);
            if (padding.HasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, padding.Value);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, padding.Value);
                m_pushedVars += 2;
            }

            Vector2? itemSpacing = resolvedStyle.GetVector2(ImTKStyleKey.ItemSpacing);
            if (itemSpacing.HasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, itemSpacing.Value);
                m_pushedVars++;
            }

            Vector2? itemInnerSpacing = resolvedStyle.GetVector2(ImTKStyleKey.ItemInnerSpacing);
            if (itemInnerSpacing.HasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, itemInnerSpacing.Value);
                m_pushedVars++;
            }

            float? alpha = resolvedStyle.GetFloat(ImTKStyleKey.Alpha);
            if (alpha.HasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha.Value);
                m_pushedVars++;
            }
        }

        public virtual void PopFromImGui()
        {
            if (m_pushedColors > 0) ImGui.PopStyleColor(m_pushedColors);
            if (m_pushedVars > 0) ImGui.PopStyleVar(m_pushedVars);
            m_pushedColors = 0;
            m_pushedVars = 0;
        }
    }
}
