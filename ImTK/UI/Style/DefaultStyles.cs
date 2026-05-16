using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public static class DefaultStyles
    {
        public static void Register(StyleSheet sheet)
        {
            var btn = sheet.AddBlock("Button");
            btn.SetColor(ImGuiCol.Button, "--primary-color");
            btn.SetColor(ImGuiCol.ButtonHovered, "--button-hovered");
            btn.SetColor(ImGuiCol.ButtonActive, "--button-active");

            var win = sheet.AddBlock("Window");
            win.SetColor(ImGuiCol.WindowBg, "--background-1");
            win.SetColor(ImGuiCol.TitleBg, "--background-2");
            win.SetColor(ImGuiCol.TitleBgActive, "--background-2");
            win.SetColor(ImGuiCol.TitleBgCollapsed, "--background-2");
        }
    }
}
