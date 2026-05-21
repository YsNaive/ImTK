using System;
using ImGuiNET;

using ImTK.Core;

namespace ImTK.UI
{
    public class CheckBox : VisualElement<CheckBox.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString HoverColor = new HashedString("HoverColor");
            public static readonly HashedString ActiveColor = new HashedString("ActiveColor");
            public static readonly HashedString CheckMarkColor = new HashedString("CheckMarkColor");
        }

        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            public StyleValue<Color>? hoverColor
            {
                get => GetOverrideColor(StyleKey.HoverColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.HoverColor, value.Value);
                    else Clear(StyleKey.HoverColor);
                }
            }

            public StyleValue<Color>? activeColor
            {
                get => GetOverrideColor(StyleKey.ActiveColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.ActiveColor, value.Value);
                    else Clear(StyleKey.ActiveColor);
                }
            }

            public StyleValue<Color>? checkMarkColor
            {
                get => GetOverrideColor(StyleKey.CheckMarkColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.CheckMarkColor, value.Value);
                    else Clear(StyleKey.CheckMarkColor);
                }
            }

            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);

                m_pushedColors = 0;

                // CheckBox Background maps to FrameBg, not WindowBg/ChildBg
                Color? bgColor = resolvedStyle.GetColor(VisualElement.StyleKey.BackgroundColor);
                if (bgColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, bgColor.Value.u32);
                    m_pushedColors++;
                }

                Color? hoverColor = resolvedStyle.GetColor(StyleKey.HoverColor);
                if (hoverColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, hoverColor.Value.u32);
                    m_pushedColors++;
                }

                Color? activeColor = resolvedStyle.GetColor(StyleKey.ActiveColor);
                if (activeColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBgActive, activeColor.Value.u32);
                    m_pushedColors++;
                }

                Color? checkMarkColor = resolvedStyle.GetColor(StyleKey.CheckMarkColor);
                if (checkMarkColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.CheckMark, checkMarkColor.Value.u32);
                    m_pushedColors++;
                }
            }

            public override void PopFromImGui()
            {
                if (m_pushedColors > 0)
                {
                    ImGui.PopStyleColor(m_pushedColors);
                    m_pushedColors = 0;
                }
                base.PopFromImGui();
            }
        }

        public string label { get; set; }

        private bool m_value;
        public bool value
        {
            get => m_value;
            set => SetValue(value);
        }

        public event Action<ValueChangedEvent<bool>> onValueChanged
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public CheckBox(string label = "", bool defaultValue = false)
        {
            this.label = label;
            m_value = defaultValue;
            classList.Add("CheckBox");
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            m_value = newValue;
        }

        private void SetValue(bool newValue)
        {
            if (m_value == newValue) return;

            var evt = ValueChangedEvent<bool>.GetPooled(m_value, newValue);
            m_value = newValue;
            SendEvent(evt);
        }

        protected override void OnRenderSelf()
        {
            bool currentValue = m_value;
            if (ImGui.Checkbox(label, ref currentValue))
            {
                SetValue(currentValue);
            }
        }
    }
}
