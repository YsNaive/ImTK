using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class Vector3Field : InputFieldBase<System.Numerics.Vector3, Vector3Field.Style>
    {
        public new class StyleKey : InputFieldBase<System.Numerics.Vector3, Vector3Field.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<System.Numerics.Vector3, Vector3Field.Style>.InputFieldStyle
        {
        }

        public int precision { get; set; } = 3;

        public Vector3Field(string label = "", System.Numerics.Vector3 defaultValue = default)
            : base(label, defaultValue)
        {
            classList.Add("vector3-field");
        }

        public override void OnRender()
        {
            System.Numerics.Vector3 currentValue = value;
            string format = $"%.{precision}f";
            if (ImGui.InputFloat3(label, ref currentValue, format))
            {
                SetValue(currentValue);
            }
        }
    }
}
