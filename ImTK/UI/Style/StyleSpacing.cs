using System.Numerics;
using ImTK.Core;

namespace ImTK.UI
{
    public struct StyleSpacing
    {
        public StyleKeyword Keyword;
        public Vector2 Value;
        public HashedString Token;

        public bool IsNull => Keyword == StyleKeyword.Null;
        public bool IsToken => !string.IsNullOrEmpty(Token.Value);

        public static implicit operator StyleSpacing(float uniform) => new StyleSpacing { Value = new Vector2(uniform, uniform), Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleSpacing(Vector2 vec) => new StyleSpacing { Value = vec, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleSpacing(string token) => new StyleSpacing { Token = new HashedString(token), Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleSpacing(StyleKeyword keyword) => new StyleSpacing { Keyword = keyword };
    }
}
