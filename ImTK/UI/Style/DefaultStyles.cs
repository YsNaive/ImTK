using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public static class DefaultStyles
    {
        public static void Register(StyleSheet sheet)
        {
            var btn = sheet.AddBlock("Button");
            btn.SetColor(VisualElement.StyleKey.BackgroundColor, "--normal-fg");
            // Instead of explicit active/hover colors which are removed from ImTKTheme, rely on default behavior or custom style logic if needed.
            // For now we map them to the same family colors for completeness.
            btn.SetColor(VisualElement.StyleKey.HoverColor, "--normal-sub-fg");
            btn.SetColor(VisualElement.StyleKey.ActiveColor, "--normal-fg");

            var win = sheet.AddBlock("Window");
            win.SetColor(VisualElement.StyleKey.BackgroundColor, "--normal-bg");
            win.SetColor(Window.StyleKey.TitleBg, "--normal-sub-bg");
            win.SetColor(VisualElement.StyleKey.BorderColor, "--normal-sub-bg");
        }
    }
}
