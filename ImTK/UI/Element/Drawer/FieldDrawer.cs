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

        private float? m_labelWidth = null;
        public float? labelWidth
        {
            get => m_labelWidth ?? theme.labelWidth;
            set => m_labelWidth = value;
        }

        public DrawerLayoutMode layoutMode { get; set; } = DrawerLayoutMode.Inline;

        public Rect? overrideRenderRect { get; set; }

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

        protected virtual void OnRenderIcon(ImDrawListPtr drawList, Rect iconRect)
        {
            // Base implementation does nothing, leaves empty space.
        }

        private float m_indentCache;

        public override bool OnBeginRender()
        {
            if (overrideRenderRect.HasValue)
            {
                Rect rect = overrideRenderRect.Value;
                ImGui.SetCursorScreenPos(rect.position);

                if (!string.IsNullOrEmpty(label))
                {
                    Vector2 startPos = ImGui.GetCursorScreenPos();
                    OnRenderLabel();

                    // We must call SameLine BEFORE reading the cursor position to ensure we measure horizontally
                    ImGui.SameLine();

                    Vector2 endPos = ImGui.GetCursorScreenPos();
                    float usedX = endPos.X - startPos.X;

                    float remainingWidth = Math.Max(0, rect.width - usedX);
                    ImGui.SetNextItemWidth(remainingWidth);
                }
                else
                {
                    ImGui.SetNextItemWidth(rect.width);
                }
            }
            else
            {
                float currentLabelWidth = labelWidth.Value;
                float frameHeight = ImGui.GetFrameHeight();
                float iconSize = frameHeight * 0.8f;
                float yOffset = (frameHeight - iconSize) * 0.5f;

                Vector2 cursorPos = ImGui.GetCursorScreenPos();
                Rect iconRect = new Rect(
                    new Vector2(cursorPos.X, cursorPos.Y + yOffset),
                    new Vector2(iconSize, iconSize)
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
                        float targetX = currentLabelWidth;
                        if (currentX < targetX)
                        {
                            ImGui.SameLine(targetX); // Force all input to start at currentLabelWidth
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
                }
                else if (layoutMode == DrawerLayoutMode.Expand)
                {
                    ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                    OnRenderLabel();

                    // Indent content for expand mode
                    m_indentCache = iconSize + ImGui.GetStyle().ItemInnerSpacing.X;
                    ImGui.Indent(m_indentCache);
                    ImGui.SetNextItemWidth(-1);
                }
            }
            return true;
        }

        public override void OnEndRender()
        {
            if (!overrideRenderRect.HasValue && layoutMode == DrawerLayoutMode.Expand)
            {
                ImGui.Unindent(m_indentCache);
            }
        }
    }
}
