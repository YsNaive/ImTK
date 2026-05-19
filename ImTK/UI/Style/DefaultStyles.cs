using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public static class DefaultStyles
    {
        public static void Register(StyleSheet sheet)
        {
            // By utilizing ImTKTheme.ApplyToImGui(), ImGui now handles standard window, button,
            // and text styling globally.
            // This global style sheet is now reserved for future custom overrides or extensions.
        }
    }
}
