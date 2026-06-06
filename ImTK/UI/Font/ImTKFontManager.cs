using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Hexa.NET.ImGui;
using ImTK.Log;
using ImTK.Event;

namespace ImTK.UI
{
    public static class ImTKFontManager
    {


        private static Dictionary<int, FontFamily> s_fontFamilies = new Dictionary<int, FontFamily>();

        // Dictionary mapping string Hash -> ImFontPtr
        private static Dictionary<int, ImFontPtr> s_loadedFonts = new Dictionary<int, ImFontPtr>();

        private static bool s_isFontDirty = true;

        public static readonly int DefaultFontFamilyHash = new ImTK.HashedString("ImGuiDefault").Hash;
        private static readonly string DefaultFontFamilyName = "ImGuiDefault";

        static ImTKFontManager()
        {
            RegisterFamily(DefaultFontFamilyName, new FontSource[0]);
        }


        public static void MarkFontDirty()
        {
            s_isFontDirty = true;
        }

        public static void RegisterFamily(string name, params FontSource[] sources)
        {
            int hash = new ImTK.HashedString(name).Hash;
            if (s_fontFamilies.ContainsKey(hash))
            {
                ImTKLog.Warning($"FontFamily '{name}' already registered. Overwriting.");
            }

            var family = new FontFamily(name);
            foreach (var source in sources)
            {
                family.AddSource(source);
            }

            s_fontFamilies[hash] = family;
            s_isFontDirty = true;
        }

        public static void RegisterFamily(string name, string[] paths)
        {
            FontSource[] sources = new FontSource[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                sources[i] = new FontSource(paths[i]);
            }
            RegisterFamily(name, sources);
        }

        public static void OverrideDefaultFamily(params FontSource[] sources)
        {
            RegisterFamily(DefaultFontFamilyName, sources);
        }

        public static bool IsFontDirty() => s_isFontDirty;

        public static unsafe void ResolveFont()
        {
            if (!s_isFontDirty) return;

            var sw = Stopwatch.StartNew();
            ImTKLog.Info("Rebuilding Font Atlas...");
            s_loadedFonts.Clear();

            var io = ImGui.GetIO();
            io.Fonts.Clear();

            // Build fonts based on Normal size as base size, multiplied by Main Viewport DPI and global scale
            float mainDpi = ImTKTheme.GlobalTheme.globalFontScale;
            if (ImGui.GetCurrentContext().Handle != null)
            {
                mainDpi = ImTK.UI.RenderEngine.Context.MainViewportDpiScale * ImTKTheme.GlobalTheme.globalFontScale;
            }
            if (mainDpi <= 0.0f) mainDpi = 1.0f;

            var sizes = ImTKTheme.GlobalTheme.GetFontSizes();
            float baseSizePixels = sizes[FontSize.Normal] * mainDpi; // Bake main viewport DPI exactly
            ImTKLog.Info($"[FontBake] DpiScale={ImGui.GetMainViewport().DpiScale:F3}  globalFontScale={ImTKTheme.GlobalTheme.globalFontScale:F3}  mainDpi={mainDpi:F3}  baseSizePixels={baseSizePixels:F1}  (fontSizeNormal={sizes[FontSize.Normal]})");

            // Ensure DefaultFontFamily is processed FIRST so it becomes io.Fonts.Fonts[0] (the global default font)
            if (s_fontFamilies.TryGetValue(DefaultFontFamilyHash, out var defaultFamily))
            {
                ProcessFamily(DefaultFontFamilyHash, defaultFamily);
            }

            foreach (var familyKvp in s_fontFamilies)
            {
                if (familyKvp.Key == DefaultFontFamilyHash)
                    continue;

                ProcessFamily(familyKvp.Key, familyKvp.Value);
            }

            void ProcessFamily(int keyHash, FontFamily family)
            {
                bool isDefaultFamily = keyHash == DefaultFontFamilyHash;
                bool isFirst = true;
                ImFontPtr baseFont = new ImFontPtr();

                if (family.FontSources.Count == 0)
                {
                    var config = CreateDefaultFontConfig();
                    config.SizePixels = baseSizePixels;
                    baseFont = io.Fonts.AddFontDefault(ref config);
                }
                else
                {
                    foreach (var source in family.FontSources)
                    {
                        if (string.IsNullOrEmpty(source.ResolvedPath) || !File.Exists(source.ResolvedPath))
                        {
                            ImTKLog.Warning($"Font file not found: {source.Path}. Skipping.");
                            continue;
                        }

                        var config = CreateDefaultFontConfig();
                        config.SizePixels = baseSizePixels;
                        if (!isFirst)
                        {
                            config.MergeMode = 1;
                        }

                        // Reduce oversampling for large fonts to prevent massive texture sizes
                        if (baseSizePixels >= 24f)
                        {
                            config.OversampleH = 1;
                            config.OversampleV = 1;
                        }

                        ImFontPtr font;
                        unsafe
                        {
                            font = io.Fonts.AddFontFromFileTTF(source.ResolvedPath, baseSizePixels, &config, (uint*)0);
                        }

                        if (isFirst && font.Handle != null)
                        {
                            baseFont = font;
                            isFirst = false;
                        }
                    }

                    if (baseFont.Handle != null)
                    {
                        float nativeFontSize = ((Hexa.NET.ImGui.ImFont*)baseFont.Handle)->LegacySize;
                        ImTKLog.Info($"[FontBake] Family={family.Name} Baked Native Size={nativeFontSize}");
                    }

                    s_loadedFonts[keyHash] = baseFont;
                }

                // Hexa.NET.ImGui handles building internally or via Backend. No need to call Build().
                s_isFontDirty = false;
                sw.Stop();

                unsafe
                {
                    if (io.Fonts.Fonts.Size > 0)
                    {
                        io.FontDefault = io.Fonts.Fonts[0];
                    }
                }

                ImTKLog.Info($"Font Atlas Rebuild Complete in {sw.Elapsed.TotalSeconds:F2} seconds.");

                ImTKEventBus.Publish(new OnFontChangedEvent());
            }
        }

        public static ImFontPtr GetFont(int familyHash)
        {
            if (s_loadedFonts.TryGetValue(familyHash, out var font))
            {
                return font;
            }

            // Fallback to default family
            if (s_loadedFonts.TryGetValue(DefaultFontFamilyHash, out font))
            {
                return font;
            }

            // Absolute fallback (should generally not be reached if ResolveFont runs correctly)
            var ioFonts = ImGui.GetIO().Fonts;
            if (ioFonts.Fonts.Size > 0)
                return ioFonts.Fonts[0];

            return default; // Will cause rendering to default
        }
        private static ImFontConfig CreateDefaultFontConfig()
        {
            var config = new ImFontConfig();
            config.FontDataOwnedByAtlas = 1; // true
            config.OversampleH = 2;
            config.OversampleV = 1;
            config.PixelSnapH = 1; // true
            config.GlyphMaxAdvanceX = float.MaxValue;
            config.RasterizerMultiply = 1.0f;
            config.RasterizerDensity = 1.0f;
            config.MergeMode = 0; // false
            return config;
        }
    }
}
