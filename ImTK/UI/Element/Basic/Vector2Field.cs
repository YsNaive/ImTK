using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class Vector2Field : InputFieldBase<System.Numerics.Vector2, Vector2Field.Style>
    {
        public new class StyleKey : InputFieldBase<System.Numerics.Vector2, Vector2Field.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<System.Numerics.Vector2, Vector2Field.Style>.InputFieldStyle
        {
        }

        public int precision { get; set; } = 3;

        public Vector2Field(string label = "", System.Numerics.Vector2 defaultValue = default)
            : base(label, defaultValue)
        {
            classList.Add("vector2-field");
        }

        protected override void OnRenderSelf()
        {
            System.Numerics.Vector2 currentValue = value;
            string format = $"%.{precision}f";
            if (ImGui.InputFloat2(label, ref currentValue, format))
            {
                SetValue(currentValue);
            }
        }
    }
}
