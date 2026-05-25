using System;
using ImGuiNET;
using ImTK.Log;
using ImTK.Core;

namespace ImTK.UI
{
    public class WindowFlags : ElementFlags<ImGuiWindowFlags>
    {
        public bool noTitleBar { get => GetFlag(ImGuiWindowFlags.NoTitleBar); set => SetFlag(ImGuiWindowFlags.NoTitleBar, value); }
        public bool noResize { get => GetFlag(ImGuiWindowFlags.NoResize); set => SetFlag(ImGuiWindowFlags.NoResize, value); }
        public bool noMove { get => GetFlag(ImGuiWindowFlags.NoMove); set => SetFlag(ImGuiWindowFlags.NoMove, value); }
        public bool noScrollbar { get => GetFlag(ImGuiWindowFlags.NoScrollbar); set => SetFlag(ImGuiWindowFlags.NoScrollbar, value); }
        public bool noScrollWithMouse { get => GetFlag(ImGuiWindowFlags.NoScrollWithMouse); set => SetFlag(ImGuiWindowFlags.NoScrollWithMouse, value); }
        public bool noCollapse { get => GetFlag(ImGuiWindowFlags.NoCollapse); set => SetFlag(ImGuiWindowFlags.NoCollapse, value); }
        public bool alwaysAutoResize { get => GetFlag(ImGuiWindowFlags.AlwaysAutoResize); set => SetFlag(ImGuiWindowFlags.AlwaysAutoResize, value); }
        public bool noBackground { get => GetFlag(ImGuiWindowFlags.NoBackground); set => SetFlag(ImGuiWindowFlags.NoBackground, value); }
        public bool noSavedSettings { get => GetFlag(ImGuiWindowFlags.NoSavedSettings); set => SetFlag(ImGuiWindowFlags.NoSavedSettings, value); }
        public bool noMouseInputs { get => GetFlag(ImGuiWindowFlags.NoMouseInputs); set => SetFlag(ImGuiWindowFlags.NoMouseInputs, value); }
        public bool menuBar { get => GetFlag(ImGuiWindowFlags.MenuBar); set => SetFlag(ImGuiWindowFlags.MenuBar, value); }
        public bool horizontalScrollbar { get => GetFlag(ImGuiWindowFlags.HorizontalScrollbar); set => SetFlag(ImGuiWindowFlags.HorizontalScrollbar, value); }
        public bool noFocusOnAppearing { get => GetFlag(ImGuiWindowFlags.NoFocusOnAppearing); set => SetFlag(ImGuiWindowFlags.NoFocusOnAppearing, value); }
        public bool noBringToFrontOnFocus { get => GetFlag(ImGuiWindowFlags.NoBringToFrontOnFocus); set => SetFlag(ImGuiWindowFlags.NoBringToFrontOnFocus, value); }
        public bool alwaysVerticalScrollbar { get => GetFlag(ImGuiWindowFlags.AlwaysVerticalScrollbar); set => SetFlag(ImGuiWindowFlags.AlwaysVerticalScrollbar, value); }
        public bool alwaysHorizontalScrollbar { get => GetFlag(ImGuiWindowFlags.AlwaysHorizontalScrollbar); set => SetFlag(ImGuiWindowFlags.AlwaysHorizontalScrollbar, value); }
        public bool noNavInputs { get => GetFlag(ImGuiWindowFlags.NoNavInputs); set => SetFlag(ImGuiWindowFlags.NoNavInputs, value); }
        public bool noNavFocus { get => GetFlag(ImGuiWindowFlags.NoNavFocus); set => SetFlag(ImGuiWindowFlags.NoNavFocus, value); }
        public bool unsavedDocument { get => GetFlag(ImGuiWindowFlags.UnsavedDocument); set => SetFlag(ImGuiWindowFlags.UnsavedDocument, value); }
        public bool noDocking { get => GetFlag(ImGuiWindowFlags.NoDocking); set => SetFlag(ImGuiWindowFlags.NoDocking, value); }
    }

