using ImTK.Core;

namespace ImTK.UI
{
    public struct StyleColor
    {
        public StyleKeyword Keyword;
        public Color Value;
        public HashedString Token;

        public bool IsNull => Keyword == StyleKeyword.Null;
        public bool IsToken => !string.IsNullOrEmpty(Token.Value);

        public static implicit operator StyleColor(Color val) => new StyleColor { Value = val, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleColor(uint hex) => new StyleColor { Value = (Color)hex, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleColor(string token) => new StyleColor { Token = new HashedString(token), Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleColor(StyleKeyword keyword) => new StyleColor { Keyword = keyword };
    }
}
