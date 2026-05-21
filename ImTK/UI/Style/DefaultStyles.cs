using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public static class DefaultStyles
    {
        public static void Register(StyleSheet sheet)
        {
            // By utilizing ImTKTheme.ApplyToImGui(), ImGui handles standard window, button,
            // and text styling globally.
            // This global style sheet is reserved for future custom overrides or extensions.
        }
    }
}
