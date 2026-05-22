using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public abstract class FieldDrawer<T> : VisualElement, IFieldDrawer<T>
    {
        protected T m_value;
        public virtual string label { get; set; } = "";

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

        public void RegisterValueChangedCallback(Action<ValueChangedEvent<T>> callback)
        {
            RegisterCallback(callback);
        }

        public void UnregisterValueChangedCallback(Action<ValueChangedEvent<T>> callback)
        {
            UnregisterCallback(callback);
        }

        protected virtual void OnRenderLabel()
        {
            if (!string.IsNullOrEmpty(label))
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text(label);
            }
        }

        protected virtual void OnRenderIcon(ImDrawListPtr drawList, ImRect iconRect)
        {
            // Base implementation does nothing, leaves empty space.
        }

        protected override void OnRenderLayout()
        {
            float labelWidth = theme.labelWidth;
            float frameHeight = ImGui.GetFrameHeight();
            float iconSize = frameHeight * 0.8f;
            float yOffset = (frameHeight - iconSize) * 0.5f;

            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            ImRect iconRect = new ImRect(
                new Vector2(cursorPos.X, cursorPos.Y + yOffset),
                new Vector2(cursorPos.X + iconSize, cursorPos.Y + yOffset + iconSize)
            );

            ImGui.Dummy(new Vector2(iconSize, frameHeight));
            OnRenderIcon(ImGui.GetWindowDrawList(), iconRect);

            if (layoutMode == DrawerLayoutMode.Inline)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                if (!string.IsNullOrEmpty(label))
                {
                    float currentX = ImGui.GetCursorPosX();
                    float targetX = labelWidth;
                    if (currentX < targetX)
                    {
                        ImGui.SameLine(targetX); // Force all input to start at labelWidth
                    }
                    else
                    {
                        ImGui.SameLine();
                    }
                }
                else
                {
                    ImGui.SameLine();
                }

                ImGui.SetNextItemWidth(-1); // Take remaining width
                base.OnRenderLayout();
            }
            else if (layoutMode == DrawerLayoutMode.Expand)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                OnRenderLabel();

                // Indent content for expand mode
                float indent = iconSize + ImGui.GetStyle().ItemInnerSpacing.X;
                ImGui.Indent(indent);
                ImGui.SetNextItemWidth(-1);
                base.OnRenderLayout();
                ImGui.Unindent(indent);
            }
        }
    }
}
