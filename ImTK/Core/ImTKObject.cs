namespace ImTK.Core
{
    /// <summary>
    /// The base class for dynamically created runtime logic objects.
    /// Objects inherit this class and are automatically registered into the application's lifecycle.
    /// </summary>
    public abstract class ImTKObject
    {
        internal bool m_enabled = true;
        internal bool m_activeInHierarchy = false; // Starts false until pending add is processed

        /// <summary>
        /// Controls whether this object participates in the update and render loops.
        /// Changing this value defers the actual OnEnable/OnDisable trigger to the LateUpdate phase.
        /// </summary>
        public bool enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }

        /// <summary>
        /// True if this object has been marked for destruction.
        /// </summary>
        public bool isDestroyed { get; private set; } = false;

        // 儲存取消訂閱的 Action，供 InternalOnDisable 使用
        private readonly System.Collections.Generic.List<System.Action> m_eventUnsubscribers = new System.Collections.Generic.List<System.Action>();
        // 儲存訂閱函式（Func<Action> 執行後回傳 unsub），供 InternalOnEnable 重新訂閱
        private readonly System.Collections.Generic.List<System.Func<System.Action>> m_subscribeActions = new System.Collections.Generic.List<System.Func<System.Action>>();

        /// <summary>
        /// Constructs a new ImTKObject and automatically registers it to the application lifecycle.
        /// </summary>
        protected ImTKObject()
        {
            ImTKApplication.RegisterObject(this);
        }

        /// <summary>
        /// Safely subscribes to a global application event.
        /// The subscription is automatically managed by the enable/disable cycle:
        /// it is activated on InternalOnEnable and deactivated on InternalOnDisable, and will be re-activated on re-enable.
        /// Note: The actual subscription does NOT take effect until the first InternalOnEnable() call (next LateUpdate after construction).
        /// </summary>
        protected void SubscribeEvent<T>(System.Action<T> handler) where T : ImTK.Event.IImTKEvent
        {
            if (m_activeInHierarchy)
            {
                throw new System.InvalidOperationException($"SubscribeEvent for {typeof(T).Name} cannot be called after the object is enabled. Please call it in the constructor.");
            }
            // 只儲存訂閱函式，不立即呼叫 GlobalSubscribe。
            // 實際訂閱會在 InternalOnEnable() 時才生效，與 Object 的 active 狀態同步。
            System.Func<System.Action> subscribe = () => ImTK.Event.ImTKEventBus.GlobalSubscribe(handler);
            m_subscribeActions.Add(subscribe);
        }

        /// <summary>
        /// Marks the object for destruction.
        /// It will be unregistered and OnDestroy will be called during the LateUpdate phase.
        /// </summary>
        public void Destroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;
            ImTKApplication.UnregisterObject(this);
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

        // --- Runtime Loop Phase ---

        protected internal virtual void OnEnable() { }

        protected internal virtual void OnDisable() { }

        protected internal virtual void OnLogicUpdate() { }
        protected internal virtual void OnGuiRender() { }
        protected internal virtual void OnGizmoRender() { }
        protected internal virtual void OnLateUpdate() { }

        // --- Teardown Phase ---

        /// <summary>
        /// Triggered when the object is fully destroyed and removed from the active loop.
        /// </summary>
        protected internal virtual void OnDestroy() { }
    }
}
