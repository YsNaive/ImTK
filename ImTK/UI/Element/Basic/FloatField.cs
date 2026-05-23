using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class FloatField : InputFieldBase<float, FloatField.Style>
    {
        public new class StyleKey : InputFieldBase<float, FloatField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<float, FloatField.Style>.InputFieldStyle
        {
        }

        public int precision { get; set; } = 3;
        public float step { get; set; } = 0f;
        public float stepFast { get; set; } = 0f;

        public FloatField(string label = "", float defaultValue = 0f, int precision = 3)
            : base(label, defaultValue)
        {
            this.precision = precision;
            classList.Add("float-field");
        }

        public override void OnRender()
        {
            float currentValue = value;
            string format = $"%.{precision}f";
            if (ImGui.InputFloat(label, ref currentValue, step, stepFast, format))
            {
                SetValue(currentValue);
            }
        }
    }
}
