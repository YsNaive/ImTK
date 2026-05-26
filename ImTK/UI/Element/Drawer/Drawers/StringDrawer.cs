using System;
using System.Numerics;
using ImGuiNET;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(string), allowInheritType: false)]
    public class StringDrawer : FieldDrawer<string>
    {
        private bool m_multiline;
        public bool multiline
        {
            get => m_multiline;
            set
            {
                m_multiline = value;
                UpdateTextFieldMode(this.value);
            }
        }
        
        private FieldElement m_fieldElement;

        public StringDrawer()
        {
            m_fieldElement = new FieldElement(this);
            m_contentContainer.Add(m_fieldElement);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateTextFieldMode(newValue);
        }

        public override string value
        {
            get => base.value;
            set
            {
                base.value = value;
                UpdateTextFieldMode(value);
            }
        }

        private void UpdateTextFieldMode(string val)
        {
            bool hasNewline = !string.IsNullOrEmpty(val) && val.Contains("\n");
            if (m_multiline || hasNewline)
            {
                layoutMode = DrawerLayoutMode.Expand;
            }
            else
            {
                layoutMode = DrawerLayoutMode.Inline;
            }
            m_fieldElement.MarkMeasureDirty();
        }

        private class FieldElement : VisualElement
        {
            private readonly StringDrawer m_drawer;
            public FieldElement(StringDrawer drawer)
            {
                m_drawer = drawer;
                this.style.flexGrow = 1;
            }

            protected override Vector2 MeasureContent(LayoutConstraint constraint)
            {
                string v = m_drawer.value ?? string.Empty;
                bool hasNewline = !string.IsNullOrEmpty(v) && v.Contains("\n");
                if (m_drawer.m_multiline || hasNewline)
                {
                    float height = Math.Max(ImGuiNET.ImGui.GetFrameHeight(), ImGuiNET.ImGui.CalcTextSize(v).Y + ImGuiNET.ImGui.GetStyle().FramePadding.Y * 2);
                    return new Vector2(0, height);
                }
                else
                {
                    return new Vector2(0, ImGuiNET.ImGui.GetFrameHeight());
                }
            }

            public override void OnRender()
            {
                string v = m_drawer.value ?? string.Empty;
                ImGuiNET.ImGuiInputTextFlags flags = ImGuiNET.ImGuiInputTextFlags.None;
                bool changed = false;

                bool hasNewline = !string.IsNullOrEmpty(v) && v.Contains("\n");
                if (m_drawer.m_multiline || hasNewline)
                {
                    float height = Math.Max(ImGuiNET.ImGui.GetFrameHeight(), ImGuiNET.ImGui.CalcTextSize(v).Y + ImGuiNET.ImGui.GetStyle().FramePadding.Y * 2);
                    changed = ImGuiNET.ImGui.InputTextMultiline("##" + m_drawer.label, ref v, 32768, new Vector2(this.layoutRect.width, height), flags);
                }
                else
                {
                    ImGuiNET.ImGui.SetNextItemWidth(this.layoutRect.width);
                    changed = ImGuiNET.ImGui.InputText("##" + m_drawer.label, ref v, 32768, flags);
                }

                if (changed || ImGuiNET.ImGui.IsItemDeactivatedAfterEdit())
                {
                    m_drawer.SetValueWithChanged(v);
                }
            }

        }
    }
}
