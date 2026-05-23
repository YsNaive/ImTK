using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class RectIntField : InputFieldBase<RectInt, RectIntField.Style>
    {
        public new class StyleKey : InputFieldBase<RectInt, RectIntField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<RectInt, RectIntField.Style>.InputFieldStyle
        {
        }

        public RectIntField(string label = "", RectInt defaultValue = default)
            : base(label, defaultValue)
        {
            classList.Add("rectint-field");
        }

        public override void OnRender()
        {
            int[] currentValues = new int[] { value.x, value.y, value.width, value.height };
            if (ImGui.InputInt4(label, ref currentValues[0]))
            {
                SetValue(new RectInt(currentValues[0], currentValues[1], currentValues[2], currentValues[3]));
            }
        }
    }
}
