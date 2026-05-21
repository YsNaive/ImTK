using ImTK.Core;

namespace ImTK.UI
{
    public struct StyleValue<T> where T : struct
    {
        public StyleKeyword Keyword;
        public T Value;
        public HashedString Token;

        public bool IsNull => Keyword == StyleKeyword.Null;
        public bool IsToken => !string.IsNullOrEmpty(Token.Value);

        public static implicit operator StyleValue<T>(T val) => new StyleValue<T> { Value = val, Keyword = StyleKeyword.Undefined };

        public static implicit operator StyleValue<T>(string token) => new StyleValue<T> { Token = token, Keyword = StyleKeyword.Undefined };

        public static implicit operator StyleValue<T>(StyleKeyword keyword) => new StyleValue<T> { Keyword = keyword };
    }
}
