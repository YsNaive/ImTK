using System.Collections.Generic;

namespace ImTK.UI
{
    public class FontFamily
    {
        public string Name { get; private set; }
        public List<string> FontPaths { get; private set; }

        public System.IntPtr GlyphRanges { get; private set; }

        public FontFamily(string name, System.IntPtr glyphRanges = default)
        {
            Name = name;
            FontPaths = new List<string>();
            GlyphRanges = glyphRanges;
        }

        public void AddFallback(string path)
        {
            FontPaths.Add(path);
        }
    }
}
