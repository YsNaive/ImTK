using System;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(string), allowInheritType: false)]
    public class StringDrawer : FieldDrawer<string>
    {
        private TextField m_textField;

        private bool m_multiline;
        public bool multiline
        {
            get => m_multiline;
            set
            {
                m_multiline = value;
                UpdateTextFieldMode(this.value);
            }
        }

        public StringDrawer()
        {
            m_textField = new TextField("##" + label);
            m_textField.onValueChanged += OnTextFieldValueChanged;
            hierarchy.Add(m_textField);
        }

        public override string label
        {
            get => base.label;
            set
            {
                base.label = value;
                if (m_textField != null)
                {
                    m_textField.label = "##" + value;
                }
            }
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateTextFieldMode(newValue);
            m_textField.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => base.value;
            set
            {
                base.value = value;
                UpdateTextFieldMode(value);
                m_textField.SetValueWithoutNotify(value);
            }
        }

        private void OnTextFieldValueChanged(ValueChangedEvent<string> evt)
        {
            UpdateTextFieldMode(evt.newValue);
            SetValueWithChanged(evt.newValue);
        }

        private void UpdateTextFieldMode(string val)
        {
            bool hasNewline = !string.IsNullOrEmpty(val) && val.Contains("\n");
            if (m_multiline || hasNewline)
            {
                layoutMode = DrawerLayoutMode.Expand;
                m_textField.multiline = true;
            }
            else
            {
                layoutMode = DrawerLayoutMode.Inline;
                m_textField.multiline = false;
            }
        }

        protected override void OnRenderSelf()
        {
            // Do not render anything natively here.
            // The composed m_textField child element will be rendered by the VisualElement hierarchy.
            base.OnRenderSelf();
        }
    }
}
