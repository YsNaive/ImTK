using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{

    public abstract class FieldDrawer<T> : VisualElement<FieldDrawer<T>.Style>, IFieldDrawer<T>
    {
        public new class Style : VisualElement.Style
        {
            public override void ComputeHighlevelToken(StyleProperty prop, IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken && prop.key == VisualElement.StyleKey.ColorFamily.Hash)
                {
                    string prefix = "--normal";
                    if (prop.enumValue == (int)ThemeColorFamily.Success) prefix = "--success";
                    else if (prop.enumValue == (int)ThemeColorFamily.Info) prefix = "--info";
                    else if (prop.enumValue == (int)ThemeColorFamily.Warning) prefix = "--warning";
                    else if (prop.enumValue == (int)ThemeColorFamily.Danger) prefix = "--danger";

                    output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.FrameBg, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component").Hash });
                    output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.FrameBgHovered, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-hover").Hash });
                    output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.FrameBgActive, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-component-active").Hash });
                    
                    // For BoolDrawer CheckMark
                    output.Add(new StyleProperty { category = StyleCategory.ThemeToken, key = (int)ImGuiCol.CheckMark, dataType = StyleDataType.HashedString, tokenHash = new HashedString(prefix + "-accent").Hash });
                    
                }
                base.ComputeHighlevelToken(prop, output);
            }
        }

        protected T m_value;

        private string m_label = "";
        private string m_cachedId = "##";
        public string cachedId => m_cachedId;
        public virtual string label
        {
            get => m_label;
            set
            {
                m_label = value;
                m_cachedId = "##" + (value ?? "");
                if (m_labelElement != null)
                {
                    m_labelElement.text = value;
                    m_labelElement.style.display = string.IsNullOrEmpty(m_label) ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        private float? m_labelWidth = null;
        public float? labelWidth
        {
            get => m_labelWidth ?? theme.labelWidth;
            set
            {
                m_labelWidth = value;
                if (m_labelElement != null && m_labelWidth.HasValue)
                {
                    m_labelElement.style.width = m_labelWidth.Value;
                }
                else if (m_labelElement != null)
                {
                    m_labelElement.style.width = null;
                }
            }
        }

        public IconElement.IconType iconType
        {
            get => m_iconElement?.type ?? IconElement.IconType.None;
            set
            {
                if (m_iconElement != null)
                {
                    m_iconElement.type = value;
                }
            }
        }

        private DrawerLayoutMode m_layoutMode = DrawerLayoutMode.Inline;
        /// <summary>
        /// 控制此 Drawer 繪製時的排版佈局模式 (例如同行並排 Inline 或是換行展開 Expand)。
        /// 設定此屬性會自動更新元件底層的 FlexDirection 與 AlignItems。
        /// </summary>
        public virtual DrawerLayoutMode layoutMode
        {
            get => m_layoutMode;
            set
            {
                m_layoutMode = value;
                if (m_layoutMode == DrawerLayoutMode.Inline)
                {
                    this.style.flexDirection = FlexDirection.Row;
                    this.style.alignItems = AlignItems.Center;
                }
                else
                {
                    this.style.flexDirection = FlexDirection.Column;
                    this.style.alignItems = AlignItems.Stretch;
                }
            }
        }

        protected VisualElement m_headerContainer;
        protected IconElement m_iconElement;
        protected Label m_labelElement;
        protected VisualElement m_contentContainer;

        public override VisualElement contentContainer => m_contentContainer;

        protected virtual VisualElement CreateHeaderContainer()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = AlignItems.Center;
            return container;
        }

        protected FieldDrawer()
        {
            m_headerContainer = CreateHeaderContainer();

            m_iconElement = new IconElement();
            
            m_labelElement = CreateLabelElement();
            m_labelElement.text = this.label;
            m_labelElement.style.display = DisplayStyle.None;
            if (labelWidth.HasValue) m_labelElement.style.width = labelWidth.Value;

            m_headerContainer.Add(m_iconElement);
            m_headerContainer.Add(m_labelElement);

            m_contentContainer = new VisualElement();
            m_contentContainer.style.flexGrow = 1;

            this.hierarchy.Add(m_headerContainer);
            this.hierarchy.Add(m_contentContainer);

            this.layoutMode = DrawerLayoutMode.Inline;
            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems    = AlignItems.Center;
            this.MarkStyleDirty();
        }

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

        void IFieldDrawer.SetValueWithoutNotify(object newValue)
        {
            if (newValue == null)
            {
                if (default(T) != null) return; // Ignore null for value types
                SetValueWithoutNotify((T)(object)null);
            }
            else if (newValue is T tValue)
            {
                SetValueWithoutNotify(tValue);
            }
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

        protected virtual Label CreateLabelElement()
        {
            return new Label();
        }

        public void RegisterValueChangedCallback(Action<ValueChangedEvent<T>> callback)
        {
            RegisterCallback(callback);
        }

        public void UnregisterValueChangedCallback(Action<ValueChangedEvent<T>> callback)
        {
            UnregisterCallback(callback);
        }
    }
}
