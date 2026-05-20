using System;
using ImTK.UI;

namespace ImTK.Sample.Framework
{
    public static class ThemeMenu
    {
        [MainMenu("Theme/Dark", priority = 10)]
        public static void SetDarkTheme()
        {
            ImTKTheme.GlobalTheme = ImTKTheme.DefaultDark;
        }

        [MainMenu("Theme/Light", priority = 20)]
        public static void SetLightTheme()
        {
            ImTKTheme.GlobalTheme = ImTKTheme.DefaultLight;
        }
    }
}