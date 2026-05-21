using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using ImGuiNET;
using ImTK.Log;
using ImTK.Event;

namespace ImTK.UI
{
    public static class ImTKFontManager
    {
        private static readonly LogContext s_log = new LogContext("ImTKFontManager");

        private static Dictionary<int, FontFamily> s_fontFamilies = new Dictionary<int, FontFamily>();

        // Dictionary mapping string Hash -> ImFontPtr
        private static Dictionary<long, ImFontPtr> s_loadedFonts = new Dictionary<long, ImFontPtr>();

        private static bool s_isFontDirty = true;

        public static readonly int DefaultFontFamilyHash = new ImTK.Core.HashedString("ImGuiDefault").Hash;
        private static readonly string DefaultFontFamilyName = "ImGuiDefault";

        static ImTKFontManager()
        {
            RegisterFamily(DefaultFontFamilyName, new FontSource[0]);
        }


        private static Stack<float> s_fontScaleStack = new Stack<float>();

        public static float CurrentFontScale => s_fontScaleStack.Count > 0 ? s_fontScaleStack.Peek() : 1.0f;

        public static void PushFontScale(float scale)
        {
            s_fontScaleStack.Push(scale);
            RenderingContext.EnqueueWindowCommand(() => ImGui.SetWindowFontScale(scale));
        }

        public static void PopFontScale()
        {
            if (s_fontScaleStack.Count > 0)
                s_fontScaleStack.Pop();

            float previousScale = CurrentFontScale;
            RenderingContext.EnqueueWindowCommand(() => ImGui.SetWindowFontScale(previousScale));
        }

        public static void MarkFontDirty()
        {
            s_isFontDirty = true;
        }

        public static void RegisterFamily(string name, params FontSource[] sources)
        {
            int hash = new ImTK.Core.HashedString(name).Hash;
            if (s_fontFamilies.ContainsKey(hash))
            {
                s_log.Warning($"FontFamily '{name}' already registered. Overwriting.");
            }

            var family = new FontFamily(name);
            foreach (var source in sources)
            {
                family.AddSource(source);
            }

            s_fontFamilies[hash] = family;
            s_isFontDirty = true;
        }

        public static void RegisterFamily(string name, string[] paths, IntPtr glyphRanges = default)
        {
            FontSource[] sources = new FontSource[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                sources[i] = new FontSource(paths[i], glyphRanges);
            }
            RegisterFamily(name, sources);
        }

        public static void OverrideDefaultFamily(params FontSource[] sources)
        {
            RegisterFamily(DefaultFontFamilyName, sources);
        }

        public static IntPtr GetGlyphRangesChineseFull() => ImGui.GetIO().Fonts.GetGlyphRangesChineseFull();
        public static IntPtr GetGlyphRangesChineseSimplifiedCommon() => ImGui.GetIO().Fonts.GetGlyphRangesChineseSimplifiedCommon();
        public static IntPtr GetGlyphRangesJapanese() => ImGui.GetIO().Fonts.GetGlyphRangesJapanese();
        public static IntPtr GetGlyphRangesKorean() => ImGui.GetIO().Fonts.GetGlyphRangesKorean();

        public static bool IsFontDirty() => s_isFontDirty;

