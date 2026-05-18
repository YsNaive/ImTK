using System;
using System.Collections.Generic;
using ImGuiNET;

namespace ImTK.UI
{
    public abstract class FieldDrawer<T> : VisualElement, IFieldDrawer<T>
    {
        protected T m_value;
        public string label { get; set; } = "";

        public DrawerLayoutMode layoutMode { get; set; } = DrawerLayoutMode.Inline;

        object IFieldDrawer.value
        {
            get => m_value;
            set => this.value = (T)value;
        }

        public virtual T value
        {
            get => m_value;
            set => _SetValue(value, checkEquality: true, notify: true);
        }

        public virtual void SetValueWithoutNotify(T newValue)
        {
            _SetValue(newValue, checkEquality: true, notify: false);
        }

        public virtual void SetValueWithChanged(T newValue)
        {
            _SetValue(newValue, checkEquality: false, notify: true);
        }

        public virtual void NotifyValueChanged()
        {
            _SetValue(m_value, checkEquality: false, notify: true, forceNotify: true);
        }

        private void _SetValue(T newValue, bool checkEquality, bool notify, bool forceNotify = false)
        {
            if (checkEquality && !forceNotify)
            {
                if (EqualityComparer<T>.Default.Equals(m_value, newValue))
                    return;
            }

            T previousValue = m_value;
            m_value = newValue;

            if (notify)
            {
                var evt = ValueChangedEvent<T>.GetPooled(previousValue, m_value, forceNotify);
                evt.source = this;
                SendEvent(evt);
            }
        }

        public virtual void ApplyModifier(Attribute modifier)
        {
            // Base implementation does nothing.
        }

        protected override void OnRenderLayout()
        {
            float labelWidth = theme.labelWidth;

            if (layoutMode == DrawerLayoutMode.Inline)
            {
                if (!string.IsNullOrEmpty(label))
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(label);
                    ImGui.SameLine(labelWidth); // Force all input to start at labelWidth
                }

                ImGui.SetNextItemWidth(-1); // Take remaining width
                base.OnRenderLayout();
            }
            else if (layoutMode == DrawerLayoutMode.Expand)
            {
                if (!string.IsNullOrEmpty(label))
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(label);
                }

                // Indent content for expand mode
                ImGui.Indent(15.0f);
                ImGui.SetNextItemWidth(-1);
                base.OnRenderLayout();
                ImGui.Unindent(15.0f);
            }
        }
    }
}
