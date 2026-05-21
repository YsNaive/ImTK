using System;
using ImGuiNET;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(float), allowInheritType: false)]
    public class FloatDrawer : FieldDrawer<float>
    {
        private FloatField m_floatField;

        public float mouseStep { get; set; } = -1f;

        public float step
        {
            get => m_floatField.step;
            set => m_floatField.step = value;
        }

        public FloatDrawer()
        {
            m_floatField = new FloatField("##" + label);
            m_floatField.onValueChanged += OnFloatFieldValueChanged;
            hierarchy.Add(m_floatField);
        }

        public override string label
        {
            get => base.label;
            set
            {
                base.label = value;
                if (m_floatField != null)
                {
                    m_floatField.label = "##" + value;
                }
            }
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_floatField.SetValueWithoutNotify(newValue);
        }

        public override float value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_floatField.SetValueWithoutNotify(value);
            }
        }

        private void OnFloatFieldValueChanged(ValueChangedEvent<float> evt)
        {
            SetValueWithChanged(evt.newValue);
        }

        protected override void OnRenderLabel()
        {
            if (string.IsNullOrEmpty(label)) return;

            // Draw label
            ImGui.AlignTextToFramePadding();
            ImGui.Text(label);

            // Create an invisible button over the label for interaction
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            ImGui.SetCursorScreenPos(min);
            ImGui.InvisibleButton("##drag_" + label, new System.Numerics.Vector2(max.X - min.X, max.Y - min.Y));

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
            }

            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                float delta = ImGui.GetIO().MouseDelta.X;
                if (delta != 0)
                {
                    float currentStep = mouseStep;
                    if (currentStep < 0)
                    {
                        // Dynamic step based on value magnitude, minimum 0.01
                        currentStep = Math.Max(0.01f, Math.Abs(value) * 0.01f);
                    }

                    float newValue = value + delta * currentStep;
                    if (newValue != value)
                    {
                        value = newValue; // this triggers SetValueWithChanged logic internally via setter
                        m_floatField.SetValueWithoutNotify(newValue);
                    }
                }
            }
        }

        protected override void OnRenderSelf()
        {
            // Do not render anything natively here.
            base.OnRenderSelf();
        }
    }
}
