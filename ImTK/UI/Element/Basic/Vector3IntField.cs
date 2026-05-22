using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class Vector3IntField : InputFieldBase<Vector3Int, Vector3IntField.Style>
    {
        public new class StyleKey : InputFieldBase<Vector3Int, Vector3IntField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<Vector3Int, Vector3IntField.Style>.InputFieldStyle
        {
        }

        public Vector3IntField(string label = "", Vector3Int defaultValue = default)
            : base(label, defaultValue)
        {
            classList.Add("vector3int-field");
        }

        protected override void OnRenderSelf()
        {
            int[] currentValues = new int[] { value.x, value.y, value.z };
            if (ImGui.InputInt3(label, ref currentValues[0]))
            {
                SetValue(new Vector3Int(currentValues[0], currentValues[1], currentValues[2]));
            }
        }
    }
}
