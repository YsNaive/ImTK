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
        internal bool m_activeInHierarchy = true;

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
