using System;
using ImGuiNET;
using ImTK.Log;
using ImTK.UI.Style;
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

    public abstract class Window : VisualElement<Window.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString TitleBg = new HashedString("TitleBg");
        }

        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);

                m_pushedColors = 0;

                Color? titleBg = resolvedStyle.GetColor(Window.StyleKey.TitleBg);
                if (titleBg.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.TitleBg, titleBg.Value.u32);
                    ImGui.PushStyleColor(ImGuiCol.TitleBgActive, titleBg.Value.u32);
                    m_pushedColors += 2;
                }
            }

            public override void PopFromImGui()
            {
                if (m_pushedColors > 0)
                {
                    ImGui.PopStyleColor(m_pushedColors);
                    m_pushedColors = 0;
                }
                base.PopFromImGui();
            }

            public StyleValue<Color>? titleBg
            {
                get => GetOverrideColor(StyleKey.TitleBg);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.TitleBg, value.Value);
                    else Clear(StyleKey.TitleBg);
                }
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

        protected override void OnRenderLayout()
        {
            bool isOpenForImGui = m_isOpen;
            bool isExpanded = ImGui.Begin(imguiId, ref isOpenForImGui, flags.Value);

            if (isExpanded)
            {
                OnRenderSelf();

                int count = hierarchy.childCount;
                for (int i = 0; i < count; i++)
                {
                    hierarchy.childAt(i).Render();
                }
            }

            ImGui.End();

            if (!isOpenForImGui && m_isOpen)
            {
                Close();
            }
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public virtual void Update() { }
    }
}
