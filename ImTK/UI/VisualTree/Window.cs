using System;
using ImGuiNET;
using ImTK.Log;

namespace ImTK.UI
{
    public abstract class Window : VisualElement
    {
        private static readonly LogContext s_log = new LogContext("Window");

        public string displayName { get; protected set; }
        public string windowId { get; protected set; }

        internal string imguiId => string.IsNullOrEmpty(windowId) ? displayName : $"{displayName}###{windowId}";

        protected bool m_isOpen = false;
        public ImGuiWindowFlags windowFlags { get; set; } = ImGuiWindowFlags.None;

        protected Window(string displayName, string windowId = "")
        {
            m_useAutoId = false;
            this.displayName = displayName;
            this.windowId = windowId;
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
            bool isExpanded = ImGui.Begin(imguiId, ref m_isOpen, windowFlags);

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

            if (!m_isOpen)
            {
                Close();
            }
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public virtual void Update() { }
    }
}
