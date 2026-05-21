using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ImTK.UI
{
    public class FontSource
    {
        public string Path { get; private set; }
        public string ResolvedPath { get; private set; }
        public IntPtr GlyphRanges { get; private set; }

        public FontSource(string path, IntPtr glyphRanges = default)
        {
            Path = path;
            GlyphRanges = glyphRanges;
            ResolvedPath = ResolveFontPath(path);
        }

        private string ResolveFontPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (File.Exists(path))
            {
                return path;
            }

            string fileName = System.IO.Path.GetFileName(path);
            bool hasExtension = System.IO.Path.HasExtension(fileName);
            string[] extensions = hasExtension ? new string[] { "" } : new string[] { "", ".ttf", ".otf", ".ttc" };

            string[] searchDirectories = GetSystemFontDirectories();

            foreach (var dir in searchDirectories)
            {
                if (!Directory.Exists(dir)) continue;

                foreach (var ext in extensions)
                {
                    string fullPath = System.IO.Path.Combine(dir, fileName + ext);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }

        private string[] GetSystemFontDirectories()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new string[] {
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"),
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts")
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new string[] {
                    "/System/Library/Fonts/",
                    "/Library/Fonts/",
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Fonts")
                };
            }
            else // Linux
            {
                return new string[] {
                    "/usr/share/fonts/",
                    "/usr/local/share/fonts/",
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "fonts"),
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")
                };
            }
        }
    }
}
