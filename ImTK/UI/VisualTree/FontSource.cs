using System;
using System.IO;
using System.Linq;
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
            string[] extensions = new string[] { "", ".ttf", ".otf", ".ttc" };
            bool hasExtension = extensions.Any(ext => !string.IsNullOrEmpty(ext) && fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

            // If it already has a valid font extension, we only search for that exact name, otherwise we try all extensions.
            string[] searchExtensions = hasExtension ? new string[] { "" } : extensions;

            string[] searchDirectories = GetSystemFontDirectories();

            foreach (var dir in searchDirectories)
            {
                if (!Directory.Exists(dir)) continue;

                foreach (var ext in searchExtensions)
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
