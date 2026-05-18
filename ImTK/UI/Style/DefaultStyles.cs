using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public static class DefaultStyles
    {
        public static void Register(StyleSheet sheet)
        {
            var btn = sheet.AddBlock("Button");
            btn.SetColor(VisualElement.StyleKey.BackgroundColor, "--primary-color");
            btn.SetColor(VisualElement.StyleKey.HoverColor, "--button-hovered");
            btn.SetColor(VisualElement.StyleKey.ActiveColor, "--button-active");

            var win = sheet.AddBlock("Window");
            win.SetColor(VisualElement.StyleKey.BackgroundColor, "--background-1");
            win.SetColor(Window.StyleKey.TitleBg, "--background-2");
            win.SetColor(VisualElement.StyleKey.BorderColor, "--background-2");
        }
    }
}
