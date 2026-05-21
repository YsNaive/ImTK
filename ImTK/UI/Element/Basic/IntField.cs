using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class IntField : InputFieldBase<int, IntField.Style>
    {
        public new class StyleKey : InputFieldBase<int, IntField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<int, IntField.Style>.InputFieldStyle
        {
        }

        public IntField(string label = "", int defaultValue = 0)
            : base(label, defaultValue)
        {
            classList.Add("int-field");
        }

        protected override void OnRenderSelf()
        {
            int currentValue = value;
            if (ImGui.InputInt(label, ref currentValue, 0))
            {
                SetValue(currentValue);
            }
        }
    }
}
