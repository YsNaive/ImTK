using System.Numerics;

namespace ImTK.UI
{
    public struct ResolvedLayoutState : System.IEquatable<ResolvedLayoutState>
    {
        public float? width;
        public float? height;
        public float? minWidth;
        public float? maxWidth;
        public float? minHeight;
        public float? maxHeight;

        public Thickness margin;
        public Thickness padding;
        public FlexDirection flexDirection;
        public FlexWrap flexWrap;
        public JustifyContent justifyContent;
        public AlignItems alignItems;
        public float flexGrow;
        public AlignItems alignSelf;

        public PositionType positionType;
        public float? top;
        public float? bottom;
        public float? left;
        public float? right;

        public DisplayStyle display;

        public static readonly ResolvedLayoutState Default = new ResolvedLayoutState
        {
            width = null,
            height = null,
            minWidth = null,
            maxWidth = null,
            minHeight = null,
            maxHeight = null,
            margin = Thickness.Zero,
            padding = Thickness.Zero,
            flexDirection = FlexDirection.Column,
            flexWrap = FlexWrap.NoWrap,
            justifyContent = JustifyContent.FlexStart,
            alignItems = AlignItems.Stretch,
            flexGrow = 0f,
            alignSelf = AlignItems.Stretch, // In CSS, alignSelf defaults to auto (which defers to alignItems). Here we can just use Stretch as default if not specified, but usually it should inherit from parent's alignItems. We'll handle this in VisualElement.
            positionType = PositionType.Relative, // As discussed in Issue 4, Option B, Relative is the default
            top = null,
            bottom = null,
            left = null,
            right = null,
            display = DisplayStyle.Flex
        };

        public bool Equals(ResolvedLayoutState other)
        {
            return width == other.width &&
                   height == other.height &&
                   minWidth == other.minWidth &&
                   maxWidth == other.maxWidth &&
                   minHeight == other.minHeight &&
                   maxHeight == other.maxHeight &&
                   margin.Equals(other.margin) &&
                   padding.Equals(other.padding) &&
                   flexDirection == other.flexDirection &&
                   flexWrap == other.flexWrap &&
                   justifyContent == other.justifyContent &&
                   alignItems == other.alignItems &&
                   flexGrow == other.flexGrow &&
                   alignSelf == other.alignSelf &&
                   positionType == other.positionType &&
                   top == other.top &&
                   bottom == other.bottom &&
                   left == other.left &&
                   right == other.right &&
                   display == other.display;
        }

        public override bool Equals(object obj) => obj is ResolvedLayoutState other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new System.HashCode();
            hash.Add(width);
            hash.Add(height);
            hash.Add(minWidth);
            hash.Add(maxWidth);
            hash.Add(minHeight);
            hash.Add(maxHeight);
            hash.Add(margin);
            hash.Add(padding);
            hash.Add(flexDirection);
            hash.Add(flexWrap);
            hash.Add(justifyContent);
            hash.Add(alignItems);
            hash.Add(flexGrow);
            hash.Add(alignSelf);
            hash.Add(positionType);
            hash.Add(top);
            hash.Add(bottom);
            hash.Add(left);
            hash.Add(right);
            hash.Add(display);
            return hash.ToHashCode();
        }

        public static bool operator ==(ResolvedLayoutState left, ResolvedLayoutState right) => left.Equals(right);
        public static bool operator !=(ResolvedLayoutState left, ResolvedLayoutState right) => !(left == right);
    }
}
