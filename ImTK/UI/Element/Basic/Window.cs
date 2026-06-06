using System;
using Hexa.NET.ImGui;
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
        /// <summary>
        /// 是否隱藏右上角的關閉按鈕 (X)。此非 ImGui 原生 Flag，而是透過是否傳入 isOpen 參數來控制。
        /// </summary>
        public bool noClose { get; set; } = false;

        /// <summary>
        /// 若為 true，此視窗將不會被加入自動復原的持久化清單中。適合用於暫時性的 Dialog 或報表視窗。
        /// </summary>
        public bool dontSaveOpenState { get; set; } = false;
    }

    public abstract class Window : VisualElement<Window.Style>, ILayoutRoot, IRenderRoot
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString TitleBarColor = new HashedString("TitleBarColor");
            public static readonly HashedString TitleBarActiveColor = new HashedString("TitleBarActiveColor");
            public static readonly HashedString TitleBarCollapsedColor = new HashedString("TitleBarCollapsedColor");
        }

        public new class Style : VisualElement.Style
        {






            public StyleColor? titleBarColor
            {
                get => GetPropertyColor(StyleKey.TitleBarColor);
                set => SetPropertyColor(StyleKey.TitleBarColor, value);
            }

            public StyleColor? titleBarActiveColor
            {
                get => GetPropertyColor(StyleKey.TitleBarActiveColor);
                set => SetPropertyColor(StyleKey.TitleBarActiveColor, value);
            }

            public StyleColor? titleBarCollapsedColor
            {
                get => GetPropertyColor(StyleKey.TitleBarCollapsedColor);
                set => SetPropertyColor(StyleKey.TitleBarCollapsedColor, value);
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



        public string displayName { get; protected set; }
        
        public string windowId { get; init; }

        public override string persistenceKey 
        {
            get
            {
                string key = base.persistenceKey;
                if (!string.IsNullOrEmpty(key)) return key;
                if (!string.IsNullOrEmpty(windowId)) return windowId;
                return this.GetType().Name;
            }
            set => base.persistenceKey = value;
        }

        public bool isFocused { get; private set; }
        
        public static System.Collections.Generic.IReadOnlyList<Window> activeWindows => Panel.ActiveWindows;

        public float CurrentDpiScale { get; private set; } = 1.0f;

        internal string imguiId => string.IsNullOrEmpty(windowId) ? displayName : $"{displayName}###{windowId}";

        private IntPtr m_imguiIdPtr = IntPtr.Zero;
        internal IntPtr imguiIdPtr
        {
            get
            {
                if (m_imguiIdPtr == IntPtr.Zero)
                {
                    m_imguiIdPtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(imguiId);
                }
                return m_imguiIdPtr;
            }
        }

        protected bool m_isOpen = false;
        public bool isOpen => m_isOpen;

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

            ImTKLog.Trace($"Opening window: {imguiId}");
            Panel.RegisterWindow(this);
            m_isOpen = true;
            OnEnable();
        }

        public void Close()
        {
            if (!m_isOpen) return;

            ImTKLog.Trace($"Closing window: {imguiId}");
            OnDisable();
            m_isOpen = false;
            Panel.UnregisterWindow(this);
        }

        public static T Open<T>(string windowId = "") where T : Window, new()
        {
            WindowKey key = new WindowKey(typeof(T), windowId);

            if (Panel.TryGetWindow(key, out Window existingWindow))
            {
                ImTKLog.Trace($"Window '{key.WindowId}' of type {key.Type.Name} already open. Focusing.");
                if (existingWindow.m_hasRenderedAtLeastOnce)
                {
                    ImGui.SetWindowFocus(existingWindow.imguiId);
                }
                return (T)existingWindow;
            }

            ImTKLog.Debug($"Creating new window instance for type {typeof(T).Name} with ID '{windowId}'.");
            T newWindow = string.IsNullOrEmpty(windowId) ? new T() : new T { windowId = windowId };

            newWindow.Open();
            return newWindow;
        }

        public static Window Open(Type windowType, string windowId = "")
        {
            if (!typeof(Window).IsAssignableFrom(windowType))
            {
                ImTKLog.Error($"Type {windowType.Name} is not a Window.");
                return null;
            }

            WindowKey key = new WindowKey(windowType, windowId);

            if (Panel.TryGetWindow(key, out Window existingWindow))
            {
                ImTKLog.Trace($"Window '{key.WindowId}' of type {key.Type.Name} already open. Focusing.");
                if (existingWindow.m_hasRenderedAtLeastOnce)
                {
                    ImGui.SetWindowFocus(existingWindow.imguiId);
                }
                return existingWindow;
            }

            ImTKLog.Debug($"Creating new window instance for type {windowType.Name} with ID '{windowId}'.");
            Window newWindow = (Window)Activator.CreateInstance(windowType);
            if (!string.IsNullOrEmpty(windowId))
            {
                var prop = typeof(Window).GetProperty(nameof(Window.windowId));
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(newWindow, windowId);
                }
            }

            newWindow.Open();
            return newWindow;
        }

        private bool m_didApplyLocalTheme = false;
        private bool m_hasRenderedAtLeastOnce = false;

        private bool m_pushedFontForBegin = false;

        protected virtual bool Begin(ref bool isOpenForImGui, ImGuiWindowFlags windowFlags)
        {
            // 若此視窗有局部 theme，在 ImGui.Begin() 前臨時將其套用為全域 style。
            // 這是因為 ImGui Docking 在繪製 DockNode 背景時讀取的是全域 style，
            // 不受 PushStyleColor 的影響，必須在 Begin 前修改全域 style 才能正確顯示。
            if (m_theme != null)
            {
                m_theme.ApplyToImGui();
                m_didApplyLocalTheme = true;
            }

            int globalFontFamilyHash = ImTKTheme.GlobalTheme.fontFamilyHash;
            var font = ImTKFontManager.GetFont(globalFontFamilyHash);
            
            unsafe
            {
                if (font.Handle != null)
                {
                    if (globalFontFamilyHash != ImTKFontManager.DefaultFontFamilyHash)
                    {
                        ImGui.PushFont((Hexa.NET.ImGui.ImFont*)font.Handle, ((Hexa.NET.ImGui.ImFont*)font.Handle)->LegacySize);
                        RenderEngine.Context.PushFontState(globalFontFamilyHash);
                        m_pushedFontForBegin = true;
                    }
                }
            }

            bool isExpanded;
            unsafe
            {
                if (!flags.noClose)
                {
                    isExpanded = ImGui.Begin((byte*)imguiIdPtr, ref isOpenForImGui, windowFlags);
                }
                else
                {
                    isExpanded = ImGui.Begin((byte*)imguiIdPtr, windowFlags);
                }
            }

            float newDpiScale = ImGui.GetWindowViewport().DpiScale;

            if (this.CurrentDpiScale != newDpiScale)
            {
                this.CurrentDpiScale = newDpiScale;
                this.MarkStyleDirty();
            }

            RenderEngine.Context.CurrentDpiScale = newDpiScale;
            RenderEngine.Context.IsInsideWindow = true;
            RenderEngine.Context.FlushPendingCommands();
            return isExpanded;
        }

        protected virtual void End()
        {
            ImGui.End();

            if (m_pushedFontForBegin)
            {
                ImGui.PopFont();
                RenderEngine.Context.PopFontState();
                m_pushedFontForBegin = false;
            }

            // 若曾臨時切換全域 style，在 End 後立即還原，確保後續視窗使用正確的全域 style。
            if (m_didApplyLocalTheme)
            {
                ImTKTheme.GlobalTheme.ApplyToImGui();
                m_didApplyLocalTheme = false;
            }

            RenderEngine.Context.IsInsideWindow = false;
        }


        private bool m_isOpenForImGuiCache;

        public override bool OnBeginRender()
        {
            m_isOpenForImGuiCache = m_isOpen;
            bool isExpanded = Begin(ref m_isOpenForImGuiCache, flags.Value);
            m_hasRenderedAtLeastOnce = true;

            if (isExpanded)
            {
                var avail = ImGui.GetContentRegionAvail();
                var startPos = ImGui.GetCursorScreenPos();
                
                var constraint = new LayoutConstraint(avail.X, avail.Y, MeasureMode.Exactly, MeasureMode.Exactly);
                this.Measure(constraint);
                this.Arrange(new Rect(startPos.X, startPos.Y, avail.X, avail.Y));
            }

            return isExpanded;
        }

        private bool m_cachedWindowHovered;

        public override void OnEndRender()
        {
            m_cachedWindowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
            End();

            if (!m_isOpenForImGuiCache && m_isOpen)
            {
                Close();
            }
        }

        protected internal override bool CheckHoverState()
        {
            return m_cachedWindowHovered;
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
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
