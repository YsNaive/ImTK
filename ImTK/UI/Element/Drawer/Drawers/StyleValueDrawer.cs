using System;
using System.Collections.Generic;
using System.Reflection;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public enum StyleValueMode
    {
        Unset,
        Value,
        Token
    }

    [CustomFieldDrawer(typeof(StyleValue<>))]
    public class StyleValueDrawer<T> : FieldDrawer<StyleValue<T>?> where T : struct
    {
        private EnumDropdownDrawer<StyleValueMode> m_modeSelector;
        private Label m_inheritLabel;
        private IFieldDrawer m_valueDrawer;
        private StringDropdownDrawer m_tokenDropdown;
        private VisualElement m_fieldContainer;

        private static List<string> s_availableTokens;

        public StyleValueDrawer()
        {
            this.layoutMode = DrawerLayoutMode.Inline;
            
            m_fieldContainer = new VisualElement();
            m_fieldContainer.style.flexGrow = 1f;
            m_fieldContainer.style.flexDirection = FlexDirection.Row;
            m_fieldContainer.style.alignItems = AlignItems.Center;
            
            m_modeSelector = new EnumDropdownDrawer<StyleValueMode>();
            m_modeSelector.label = "";
            m_modeSelector.iconType = IconElement.IconType.Null;
            m_modeSelector.style.minWidth = theme.labelWidth;
            m_modeSelector.RegisterValueChangedCallback(OnModeChanged);
            
            m_inheritLabel = new Label("inherit");
            m_inheritLabel.style.textColor = ImTKTheme.GlobalTheme.normalColor.disabledText;
            m_inheritLabel.style.flexGrow = 1f;
            
            m_valueDrawer = FieldDrawerFactory.Create().FromType(typeof(T)).Build();
            if (m_valueDrawer is VisualElement valueVe)
            {
                m_valueDrawer.label = "";
                valueVe.style.flexGrow = 1f;
            }
            
            if (m_valueDrawer is FieldDrawer<T> typedValueDrawer)
            {
                typedValueDrawer.iconType = IconElement.IconType.Null;
                typedValueDrawer.RegisterValueChangedCallback(evt => 
                {
                    if (m_modeSelector.value == StyleValueMode.Value && !m_isUpdatingUI)
                    {
                        var newValue = new StyleValue<T> { Value = evt.newValue, Keyword = StyleKeyword.Undefined };
                        SetValueWithChanged(newValue);
                    }
                });
            }
            else
            {
                var pIconType = m_valueDrawer?.GetType().GetProperty("iconType");
                if (pIconType != null) pIconType.SetValue(m_valueDrawer, IconElement.IconType.Null);
                
                RegisterGenericCallback(m_valueDrawer as VisualElement, typeof(T));
            }

            m_tokenDropdown = new StringDropdownDrawer();
            m_tokenDropdown.label = "";
            m_tokenDropdown.iconType = IconElement.IconType.Null;
            m_tokenDropdown.style.flexGrow = 1f;
            if (s_availableTokens == null)
            {
                s_availableTokens = GatherTokens();
            }
            m_tokenDropdown.options = s_availableTokens;
            m_tokenDropdown.searchable = true;
            m_tokenDropdown.RegisterValueChangedCallback(evt => 
            {
                if (m_modeSelector.value == StyleValueMode.Token && !m_isUpdatingUI)
                {
                    var newValue = new StyleValue<T> { Token = evt.newValue, Keyword = StyleKeyword.Undefined };
                    SetValueWithChanged(newValue);
                }
            });

            m_fieldContainer.Add(m_inheritLabel);
            if (m_valueDrawer is VisualElement ve) m_fieldContainer.Add(ve);
            m_fieldContainer.Add(m_tokenDropdown);
            
            m_contentContainer.style.flexDirection = FlexDirection.Row;
            m_contentContainer.Add(m_fieldContainer);
            m_contentContainer.Add(m_modeSelector);
            
            UpdateUIFromValue(this.value);
        }

        private void RegisterGenericCallback(VisualElement element, Type valueType)
        {
            if (element == null) return;
            var method = GetType().GetMethod(nameof(RegisterGenericCallbackInternal), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(valueType);
            genericMethod.Invoke(this, new object[] { element });
        }

        private void RegisterGenericCallbackInternal<TValue>(VisualElement element)
        {
            element.RegisterCallback<ValueChangedEvent<TValue>>(evt => 
            {
                if (m_modeSelector.value == StyleValueMode.Value && !m_isUpdatingUI)
                {
                    if (evt.newValue is T typedVal)
                    {
                        var newValue = new StyleValue<T> { Value = typedVal, Keyword = StyleKeyword.Undefined };
                        SetValueWithChanged(newValue);
                    }
                }
            });
        }

        private static List<string> GatherTokens()
        {
            var tokens = new List<string>();
            var tokenTypes = new Type[] { typeof(ImTKTheme.Tokens), typeof(ImTKTheme.Tokens.Syntax) };
            foreach (var t in tokenTypes)
            {
                var fields = t.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var f in fields)
                {
                    if (f.FieldType == typeof(HashedString))
                    {
                        var hs = (HashedString)f.GetValue(null);
                        tokens.Add(hs.Value);
                    }
                }
            }
            
            string[] families = { "--normal", "--success", "--info", "--warning", "--danger" };
            string[] suffixes = { "-surface", "-container", "-component", "-component-hover", "-component-active", "-accent", "-accent-hover", "-accent-active", "-selection", "-border", "-divider", "-text", "-sub-text", "-disabled-text" };
            foreach (var f in families)
            {
                foreach (var s in suffixes)
                {
                    tokens.Add(f + s);
                }
            }
            return tokens;
        }

        private bool m_isUpdatingUI = false;

        public override StyleValue<T>? value
        {
            get => base.value;
            set
            {
                base.value = value;
                UpdateUIFromValue(value);
            }
        }
        
        public override void SetValueWithoutNotify(StyleValue<T>? newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateUIFromValue(newValue);
        }

        private void UpdateUIFromValue(StyleValue<T>? valNullable)
        {
            m_isUpdatingUI = true;
            
            if (!valNullable.HasValue || valNullable.Value.Keyword == StyleKeyword.Null)
            {
                m_modeSelector.SetValueWithoutNotify(StyleValueMode.Unset);
            }
            else
            {
                var val = valNullable.Value;
                if (val.IsToken)
                {
                    m_modeSelector.SetValueWithoutNotify(StyleValueMode.Token);
                    m_tokenDropdown.SetValueWithoutNotify(val.Token.Value);
                }
                else
                {
                    m_modeSelector.SetValueWithoutNotify(StyleValueMode.Value);
                    if (m_valueDrawer != null)
                    {
                        m_valueDrawer.SetValueWithoutNotify(val.Value);
                    }
                }
            }
            
            RefreshFieldVisibility();
            m_isUpdatingUI = false;
        }

        private void OnModeChanged(ValueChangedEvent<StyleValueMode> evt)
        {
            if (m_isUpdatingUI) return;
            
            RefreshFieldVisibility();
            
            StyleValue<T>? newValue = null;
            if (evt.newValue == StyleValueMode.Unset)
            {
                newValue = null;
            }
            else if (evt.newValue == StyleValueMode.Token)
            {
                newValue = new StyleValue<T> { Token = m_tokenDropdown.value, Keyword = StyleKeyword.Undefined };
            }
            else if (evt.newValue == StyleValueMode.Value)
            {
                if (m_valueDrawer != null && m_valueDrawer.value is T tVal)
                {
                    newValue = new StyleValue<T> { Value = tVal, Keyword = StyleKeyword.Undefined };
                }
                else
                {
                    newValue = new StyleValue<T> { Value = default, Keyword = StyleKeyword.Undefined };
                }
            }
            SetValueWithChanged(newValue);
            UpdateUIFromValue(newValue);
        }

        private void RefreshFieldVisibility()
        {
            var mode = m_modeSelector.value;
            m_inheritLabel.style.display = mode == StyleValueMode.Unset ? DisplayStyle.Flex : DisplayStyle.None;
            m_tokenDropdown.style.display = mode == StyleValueMode.Token ? DisplayStyle.Flex : DisplayStyle.None;
            
            if (m_valueDrawer is VisualElement ve)
            {
                ve.style.display = mode == StyleValueMode.Value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
