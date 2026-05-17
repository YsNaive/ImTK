using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public static class DefaultStyles
    {
        public static void Register(StyleSheet sheet)
        {
            var btn = sheet.AddBlock("Button");
            btn.SetColor(ImTKStyleKey.BackgroundColor, "--primary-color");
            btn.SetColor(ImTKStyleKey.HoverColor, "--button-hovered");
            btn.SetColor(ImTKStyleKey.ActiveColor, "--button-active");

            var win = sheet.AddBlock("Window");
            win.SetColor(ImTKStyleKey.BackgroundColor, "--background-1");
            win.SetColor(ImTKStyleKey.HoverColor, "--background-2");
            win.SetColor(ImTKStyleKey.ActiveColor, "--background-2");
            win.SetColor(ImTKStyleKey.BorderColor, "--background-2");
        }
    }
}
