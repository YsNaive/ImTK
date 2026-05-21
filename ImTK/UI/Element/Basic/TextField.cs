using System;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class TextField : InputFieldBase<string, TextField.Style>
    {
        public new class StyleKey : InputFieldBase<string, TextField.Style>.StyleKey
        {
        }

        public new class Style : InputFieldBase<string, TextField.Style>.InputFieldStyle
        {
        }

        public uint maxLength { get; set; }

        public TextField(string label = "", string defaultValue = "", uint maxLength = 1024)
            : base(label, defaultValue)
        {
            this.maxLength = maxLength;
            classList.Add("text-field");
        }

        protected override string SanitizeValue(string value)
        {
            return value ?? string.Empty;
        }

        protected override void OnRenderSelf()
        {
            string currentValue = value;
            if (ImGui.InputText(label, ref currentValue, maxLength))
            {
                SetValue(currentValue);
            }
        }
    }
}