        public static unsafe void ResolveFont()
        {
            if (!s_isFontDirty) return;

            var sw = Stopwatch.StartNew();
            s_log.Info("Rebuilding Font Atlas...");
            s_loadedFonts.Clear();

            var io = ImGui.GetIO();
            io.Fonts.Clear();

            // Build fonts based on GlobalTheme sizes
            var sizes = ImTKTheme.GlobalTheme.GetFontSizes();

            // To ensure ImGui's default font (io.Fonts.Fonts[0]) aligns with our conceptual "Normal" size,
            // we sort the dictionary to process FontSize.Normal first.
            var sortedSizes = new List<KeyValuePair<FontSize, float>>(sizes);
            sortedSizes.Sort((a, b) =>
            {
                if (a.Key == FontSize.Normal && b.Key != FontSize.Normal) return -1;
                if (a.Key != FontSize.Normal && b.Key == FontSize.Normal) return 1;
                return a.Key.CompareTo(b.Key);
            });

            foreach (var familyKvp in s_fontFamilies)
            {
                var familyHash = familyKvp.Key;
                var family = familyKvp.Value;

                foreach (var sizeKvp in sortedSizes)
                {
                    var fontSizeEnum = sizeKvp.Key;
                    float sizePixels = sizeKvp.Value * ImTKTheme.GlobalTheme.globalFontScale;

                    ImFontPtr baseFont = default;

                    if (family.FontSources.Count == 0)
                    {
                        var config = ImGuiNative.ImFontConfig_ImFontConfig();
                        config->SizePixels = sizePixels;
                        baseFont = io.Fonts.AddFontDefault(config);
                        ImGuiNative.ImFontConfig_destroy(config);
                    }
                    else
                    {
                        bool first = true;
                        foreach (var source in family.FontSources)
                        {
                            if (string.IsNullOrEmpty(source.ResolvedPath) || !File.Exists(source.ResolvedPath))
                            {
                                s_log.Warning($"Font file not found: {source.Path}. Skipping.");
                                continue;
                            }

                            var config = ImGuiNative.ImFontConfig_ImFontConfig();
                            if (!first)
                            {
                                config->MergeMode = 1;
                            }

                            IntPtr ranges = source.GlyphRanges;
                            ImFontPtr font;
                            if (ranges != IntPtr.Zero)
                            {
                                font = io.Fonts.AddFontFromFileTTF(source.ResolvedPath, sizePixels, config, ranges);
                            }
                            else
                            {
                                font = io.Fonts.AddFontFromFileTTF(source.ResolvedPath, sizePixels, config);
                            }

                            if (first && font.NativePtr != null)
                            {
                                baseFont = font;
                                first = false;
                            }

                            ImGuiNative.ImFontConfig_destroy(config);
                        }

                        // Always add default font as ultimate fallback
                        var fallbackConfig = ImGuiNative.ImFontConfig_ImFontConfig();
                        fallbackConfig->MergeMode = 1;
                        fallbackConfig->SizePixels = sizePixels;
                        io.Fonts.AddFontDefault(fallbackConfig);
                        ImGuiNative.ImFontConfig_destroy(fallbackConfig);
                    }

                    long key = GetFontKey(familyHash, fontSizeEnum);
                    s_loadedFonts[key] = baseFont;
                }
            }

            // Always build the atlas immediately before notifying the bridge
            io.Fonts.Build();
            s_isFontDirty = false;
            sw.Stop();
            s_log.Info($"Font Atlas Rebuild Complete in {sw.Elapsed.TotalSeconds:F2} seconds.");

            ImTKEventBus.Publish(new OnFontChangedEvent());
        }

        private static long GetFontKey(int familyHash, FontSize size)
        {
            // Combine int hash and enum into a long key to avoid allocations
            return ((long)familyHash << 32) | (uint)size;
        }

        public static ImFontPtr GetFont(int familyHash, FontSize size)
        {
            long key = GetFontKey(familyHash, size);
            if (s_loadedFonts.TryGetValue(key, out var font))
            {
                return font;
            }

            // Fallback to default family with the requested size if available
            key = GetFontKey(DefaultFontFamilyHash, size);
            if (s_loadedFonts.TryGetValue(key, out font))
            {
                return font;
            }

            // Absolute fallback (should generally not be reached if ResolveFont runs correctly)
            var ioFonts = ImGui.GetIO().Fonts;
            if (ioFonts.Fonts.Size > 0)
                return ioFonts.Fonts[0];

            return default; // Will cause rendering to default
        }

        public static (ImFontPtr font, float scale) GetFontWithScale(int familyHash, int targetSize)
        {
            var sizes = ImTKTheme.GlobalTheme.GetFontSizes();
            float globalScale = ImTKTheme.GlobalTheme.globalFontScale;
            float scaledTargetSize = targetSize * globalScale;

            FontSize bestEnum = FontSize.Normal;
            float bestSizeDiff = float.MaxValue;
            float bestSizePixels = sizes[FontSize.Normal] * globalScale;

            foreach(var kvp in sizes)
            {
                float currentScaledSize = kvp.Value * globalScale;
                float diff = currentScaledSize - scaledTargetSize;
                // Prefer slightly larger fonts for downscaling rather than upscaling (which causes blur)
                if(diff >= 0 && diff < bestSizeDiff)
                {
                    bestSizeDiff = diff;
                    bestEnum = kvp.Key;
                    bestSizePixels = currentScaledSize;
                }
            }

            // If all fonts are smaller than target, pick the largest one available
            if(bestSizeDiff == float.MaxValue)
            {
                float maxSize = -1;
                foreach(var kvp in sizes)
                {
                    float currentScaledSize = kvp.Value * globalScale;
                    if(currentScaledSize > maxSize)
                    {
                        maxSize = currentScaledSize;
                        bestEnum = kvp.Key;
                        bestSizePixels = currentScaledSize;
                    }
                }
            }

            ImFontPtr font = GetFont(familyHash, bestEnum);
            float scale = scaledTargetSize / bestSizePixels;

            return (font, scale);
        }
    }
}
