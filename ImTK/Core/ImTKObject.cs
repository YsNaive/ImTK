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

        private readonly System.Collections.Generic.List<System.Action> m_eventUnsubscribers = new System.Collections.Generic.List<System.Action>();

        /// <summary>
        /// Constructs a new ImTKObject and automatically registers it to the application lifecycle.
        /// </summary>
        protected ImTKObject()
        {
            ImTKApplication.RegisterObject(this);
        }

        /// <summary>
        /// Safely subscribes to a global application event.
        /// The subscription is automatically cleared when this object is disabled or destroyed.
        /// </summary>
        protected void SubscribeEvent<T>(System.Action<T> handler) where T : ImTK.Event.IImTKEvent
        {
            System.Action unsub = ImTK.Event.ImTKEventBus.GlobalSubscribe(handler);
            m_eventUnsubscribers.Add(unsub);
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

        // --- Runtime Loop Phase ---

        protected internal virtual void OnEnable() { }

        protected internal virtual void OnDisable()
        {
            foreach (var unsub in m_eventUnsubscribers)
            {
                unsub?.Invoke();
            }
            m_eventUnsubscribers.Clear();
        }

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
