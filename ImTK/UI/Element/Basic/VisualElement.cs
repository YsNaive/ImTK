using System;
using System.Collections.Generic;
using ImGuiNET;

using System.Numerics;
using ImTK.Core;

namespace ImTK.UI
{
    public class VisualElement : IVisualElementHierarchy
    {
        public class StyleKey
        {
            public static readonly HashedString BackgroundColor = new HashedString("BackgroundColor");
            public static readonly HashedString TextColor = new HashedString("TextColor");
            public static readonly HashedString DisabledTextColor = new HashedString("DisabledTextColor");
            public static readonly HashedString SelectionColor = new HashedString("SelectionColor");
            public static readonly HashedString BorderColor = new HashedString("BorderColor");

            public static readonly HashedString Padding = new HashedString("Padding");
            public static readonly HashedString ItemSpacing = new HashedString("ItemSpacing");
            public static readonly HashedString ItemInnerSpacing = new HashedString("ItemInnerSpacing");
            public static readonly HashedString BorderWidth = new HashedString("BorderWidth");
            public static readonly HashedString BorderRadius = new HashedString("BorderRadius");
            public static readonly HashedString Alpha = new HashedString("Alpha");
            public static readonly HashedString DisabledAlpha = new HashedString("DisabledAlpha");

            public static readonly HashedString FontFamily = new HashedString("FontFamily");
            public static readonly HashedString FontSize = new HashedString("FontSize");
        }

        public class Style : VisualElementStyle, IVisualElementStyle
        {
            public StyleValue<HashedString> fontFamily { set => SetStringToken(StyleKey.FontFamily, value); }
            public StyleValue<int> fontSize { set => SetInt(StyleKey.FontSize, value); }
            public StyleValue<FontSize> fontSizeEnum { set => SetEnum(StyleKey.FontSize, value); }

            public Style() { }

            protected StyleValue<Color>? GetPropertyColor(HashedString key)
            {
                var p = GetProperty(key.Hash);
                return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Color> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Color> { Value = (Color)p.colorValue });
            }

            protected StyleValue<float>? GetPropertyFloat(HashedString key)
            {
                var p = GetProperty(key.Hash);
                return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<float> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<float> { Value = p.floatValue });
            }

            // --- Low-level Override Setters ---

