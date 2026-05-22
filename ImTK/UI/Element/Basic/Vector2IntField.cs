using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class Vector2IntField : InputFieldBase<Vector2Int, Vector2IntField.Style>
    {
        public new class StyleKey : InputFieldBase<Vector2Int, Vector2IntField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<Vector2Int, Vector2IntField.Style>.InputFieldStyle
        {
        }

        public Vector2IntField(string label = "", Vector2Int defaultValue = default)
            : base(label, defaultValue)
        {
            classList.Add("vector2int-field");
        }

        protected override void OnRenderSelf()
        {
            int[] currentValues = new int[] { value.x, value.y };
            if (ImGui.InputInt2(label, ref currentValues[0]))
            {
                SetValue(new Vector2Int(currentValues[0], currentValues[1]));
            }
        }
    }
}