    public abstract class Window : VisualElement<Window.Style>, IWindow
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString TitleBarColor = new HashedString("TitleBarColor");
            public static readonly HashedString TitleBarActiveColor = new HashedString("TitleBarActiveColor");
            public static readonly HashedString TitleBarCollapsedColor = new HashedString("TitleBarCollapsedColor");
        }

        public new class Style : VisualElement.Style
        {






            public StyleValue<Color>? titleBarColor
            {
                get => GetPropertyColor(StyleKey.TitleBarColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.TitleBarColor, value.Value);
                    else Clear(StyleKey.TitleBarColor);
                }
            }

            public StyleValue<Color>? titleBarActiveColor
            {
                get => GetPropertyColor(StyleKey.TitleBarActiveColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.TitleBarActiveColor, value.Value);
                    else Clear(StyleKey.TitleBarActiveColor);
                }
            }

            public StyleValue<Color>? titleBarCollapsedColor
            {
                get => GetPropertyColor(StyleKey.TitleBarCollapsedColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.TitleBarCollapsedColor, value.Value);
                    else Clear(StyleKey.TitleBarCollapsedColor);
                }
            }

            public override void ComputeHighlevelToken(StyleProperty prop, System.Collections.Generic.IList<StyleProperty> output)
            {
                if (prop.category == StyleCategory.HighLevelToken)
                {
                    if (prop.key == StyleKey.TitleBarColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.TitleBg;
                        output.Add(prop);
                        return;
                    }
                    else if (prop.key == StyleKey.TitleBarActiveColor.Hash)
                    {
                        prop.category = prop.dataType == StyleDataType.HashedString ? StyleCategory.ThemeToken : StyleCategory.ImGuiStyle;
                        prop.key = (int)ImGuiCol.TitleBgActive;
                        output.Add(prop);
                        return;
                    }
                }
                base.ComputeHighlevelToken(prop, output);
            }
}

        private static readonly LogContext s_log = new LogContext("Window");

        public string displayName { get; protected set; }
        public string windowId { get; protected set; }

        internal string imguiId => string.IsNullOrEmpty(windowId) ? displayName : $"{displayName}###{windowId}";

        protected bool m_isOpen = false;

        [Obsolete("Please use the 'flags' property syntax sugar instead.")]
        public ImGuiWindowFlags windowFlags { get => flags.Value; set => flags.Value = value; }

        public WindowFlags flags { get; } = new WindowFlags();

        protected Window(string displayName, string windowId = "")
        {
            m_useAutoId = false;
            this.displayName = displayName;
            this.windowId = windowId;
            classList.Add("Window");
        }

        public void Open()
        {
            if (m_isOpen) return;

            s_log.Trace($"Opening window: {imguiId}");
            Panel.RegisterWindow(this);
            m_isOpen = true;
            OnEnable();
        }

        public void Close()
        {
            if (!m_isOpen) return;

            s_log.Trace($"Closing window: {imguiId}");
            OnDisable();
            m_isOpen = false;
            Panel.UnregisterWindow(this);
        }

        public static T Open<T>(string windowId = "") where T : Window, new()
        {
            WindowKey key = new WindowKey(typeof(T), windowId);

            if (Panel.TryGetWindow(key, out Window existingWindow))
            {
                s_log.Trace($"Window '{key.WindowId}' of type {key.Type.Name} already open. Focusing.");
                ImGui.SetWindowFocus(existingWindow.imguiId);
                return (T)existingWindow;
            }

            s_log.Debug($"Creating new window instance for type {typeof(T).Name} with ID '{windowId}'.");
            T newWindow = new T();
            if (!string.IsNullOrEmpty(windowId))
            {
                newWindow.windowId = windowId;
            }

            newWindow.Open();
            return newWindow;
        }

        protected virtual bool Begin(ref bool isOpenForImGui, ImGuiWindowFlags windowFlags)
        {
            bool isExpanded = ImGui.Begin(imguiId, ref isOpenForImGui, windowFlags);
            RenderingContext.IsInsideWindow = true;
            RenderingContext.FlushPendingCommands();
            return isExpanded;
        }

        protected virtual void End()
        {
            ImGui.End();
            RenderingContext.IsInsideWindow = false;
        }

        private bool m_isOpenForImGuiCache;

        public override bool OnBeginRender()
        {
            m_isOpenForImGuiCache = m_isOpen;
            bool isExpanded = Begin(ref m_isOpenForImGuiCache, flags.Value);
            return isExpanded;
        }

        public override void OnEndRender()
        {
            End();

            if (!m_isOpenForImGuiCache && m_isOpen)
            {
                Close();
            }
        }

        protected internal override bool CheckHoverState()
        {
            return ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public virtual void Update() { }
    }

    internal struct WindowKey : IEquatable<WindowKey>
        {
            public Type Type;
            public string WindowId;

            public WindowKey(Type type, string windowId)
            {
                Type = type;
                WindowId = windowId;
            }

            public bool Equals(WindowKey other)
            {
                return Type == other.Type && WindowId == other.WindowId;
            }

            public override bool Equals(object obj)
            {
                return obj is WindowKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
                    hash = hash * 23 + (WindowId != null ? WindowId.GetHashCode() : 0);
                    return hash;
                }
            }
        }
}
