namespace ImTK.Silk
{
    /// <summary>
    /// Configuration constants for initializing the ImTKSilk window and context.
    /// </summary>
    public class ImTKSilkConstant
    {
        public string windowTitle = "ImTK Application";
        public int windowWidth = 1280;
        public int windowHeight = 720;
        public bool vsync = true;

        /// <summary>
        /// Enable ImGui multi-window viewports feature.
        /// </summary>
        public bool enableViewports = true;

        /// <summary>
        /// The path where ImGui configuration (.ini) and ImTK window states are saved.
        /// </summary>
        public string configFolderPath = "imgui";

        /// <summary>
        /// When true, windows can only be moved by dragging their title bar, not from anywhere inside the window.
        /// </summary>
        public bool windowMoveFromTitleBarOnly = false;
    }
}
