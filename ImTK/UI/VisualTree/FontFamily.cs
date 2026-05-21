using System;
using System.Collections.Generic;

namespace ImTK.UI
{
    public class FontFamily
    {
        public string Name { get; private set; }
        public List<FontSource> FontSources { get; private set; }

        public FontFamily(string name)
        {
            Name = name;
            FontSources = new List<FontSource>();
        }

        public void AddSource(FontSource source)
        {
            FontSources.Add(source);
        }

        [Obsolete("Use AddSource instead")]
        public void AddFallback(string path)
        {
            FontSources.Add(new FontSource(path));
        }

        [Obsolete("GlyphRanges are now defined per FontSource")]
        public IntPtr GlyphRanges { get; private set; }
    }
}
