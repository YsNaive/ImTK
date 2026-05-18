using System;
using ImGuiNET;
using ImTK.UI.Style;

namespace ImTK.UI
{
    public class TextField : VisualElement
    {
        public string label { get; set; }

        public uint maxLength { get; set; }

        private string m_value;
        public string value
        {
            get => m_value;
            set => SetValue(value);
        }

        public event Action<ValueChangedEvent<string>> onValueChanged
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public TextField(string label = "", string defaultValue = "", uint maxLength = 1024)
        {
            this.label = label;
            m_value = defaultValue ?? string.Empty;
            this.maxLength = maxLength;
            classList.Add("TextField");
        }

        public void SetValueWithoutNotify(string newValue)
        {
            m_value = newValue ?? string.Empty;
        }

        private void SetValue(string newValue)
        {
            newValue = newValue ?? string.Empty;
            if (m_value == newValue) return;

            var evt = ValueChangedEvent<string>.GetPooled(m_value, newValue);
            m_value = newValue;
            SendEvent(evt);
        }

        protected override void OnRenderSelf()
        {
            string currentValue = m_value;
            if (ImGui.InputText(label, ref currentValue, maxLength))
            {
                SetValue(currentValue);
            }
        }
    }
}
