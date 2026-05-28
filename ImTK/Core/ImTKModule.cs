namespace ImTK.Core
{
    /// <summary>
    /// The base class for all system-level singletons in the ImTK framework.
    /// Modules are strictly instantiated via reflection by ImTKApplication on startup.
    /// </summary>
    public abstract class ImTKModule
    {
        // Internal state tracked by the Application
        internal bool m_enabled = true;
        internal bool m_activeInHierarchy = false;

        // 儲存取消訂閱的 Action，供 InternalOnDisable 使用
        private readonly System.Collections.Generic.List<System.Action> m_eventUnsubscribers = new System.Collections.Generic.List<System.Action>();
        // 儲存訂閱函式（Func<Action> 執行後回傳 unsub），供 InternalOnEnable 重新訂閱
        private readonly System.Collections.Generic.List<System.Func<System.Action>> m_subscribeActions = new System.Collections.Generic.List<System.Func<System.Action>>();

        /// <summary>
        /// Controls whether this module participates in the update and render loops.
        /// Changing this value defers the actual OnEnable/OnDisable trigger to the LateUpdate phase.
        /// </summary>
        public bool enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }

        /// <summary>
        /// Protected constructor to enforce the rule that modules can only be instantiated by the framework.
        /// Child classes MUST NOT define public or parameterized constructors.
        /// </summary>
        protected ImTKModule() { }

        /// <summary>
        /// Safely subscribes to a global application event.
        /// The subscription is automatically managed by the enable/disable cycle:
        /// it is activated on OnEnable and deactivated on OnDisable, and will be re-activated on re-enable.
        /// Note: The actual subscription does NOT take effect until the first InternalOnEnable() call.
        /// </summary>
        protected void SubscribeEvent<T>(System.Action<T> handler) where T : ImTK.Event.IImTKEvent
        {
            // 只儲存訂閱函式，不立即呼叫 GlobalSubscribe。
            // 實際訂閱會在 InternalOnEnable() 時才生效，與 Module 的 active 狀態同步。
            System.Func<System.Action> subscribe = () => ImTK.Event.ImTKEventBus.GlobalSubscribe(handler);
            m_subscribeActions.Add(subscribe);
        }

        // --- Framework-Internal Lifecycle Wrappers ---
        // 由 ImTKApplication 呼叫，子類不應直接覆寫這些方法。
        // 子類應覆寫 OnEnable() / OnDisable() 以實作自訂邏輯。

        internal void InternalOnEnable()
        {
            // 重新訂閱所有已登記的事件（不可被子類 override 跳過）
            foreach (var subscribe in m_subscribeActions)
            {
                m_eventUnsubscribers.Add(subscribe());
            }
            OnEnable();
        }

        internal void InternalOnDisable()
        {
            OnDisable();
            // 取消所有訂閱（不可被子類 override 跳過）
            foreach (var unsub in m_eventUnsubscribers)
            {
                unsub?.Invoke();
            }
            m_eventUnsubscribers.Clear();
        }

        // --- Initialization Phase ---

        /// <summary>
        /// Phase 1: Setup internal state. Do NOT access other modules here.
        /// </summary>
        protected internal virtual void OnInitializeSelf() { }

        /// <summary>
        /// Phase 2: Setup dependencies. Safe to access other modules via ImTKApplication.GetModule.
        /// </summary>
        protected internal virtual void OnInitializeDependencies() { }

        /// <summary>
        /// Phase 3: Setup graphics resources (e.g., loading textures).
        /// Triggered after the window/ImGui context is created.
        /// </summary>
        protected internal virtual void OnGraphicsSetup() { }

        // --- Runtime Loop Phase ---

        protected internal virtual void OnLogicUpdate() { }
        protected internal virtual void OnGuiRender() { }
        protected internal virtual void OnGizmoRender() { }
        protected internal virtual void OnLateUpdate() { }

        // --- State Changes & Teardown ---

        protected internal virtual void OnEnable() { }

        protected internal virtual void OnDisable() { }

        /// <summary>
        /// Triggered when the entire application is closing.
        /// Use this to free unmanaged resources (like ImGui pointers or OpenGL textures).
        /// </summary>
        protected internal virtual void OnClose() { }
    }
}
