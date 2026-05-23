using ImGuiNET;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(bool), allowInheritType: false)]
    public class BoolDrawer : FieldDrawer<bool>
    {
        private CheckBox m_checkBox;

        public BoolDrawer()
        {
            m_checkBox = new CheckBox("##" + label);
            m_checkBox.onValueChanged += OnCheckBoxValueChanged;
            hierarchy.Add(m_checkBox);
        }

        public override string label
        {
            get => base.label;
            set
            {
                base.label = value;
                if (m_checkBox != null)
                {
                    m_checkBox.label = "##" + value;
                }
            }
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_checkBox.SetValueWithoutNotify(newValue);
        }

        public override bool value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_checkBox.SetValueWithoutNotify(value);
            }
        }

        private void OnCheckBoxValueChanged(ValueChangedEvent<bool> evt)
        {
            SetValueWithChanged(evt.newValue);
        }

        public override void OnRender()
        {
            // Do not render anything natively here.
            // The composed m_checkBox child element will be rendered by the VisualElement hierarchy.
            base.OnRender();
        }
    }
}
