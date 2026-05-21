using ImGuiNET;
using System.Text;
using System.Runtime.InteropServices;
using System;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(string), allowInheritType: false)]
    public class StringDrawer : FieldDrawer<string>
    {
        private const int MAX_STRING_LENGTH = 1024;
        private byte[] m_buffer = new byte[MAX_STRING_LENGTH];

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateBuffer();
        }

        public override string value
        {
            get => base.value;
            set
            {
                base.value = value;
                UpdateBuffer();
            }
        }

        private void UpdateBuffer()
        {
            Array.Clear(m_buffer, 0, m_buffer.Length);
            if (!string.IsNullOrEmpty(m_value))
            {
                var bytes = Encoding.UTF8.GetBytes(m_value);
                Array.Copy(bytes, m_buffer, Math.Min(bytes.Length, m_buffer.Length - 1));
            }
        }

        protected override void OnRenderSelf()
        {
            base.OnRenderSelf();

            // We must deal with byte* safely in C# without GC pinned arrays if we pass to ImGui directly,
            // but ImGuiNET provides a nice wrapper `InputText` that takes `ref string`.
            // Wait, actually ImGuiNET's InputText wrapper taking `ref string` is sometimes problematic with GC,
            // but for a simple managed wrapper we can try using the `ref string` or byte array overload.

            // Let's use the byte array overload for safety and max length control.
            if (m_buffer == null) UpdateBuffer();

            if (ImGui.InputText("##" + label, m_buffer, (uint)m_buffer.Length))
            {
                // find null terminator
                int len = Array.IndexOf(m_buffer, (byte)0);
                if (len < 0) len = m_buffer.Length;

                string newStr = Encoding.UTF8.GetString(m_buffer, 0, len);
                SetValueWithChanged(newStr);
            }
        }
    }
}
