using ImTK.Core;

namespace ImTK.UI
{
    public struct StyleFontSize
    {
        public StyleKeyword Keyword;
        public int IntValue;
        public FontSize EnumValue;
        public HashedString Token;
        public bool IsEnum;

        public bool IsNull => Keyword == StyleKeyword.Null;
        public bool IsToken => !string.IsNullOrEmpty(Token.Value);

        public static implicit operator StyleFontSize(int val) => new StyleFontSize { IntValue = val, IsEnum = false, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleFontSize(float val) => new StyleFontSize { IntValue = (int)val, IsEnum = false, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleFontSize(FontSize val) => new StyleFontSize { EnumValue = val, IsEnum = true, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleFontSize(string token) => new StyleFontSize { Token = new HashedString(token), Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleFontSize(StyleKeyword keyword) => new StyleFontSize { Keyword = keyword };
    }
}
