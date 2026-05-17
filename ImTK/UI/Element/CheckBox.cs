using System;
using ImGuiNET;
using ImTK.UI.Style;

namespace ImTK.UI
{
    public class CheckBox : VisualElement
    {
        public string label { get; set; }

        private bool m_value;
        public bool value
        {
            get => m_value;
            set => SetValue(value);
        }

        public event Action<ValueChangedEvent<bool>> onValueChanged
        {
            add => RegisterCallback(value);
            remove => UnregisterCallback(value);
        }

        public CheckBox(string label = "", bool defaultValue = false)
        {
            this.label = label;
            m_value = defaultValue;
            classList.Add("CheckBox");
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            m_value = newValue;
        }

        private void SetValue(bool newValue)
        {
            if (m_value == newValue) return;

            var evt = ValueChangedEvent<bool>.GetPooled(m_value, newValue);
            m_value = newValue;
            SendEvent(evt);
        }

        protected override void OnRenderSelf()
        {
            bool currentValue = m_value;
            if (ImGui.Checkbox(label, ref currentValue))
            {
                SetValue(currentValue);
            }
        }
    }
}
