namespace ImTK.Core
{
    /// <summary>
    /// Represents the precise execution phase of the ImTK Application.
    /// Used to enforce strict lifecycle ordering and prevent reentrancy.
    /// </summary>
    public enum ApplicationState
    {
        Uninitialized,

        // --- Initialization Phase ---
        InitializeSelf,
        InitializeDependencies,

        /// <summary>
        /// The application has initialized modules but is waiting for the graphics context to be created.
        /// </summary>
        AwaitingGraphicsSetup,
        GraphicsSetup,

        // --- Runtime Loop Phase ---
        /// <summary>
        /// The application is ready and waiting for the next lifecycle command.
        /// </summary>
        Idle,
        LogicUpdate,
        GuiRender,
        GizmoRender,
        LateUpdate,

        // --- Teardown Phase ---
        Close,
        Closed
    }
}
