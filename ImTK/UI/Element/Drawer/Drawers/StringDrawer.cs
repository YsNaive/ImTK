using System;

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

        public StringDrawer()
        {
        }

        public override string label
        {
            get => base.label;
            set
            {
                base.label = value;
            }
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
        }

                protected internal override bool CheckHoverState()
        {
            return ImGuiNET.ImGui.IsItemHovered(ImGuiNET.ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        public override void OnRender()
        {
            string v = value ?? string.Empty;
            bool changed = false;

            if (layoutMode == DrawerLayoutMode.Expand || m_multiline)
            {
                var availWidth = ImGuiNET.ImGui.GetContentRegionAvail().X;
                float height = Math.Max(ImGuiNET.ImGui.GetFrameHeight(), ImGuiNET.ImGui.CalcTextSize(v).Y + ImGuiNET.ImGui.GetStyle().FramePadding.Y * 2);
                changed = ImGuiNET.ImGui.InputTextMultiline("##" + label, ref v, 32768, new System.Numerics.Vector2(availWidth, height), ImGuiNET.ImGuiInputTextFlags.None);
            }
            else
            {
                changed = ImGuiNET.ImGui.InputText("##" + label, ref v, 32768, ImGuiNET.ImGuiInputTextFlags.None);
            }

            if (changed)
            {
                SetValueWithChanged(v);
            }

            base.OnRender();
        }
    }
}
