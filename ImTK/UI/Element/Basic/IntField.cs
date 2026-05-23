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

        public int step { get; set; } = 0;
        public int stepFast { get; set; } = 100;

        public IntField(string label = "", int defaultValue = 0)
            : base(label, defaultValue)
        {
            classList.Add("int-field");
        }

        public override void OnRender()
        {
            int currentValue = value;
            if (ImGui.InputInt(label, ref currentValue, step, stepFast))
            {
                SetValue(currentValue);
            }
        }
    }
}
