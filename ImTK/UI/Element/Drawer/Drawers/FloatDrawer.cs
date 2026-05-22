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

            // To avoid ImGui's layout engine adding extra ItemSpacing.Y due to rendering
            // multiple items in succession on the Y axis, we need to carefully overlap them.

            // Calculate text size to size the invisible button appropriately
            var textSize = ImGui.CalcTextSize(label);

            // Optional: allow subsequent items to overlap this button
            ImGui.SetNextItemAllowOverlap();

            // Draw the invisible button first to claim the space and allow interaction. We use FrameHeight to match the input box height exactly.
            float frameHeight = ImGui.GetFrameHeight();
            var buttonPos = ImGui.GetCursorScreenPos();
            ImGui.InvisibleButton("##drag_" + label, new System.Numerics.Vector2(textSize.X, frameHeight));

            // Cache active state immediately for the InvisibleButton, as we draw Text next
            bool isDragActive = ImGui.IsItemActive();

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
            }

            // Save the end position so we can restore the layout cursor properly after text
            var endPos = ImGui.GetCursorScreenPos();

            // Move cursor back and draw the text vertically centered
            float textYOffset = (frameHeight - textSize.Y) * 0.5f;
            ImGui.SetCursorScreenPos(new System.Numerics.Vector2(buttonPos.X, buttonPos.Y + textYOffset));
            ImGui.Text(label);

            // Move cursor back to where it should be for the next layout item
            ImGui.SetCursorScreenPos(endPos);

            if (isDragActive && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
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
