using System.Numerics;
using ImTK.Core;

namespace ImTK.UI
{
    public struct StyleThickness
    {
        public StyleKeyword Keyword;
        public Thickness Value;
        public HashedString Token;

        public bool IsNull => Keyword == StyleKeyword.Null;
        public bool IsToken => !string.IsNullOrEmpty(Token.Value);

        public static implicit operator StyleThickness(float uniform) => new StyleThickness { Value = new Thickness(uniform), Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleThickness(Vector2 vec) => new StyleThickness { Value = new Thickness(vec.X, vec.Y), Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleThickness(Thickness t) => new StyleThickness { Value = t, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleThickness(string token) => new StyleThickness { Token = token, Keyword = StyleKeyword.Undefined };
        public static implicit operator StyleThickness(StyleKeyword keyword) => new StyleThickness { Keyword = keyword };
    }
}
