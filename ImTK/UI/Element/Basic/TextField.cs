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
        public bool multiline { get; set; } = false;
        public System.Numerics.Vector2 size { get; set; } = new System.Numerics.Vector2(0, 0);

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
            bool changed = false;

            System.Numerics.Vector2 renderSize = size;

            // Auto-growing text area logic: always use multiline under the hood to allow Enter key organically.
            if (renderSize.Y <= 0)
            {
                int lines = 1;
                // If multiline mode is explicitly OFF, we force it to look and act like 1 line visually,
                // but if it is ON, we calculate true lines. We also let string with \n grow it automatically.
                if (multiline || (!string.IsNullOrEmpty(currentValue) && currentValue.Contains("\n")))
                {
                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        foreach (char c in currentValue)
                        {
                            if (c == '\n') lines++;
                        }
                    }
                }

                float lineHeight = ImGui.GetTextLineHeight();
                float paddingY = ImGui.GetStyle().FramePadding.Y;

                // Add a tiny buffer to prevent jittering when reaching exactly 1 line
                renderSize.Y = (lines * lineHeight) + (paddingY * 2.0f);
            }

            // Always use Multiline so the user can actually press Enter to insert newlines
            // ImGui.InputTextFlags.AllowTabInput can be useful for multiline,
            // but we stick to defaults unless specified.
            // ImGui handles Enter to new line in Multiline mode by default.
            changed = ImGui.InputTextMultiline(label, ref currentValue, maxLength, renderSize);

            if (changed)
            {
                SetValue(currentValue);
            }
        }
    }
}
