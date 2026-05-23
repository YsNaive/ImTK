using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class RectField : InputFieldBase<Rect, RectField.Style>
    {
        public new class StyleKey : InputFieldBase<Rect, RectField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<Rect, RectField.Style>.InputFieldStyle
        {
        }

        public int precision { get; set; } = 3;

        public RectField(string label = "", Rect defaultValue = default)
            : base(label, defaultValue)
        {
            classList.Add("rect-field");
        }

        public override void OnRender()
        {
            System.Numerics.Vector4 currentValues = new System.Numerics.Vector4(value.x, value.y, value.width, value.height);
            string format = $"%.{precision}f";
            if (ImGui.InputFloat4(label, ref currentValues, format))
            {
                SetValue(new Rect(currentValues.X, currentValues.Y, currentValues.Z, currentValues.W));
            }
        }
    }
}