            public void SetColor(HashedString key, StyleValue<Color> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Color };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.colorValue = value.Value.u32;
                SetProperty(prop);
            }

            public void SetInt(HashedString key, StyleValue<int> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Int };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.intValue = value.Value;
                SetProperty(prop);
            }

            public void SetEnum<TEnum>(HashedString key, StyleValue<TEnum> value) where TEnum : struct, System.Enum
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Enum };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.enumValue = System.Convert.ToInt32(value.Value);
                SetProperty(prop);
            }

            public void SetStringToken(HashedString key, StyleValue<HashedString> value)
            {
                if (value.IsNull) { Clear(key); return; }
                int hash = value.IsToken ? value.Token.Hash : value.Value.Hash;
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = StyleDataType.HashedString, tokenHash = hash };
                SetProperty(prop);
            }

            public void SetFloat(HashedString key, StyleValue<float> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Float };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.floatValue = value.Value;
                SetProperty(prop);
            }

            public void SetVector2(HashedString key, StyleValue<Vector2> value)
            {
                if (value.IsNull) { Clear(key); return; }
                var prop = new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = value.IsToken ? StyleDataType.HashedString : StyleDataType.Vector2 };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.vector2Value = value.Value;
                SetProperty(prop);
            }

            public void Clear(HashedString key)
            {
                SetProperty(new StyleProperty { category = StyleCategory.HighLevelToken, key = key.Hash, dataType = StyleDataType.Null });
            }

            // --- High-level Property Syntax Sugar ---

            public StyleValue<Color>? backgroundColor
            {
                get => GetPropertyColor(StyleKey.BackgroundColor);
                set { if (value.HasValue) SetColor(StyleKey.BackgroundColor, value.Value); else Clear(StyleKey.BackgroundColor); }
            }

            public StyleValue<Color>? textColor
            {
                get => GetPropertyColor(StyleKey.TextColor);
                set { if (value.HasValue) SetColor(StyleKey.TextColor, value.Value); else Clear(StyleKey.TextColor); }
            }

            public StyleValue<Color>? disabledTextColor
            {
                get => GetPropertyColor(StyleKey.DisabledTextColor);
                set { if (value.HasValue) SetColor(StyleKey.DisabledTextColor, value.Value); else Clear(StyleKey.DisabledTextColor); }
            }

            public StyleValue<Color>? borderColor
            {
                get => GetPropertyColor(StyleKey.BorderColor);
                set { if (value.HasValue) SetColor(StyleKey.BorderColor, value.Value); else Clear(StyleKey.BorderColor); }
            }

            public StyleValue<float>? borderWidth
            {
                get => GetPropertyFloat(StyleKey.BorderWidth);
                set { if (value.HasValue) SetFloat(StyleKey.BorderWidth, value.Value); else Clear(StyleKey.BorderWidth); }
            }

            public StyleValue<float>? borderRadius
            {
                get => GetPropertyFloat(StyleKey.BorderRadius);
                set { if (value.HasValue) SetFloat(StyleKey.BorderRadius, value.Value); else Clear(StyleKey.BorderRadius); }
            }

            public StyleValue<float>? alpha
            {
                get => GetPropertyFloat(StyleKey.Alpha);
                set { if (value.HasValue) SetFloat(StyleKey.Alpha, value.Value); else Clear(StyleKey.Alpha); }
            }

            public StyleValue<float>? disabledAlpha
            {
                get => GetPropertyFloat(StyleKey.DisabledAlpha);
                set { if (value.HasValue) SetFloat(StyleKey.DisabledAlpha, value.Value); else Clear(StyleKey.DisabledAlpha); }
            }

            public StyleValue<Vector2>? padding
            {
                get { var p = GetProperty(StyleKey.Padding.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.Padding, value.Value); else Clear(StyleKey.Padding); }
            }

            public StyleValue<Vector2>? itemSpacing
            {
                get { var p = GetProperty(StyleKey.ItemSpacing.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.ItemSpacing, value.Value); else Clear(StyleKey.ItemSpacing); }
            }

            public StyleValue<Vector2>? itemInnerSpacing
            {
                get { var p = GetProperty(StyleKey.ItemInnerSpacing.Hash); return p.dataType == StyleDataType.Null ? null : (p.dataType == StyleDataType.HashedString ? new StyleValue<Vector2> { Token = new HashedString("TOKEN_" + p.tokenHash) } : new StyleValue<Vector2> { Value = p.vector2Value }); }
                set { if (value.HasValue) SetVector2(StyleKey.ItemInnerSpacing, value.Value); else Clear(StyleKey.ItemInnerSpacing); }
            }

            public StyleValue<Color>? selectionColor
            {
                get => GetPropertyColor(StyleKey.SelectionColor);
                set { if (value.HasValue) SetColor(StyleKey.SelectionColor, value.Value); else Clear(StyleKey.SelectionColor); }
            }
        }

        private static int s_elementCounter = 0;
        internal readonly int m_elementId;

        public VisualElementHierarchy hierarchy { get; }
        public virtual VisualElement contentContainer => this;

        public VisualElement parent { get; internal set; }

        public PickingMode pickingMode { get; set; } = PickingMode.Position;
        internal bool m_wasHovered = false;
        internal bool m_useAutoId = true;

        private Dictionary<Type, Delegate> m_callbacks;

        public IVisualElementStyle internalStyle { get; set; }

        public Style style => internalStyle as Style;

        public StyleClass classList { get; private set; }

        private StyleSheet m_localStyleSheet;
        public StyleSheet localStyleSheet
        {
            get => m_localStyleSheet;
            set
            {
                if (m_localStyleSheet != value)
                {
                    m_localStyleSheet = value;
                    MarkStyleDirty();
                }
            }
        }

        internal bool m_isStyleDirty = true;

        public ImGuiStyleHandler resolvedStyle { get; } = new ImGuiStyleHandler();
        public ImGuiStyleHandler requiredStyle { get; } = new ImGuiStyleHandler();


        public VisualElement()
        {
            m_elementId = ++s_elementCounter;
            internalStyle = new Style();
            hierarchy = new VisualElementHierarchy(this);

            classList = new StyleClass();
            classList.OnClassChanged = MarkStyleDirty;
        }

        public void MarkStyleDirty()
        {
            m_isStyleDirty = true;
            int count = hierarchy.childCount;
            for (int i = 0; i < count; i++)
            {
                hierarchy.childAt(i).MarkStyleDirty();
            }
        }

        private ImTKTheme m_theme;

        public ImTKTheme theme
        {
            get
            {
                if (m_theme != null) return m_theme;
                if (parent != null) return parent.theme;
                return ImTKTheme.GlobalTheme;
            }
            set
            {
                if (m_theme != value)
                {
                    m_theme = value;
                    MarkStyleDirty();
                }
            }
        }

        public NodeType GetNodeType()
        {
            bool hasLogicalParent = this.parent != null;
            bool hasPhysicalParent = this.hierarchy.parent != null;

            if (!hasLogicalParent && !hasPhysicalParent) return NodeType.None;
            if (hasLogicalParent && hasPhysicalParent) return NodeType.LogicNode;
            if (!hasLogicalParent && hasPhysicalParent) return NodeType.PhysicsNode;
            return NodeType.Invalid;
        }

        public int childCount => contentContainer == this ? hierarchy.childCount : contentContainer.childCount;

        public VisualElement childAt(int index) => contentContainer == this ? hierarchy.childAt(index) : contentContainer.childAt(index);

        public void Add(VisualElement child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (!hierarchy.CheckSafeState()) return;

            NodeType type = child.GetNodeType();
            if (type == NodeType.LogicNode)
            {
                child.parent.Remove(child);
            }
            else if (type == NodeType.PhysicsNode)
            {
                child.hierarchy.parent.hierarchy.Remove(child);
            }

            child.parent = this;

            if (contentContainer == this)
            {
                hierarchy.Add(child);
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Add(child);
            }

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public void AddRange(IEnumerable<VisualElement> children)
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            foreach (var child in children)
            {
                Add(child);
            }
        }

        public void Remove(VisualElement child)
        {
            if (child == null) return;
            if (!hierarchy.CheckSafeState()) return;

            if (contentContainer == this)
            {
                hierarchy.Remove(child);
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Remove(child);
            }

            if (child.parent == this)
            {
                child.parent = null;
            }

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public void Clear()
        {
            if (!hierarchy.CheckSafeState()) return;

            var childrenToClear = new List<VisualElement>(contentContainer == this ? hierarchy.Children() : contentContainer.Children());
            foreach(var child in childrenToClear)
            {
                if (child.parent == this)
                {
                    child.parent = null;
                }
            }

            if (contentContainer == this)
            {
                hierarchy.Clear();
            }
            else
            {
                VisualElement targetContainer = contentContainer;
                while (targetContainer.contentContainer != targetContainer)
                {
                    targetContainer = targetContainer.contentContainer;
                }
                targetContainer.hierarchy.Clear();
            }

            EventDispatcher.MarkHierarchyDirty(this);
        }

        public IEnumerable<VisualElement> Children()
        {
            return contentContainer == this ? hierarchy.Children() : contentContainer.Children();
        }

        public void RegisterCallback<TEventType>(Action<TEventType> callback) where TEventType : UIEventBase
        {
            if (m_callbacks == null) m_callbacks = new Dictionary<Type, Delegate>();

            Type type = typeof(TEventType);
            if (m_callbacks.TryGetValue(type, out var existing))
            {
                m_callbacks[type] = Delegate.Combine(existing, callback);
            }
            else
            {
                m_callbacks[type] = callback;
            }
        }

        public void UnregisterCallback<TEventType>(Action<TEventType> callback) where TEventType : UIEventBase
        {
            if (m_callbacks == null) return;

            Type type = typeof(TEventType);
            if (m_callbacks.TryGetValue(type, out var existing))
            {
                var newDelegate = Delegate.Remove(existing, callback);
                if (newDelegate == null)
                {
                    m_callbacks.Remove(type);
                }
                else
                {
                    m_callbacks[type] = newDelegate;
                }
            }
        }

        protected void SendEvent(UIEventBase evt)
        {
            evt.source = this;
            EventDispatcher.Enqueue(evt);
        }

        public virtual bool OnBeginRender()
        {
            return true;
        }

        public virtual void OnRender()
        {
        }

        public virtual void OnEndRender()
        {
        }

        public virtual void HandleEvent(UIEventBase evt)
        {
            if (m_callbacks != null && m_callbacks.TryGetValue(evt.GetType(), out var callback))
            {
                callback.DynamicInvoke(evt);
            }
        }

        internal bool HasAnyCallback()
        {
            return m_callbacks != null && m_callbacks.Count > 0;
        }
    }

    public class VisualElement<TStyle> : VisualElement where TStyle : IVisualElementStyle, new()
    {
        public new TStyle style => (TStyle)internalStyle;

        public VisualElement() : base()
        {
            internalStyle = new TStyle();
        }
    }

    public enum PickingMode
    {
        Position,
        Ignore
    }

    public abstract class ElementFlags<TEnum> where TEnum : struct, Enum
    {
        public TEnum Value { get; set; }

        public ElementFlags()
        {
            Value = default;
        }

        public ElementFlags(TEnum initialValue)
        {
            Value = initialValue;
        }

        protected void SetFlag(TEnum flag, bool state)
        {
            int mask = Convert.ToInt32(flag);
            int current = Convert.ToInt32(Value);
            if (state)
            {
                current |= mask;
            }
            else
            {
                current &= ~mask;
            }
            Value = (TEnum)Enum.ToObject(typeof(TEnum), current);
        }

        protected bool GetFlag(TEnum flag)
        {
            int mask = Convert.ToInt32(flag);
            int current = Convert.ToInt32(Value);
            return (current & mask) == mask;
        }
    }

    public enum NodeType
    {
        None,
        LogicNode,
        PhysicsNode,
        Invalid
    }
}
