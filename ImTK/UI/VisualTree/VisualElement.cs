using System;
using System.Collections.Generic;
using ImGuiNET;
using ImTK.UI.Style;

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

        public class Style : IVisualElementStyle
        {
            internal List<StyleProperty> m_overrideStyles;
            internal int m_pushedFonts = 0;
            internal float m_previousFontScale = 1.0f;

            public StyleValue<HashedString> fontFamily { set => SetStringToken(StyleKey.FontFamily, value); }
            public StyleValue<int> fontSize { set => SetInt(StyleKey.FontSize, value); }
            public StyleValue<FontSize> fontSizeEnum { set => SetEnum(StyleKey.FontSize, value); }

            public Style() { }

            // --- Low-level Override Setters ---

            public void SetColor(HashedString key, StyleValue<Color> value)
            {
                EnsureOverrideStyles();
                RemoveEntry(key.Hash);

                if (value.IsNull) return;

                var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.ColorValue };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.colorValue = value.Value.u32;

                m_overrideStyles.Add(prop);
            }

            public void SetInt(HashedString key, StyleValue<int> value)
            {
                EnsureOverrideStyles();
                RemoveEntry(key.Hash);

                if (value.IsNull) return;

                var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.IntValue };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.intValue = value.Value;

                m_overrideStyles.Add(prop);
            }


            public void SetEnum<TEnum>(HashedString key, StyleValue<TEnum> value) where TEnum : struct, System.Enum
            {
                EnsureOverrideStyles();
                RemoveEntry(key.Hash);

                if (value.IsNull) return;

                var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.EnumValue };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.enumValue = System.Convert.ToInt32(value.Value);

                m_overrideStyles.Add(prop);
            }

            public void SetStringToken(HashedString key, StyleValue<HashedString> value)
            {
                EnsureOverrideStyles();
                RemoveEntry(key.Hash);

                if (value.IsNull) return;

                var prop = new StyleProperty { key = key.Hash, type = StylePropertyType.Token, tokenHash = value.Value.Hash };
                m_overrideStyles.Add(prop);
            }

            public void SetFloat(HashedString key, StyleValue<float> value)
            {
                EnsureOverrideStyles();
                RemoveEntry(key.Hash);

                if (value.IsNull) return;

                var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.FloatValue };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.floatValue = value.Value;

                m_overrideStyles.Add(prop);
            }

            public void SetVector2(HashedString key, StyleValue<Vector2> value)
            {
                EnsureOverrideStyles();
                RemoveEntry(key.Hash);

                if (value.IsNull) return;

                var prop = new StyleProperty { key = key.Hash, type = value.IsToken ? StylePropertyType.Token : StylePropertyType.Vector2Value };
                if (value.IsToken) prop.tokenHash = value.Token.Hash;
                else prop.vector2Value = value.Value;

                m_overrideStyles.Add(prop);
            }

            // --- Low-level Override Clearers ---

            public void Clear(HashedString key)
            {
                RemoveEntry(key.Hash);
            }

            // --- High-level Property Syntax Sugar ---

            public StyleValue<Color>? backgroundColor
            {
                get => GetOverrideColor(StyleKey.BackgroundColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.BackgroundColor, value.Value);
                    else Clear(StyleKey.BackgroundColor);
                }
            }

            public StyleValue<Color>? textColor
            {
                get => GetOverrideColor(StyleKey.TextColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.TextColor, value.Value);
                    else Clear(StyleKey.TextColor);
                }
            }

            public StyleValue<Color>? disabledTextColor
            {
                get => GetOverrideColor(StyleKey.DisabledTextColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.DisabledTextColor, value.Value);
                    else Clear(StyleKey.DisabledTextColor);
                }
            }

            public StyleValue<Color>? selectionColor
            {
                get => GetOverrideColor(StyleKey.SelectionColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.SelectionColor, value.Value);
                    else Clear(StyleKey.SelectionColor);
                }
            }

            public StyleValue<Color>? borderColor
            {
                get => GetOverrideColor(StyleKey.BorderColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.BorderColor, value.Value);
                    else Clear(StyleKey.BorderColor);
                }
            }

            public StyleValue<Vector2>? padding
            {
                get => GetOverrideVector2(StyleKey.Padding);
                set
                {
                    if (value.HasValue) SetVector2(StyleKey.Padding, value.Value);
                    else Clear(StyleKey.Padding);
                }
            }

            public StyleValue<Vector2>? itemSpacing
            {
                get => GetOverrideVector2(StyleKey.ItemSpacing);
                set
                {
                    if (value.HasValue) SetVector2(StyleKey.ItemSpacing, value.Value);
                    else Clear(StyleKey.ItemSpacing);
                }
            }

            public StyleValue<Vector2>? itemInnerSpacing
            {
                get => GetOverrideVector2(StyleKey.ItemInnerSpacing);
                set
                {
                    if (value.HasValue) SetVector2(StyleKey.ItemInnerSpacing, value.Value);
                    else Clear(StyleKey.ItemInnerSpacing);
                }
            }

            public StyleValue<float>? borderWidth
            {
                get => GetOverrideFloat(StyleKey.BorderWidth);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.BorderWidth, value.Value);
                    else Clear(StyleKey.BorderWidth);
                }
            }

            public StyleValue<float>? borderRadius
            {
                get => GetOverrideFloat(StyleKey.BorderRadius);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.BorderRadius, value.Value);
                    else Clear(StyleKey.BorderRadius);
                }
            }

            public StyleValue<float>? alpha
            {
                get => GetOverrideFloat(StyleKey.Alpha);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.Alpha, value.Value);
                    else Clear(StyleKey.Alpha);
                }
            }

            public StyleValue<float>? disabledAlpha
            {
                get => GetOverrideFloat(StyleKey.DisabledAlpha);
                set
                {
                    if (value.HasValue) SetFloat(StyleKey.DisabledAlpha, value.Value);
                    else Clear(StyleKey.DisabledAlpha);
                }
            }

            // --- Internal Helpers ---

            protected void EnsureOverrideStyles()
            {
                if (m_overrideStyles == null) m_overrideStyles = new List<StyleProperty>();
            }

            protected void RemoveEntry(int keyHash)
            {
                if (m_overrideStyles == null) return;
                for (int i = 0; i < m_overrideStyles.Count; i++)
                {
                    if (m_overrideStyles[i].key == keyHash)
                    {
                        m_overrideStyles.RemoveAt(i);
                        return;
                    }
                }
            }

            protected StyleValue<Color>? GetOverrideColor(HashedString key)
            {
                if (m_overrideStyles == null) return null;
                int keyHash = key.Hash;
                for (int i = 0; i < m_overrideStyles.Count; i++)
                {
                    if (m_overrideStyles[i].key == keyHash)
                    {
                        if (m_overrideStyles[i].isToken)
                            return new StyleValue<Color> { Keyword = StyleKeyword.Undefined };
                        return new StyleValue<Color> { Value = (Color)m_overrideStyles[i].colorValue };
                    }
                }
                return null;
            }

            protected StyleValue<float>? GetOverrideFloat(HashedString key)
            {
                if (m_overrideStyles == null) return null;
                int keyHash = key.Hash;
                for (int i = 0; i < m_overrideStyles.Count; i++)
                {
                    if (m_overrideStyles[i].key == keyHash)
                    {
                        if (m_overrideStyles[i].isToken)
                            return new StyleValue<float> { Keyword = StyleKeyword.Undefined };
                        return new StyleValue<float> { Value = m_overrideStyles[i].floatValue };
                    }
                }
                return null;
            }

            protected StyleValue<Vector2>? GetOverrideVector2(HashedString key)
            {
                if (m_overrideStyles == null) return null;
                int keyHash = key.Hash;
                for (int i = 0; i < m_overrideStyles.Count; i++)
                {
                    if (m_overrideStyles[i].key == keyHash)
                    {
                        if (m_overrideStyles[i].isToken)
                            return new StyleValue<Vector2> { Keyword = StyleKeyword.Undefined };
                        return new StyleValue<Vector2> { Value = m_overrideStyles[i].vector2Value };
                    }
                }
                return null;
            }

            // --- IVisualElementStyle Implementation ---

            private int m_pushedColors = 0;
            private int m_pushedVars = 0;

            public virtual void PushToImGui(ResolvedStyle resolvedStyle)
            {
                m_pushedColors = 0;
                m_pushedVars = 0;
                m_pushedFonts = 0;

                int? familyHash = resolvedStyle.GetTokenHash(StyleKey.FontFamily);
                int? sizePixel = resolvedStyle.GetInt(StyleKey.FontSize);
                int? sizeEnum = resolvedStyle.GetEnum(StyleKey.FontSize);

                if (familyHash.HasValue || sizePixel.HasValue || sizeEnum.HasValue)
                {
                    int finalFamilyHash = familyHash.HasValue ? familyHash.Value : ImTKFontManager.DefaultFontFamilyHash;

                    if (sizePixel.HasValue)
                    {
                        var (font, scale) = ImTKFontManager.GetFontWithScale(finalFamilyHash, sizePixel.Value);
                        ImGui.PushFont(font);
                        ImTKFontManager.PushFontScale(scale);
                        m_pushedFonts++;
                    }
                    else if (sizeEnum.HasValue)
                    {
                        var font = ImTKFontManager.GetFont(finalFamilyHash, (ImTK.UI.Style.FontSize)sizeEnum.Value);
                        ImGui.PushFont(font);
                        ImTKFontManager.PushFontScale(ImTKFontManager.CurrentFontScale); // Default scale for exact matched enum
                        m_pushedFonts++;
                    }
                    else
                    {
                        var font = ImTKFontManager.GetFont(finalFamilyHash, ImTK.UI.Style.FontSize.Normal);
                        ImGui.PushFont(font);
                        ImTKFontManager.PushFontScale(ImTKFontManager.CurrentFontScale);
                        m_pushedFonts++;
                    }
                }

                Color? bgColor = resolvedStyle.GetColor(StyleKey.BackgroundColor);
                if (bgColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor.Value.u32);
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, bgColor.Value.u32);
                    m_pushedColors += 2;
                }

                Color? textColor = resolvedStyle.GetColor(StyleKey.TextColor);
                if (textColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.u32);
                    m_pushedColors++;
                }

                Color? disabledTextColor = resolvedStyle.GetColor(StyleKey.DisabledTextColor);
                if (disabledTextColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.TextDisabled, disabledTextColor.Value.u32);
                    m_pushedColors++;
                }

                Color? selectionColor = resolvedStyle.GetColor(StyleKey.SelectionColor);
                if (selectionColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.TextSelectedBg, selectionColor.Value.u32);
                    m_pushedColors++;
                }

                Color? borderColor = resolvedStyle.GetColor(StyleKey.BorderColor);
                if (borderColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.Border, borderColor.Value.u32);
                    m_pushedColors++;
                }

                float? borderRadius = resolvedStyle.GetFloat(StyleKey.BorderRadius);
                if (borderRadius.HasValue)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, borderRadius.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, borderRadius.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, borderRadius.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, borderRadius.Value);
                    m_pushedVars += 4;
                }

                float? borderWidth = resolvedStyle.GetFloat(StyleKey.BorderWidth);
                if (borderWidth.HasValue)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, borderWidth.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, borderWidth.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, borderWidth.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, borderWidth.Value);
                    m_pushedVars += 4;
                }

                Vector2? padding = resolvedStyle.GetVector2(StyleKey.Padding);
                if (padding.HasValue)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, padding.Value);
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, padding.Value);
                    m_pushedVars += 2;
                }

                Vector2? itemSpacing = resolvedStyle.GetVector2(StyleKey.ItemSpacing);
                if (itemSpacing.HasValue)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, itemSpacing.Value);
                    m_pushedVars++;
                }

                Vector2? itemInnerSpacing = resolvedStyle.GetVector2(StyleKey.ItemInnerSpacing);
                if (itemInnerSpacing.HasValue)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, itemInnerSpacing.Value);
                    m_pushedVars++;
                }

                float? alpha = resolvedStyle.GetFloat(StyleKey.Alpha);
                if (alpha.HasValue)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha.Value);
                    m_pushedVars++;
                }
            }

            public virtual void PopFromImGui()
            {
                if (m_pushedColors > 0) ImGui.PopStyleColor(m_pushedColors);
                if (m_pushedVars > 0) ImGui.PopStyleVar(m_pushedVars);

                if (m_pushedFonts > 0)
                {
                    ImGui.PopFont();
                    ImTKFontManager.PopFontScale();
                }

                m_pushedColors = 0;
                m_pushedVars = 0;
                m_pushedFonts = 0;
            }
        }

        private static int s_elementCounter = 0;
        protected readonly int m_elementId;

        public VisualElementHierarchy hierarchy { get; }
        public virtual VisualElement contentContainer => this;

        public VisualElement parent { get; internal set; }

        public PickingMode pickingMode { get; set; } = PickingMode.Position;
        protected bool m_wasHovered = false;
        protected bool m_useAutoId = true;

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
        public ResolvedStyle resolvedStyle { get; }


        public VisualElement()
        {
            m_elementId = ++s_elementCounter;
            internalStyle = new Style();
            resolvedStyle = new ResolvedStyle(this);
            hierarchy = new VisualElementHierarchy(this);

            classList = new StyleClass();
            classList.OnClassChanged = MarkStyleDirty;
        }

        public void MarkStyleDirty()
        {
            m_isStyleDirty = true;
            // Children's cascaded styles might need update
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
                return ImTKTheme.GlobalTheme; // Root fallback
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

        // ApplyTheme is removed as Theme token resolution is now dynamically handled in ComputeStyle

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

            // Collect children to avoid modifying while iterating, just in case physical Clear has side effects
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

        // REMOVED 'virtual' to ensure protection. Kept 'public' temporarily for Test access, or use InternalsVisibleTo.
        // As requested before, we'll keep it 'internal' but we need the Test module to access it.
        /// <summary>
        /// 觸發元件渲染的公開入口點。
        /// 負責處理防護層邏輯 (PushID/PopID、MouseHover推導、事件分派)，並呼叫 OnRenderLayout。
        /// 不可被覆寫，子類別應實作 OnRenderLayout 或 OnRenderSelf。
        /// </summary>
        public void Render()
        {
            if (m_isStyleDirty)
            {
                resolvedStyle.Compute();
                m_isStyleDirty = false;
            }

            if (m_useAutoId)
            {
                ImGui.PushID(m_elementId);
            }

            if (pickingMode == PickingMode.Ignore)
            {
                ImGui.SetNextItemAllowOverlap();
            }

            internalStyle.PushToImGui(resolvedStyle);

            OnRenderLayout();

            bool isSelfHovered = false;

            if (pickingMode != PickingMode.Ignore)
            {
                isSelfHovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
            }

            bool isAnyChildHovered = false;
            int count = hierarchy.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = hierarchy.childAt(i);
                if (child.m_wasHovered)
                {
                    isAnyChildHovered = true;
                }
            }

            bool isEffectivelyHovered = isSelfHovered || isAnyChildHovered;

            if (isEffectivelyHovered && !m_wasHovered)
            {
                var evt = EventPool<MouseEnterEvent>.Get();
                evt.source = this;
                EventDispatcher.Enqueue(evt);
            }
            else if (!isEffectivelyHovered && m_wasHovered)
            {
                var evt = EventPool<MouseLeaveEvent>.Get();
                evt.source = this;
                EventDispatcher.Enqueue(evt);
            }

            m_wasHovered = isEffectivelyHovered;

            internalStyle.PopFromImGui();

            if (m_useAutoId)
            {
                ImGui.PopID();
            }
        }

        protected virtual void OnRenderLayout()
        {
            OnRenderSelf();

            int count = hierarchy.childCount;
            for (int i = 0; i < count; i++)
            {
                hierarchy.childAt(i).Render();
            }
        }

        protected virtual void OnRenderSelf()
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
}

namespace ImTK.UI
{
    public class VisualElement<TStyle> : VisualElement where TStyle : ImTK.UI.Style.IVisualElementStyle, new()
    {
        public new TStyle style => (TStyle)internalStyle;

        public VisualElement() : base()
        {
            internalStyle = new TStyle();
        }
    }
}
