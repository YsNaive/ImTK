using System.Collections.Generic;
using System.Numerics;
using ImTK.Core;

namespace ImTK.UI
{
    public class StyleBlock
    {
        public HashedString ClassName { get; }
        public List<StyleProperty> Properties { get; } = new List<StyleProperty>();

        public StyleBlock(HashedString className)
        {
            ClassName = className;
        }

        // --- Core Setters ---

        public StyleBlock SetColor(HashedString key, StyleValue<Color> value)
        {
            RemoveProperty(key.Hash);

            var prop = new StyleProperty
            {
                category = StyleCategory.HighLevelToken,
                key = key.Hash,
                dataType = value.Keyword == StyleKeyword.Null ? StyleDataType.Null : (value.IsToken ? StyleDataType.HashedString : StyleDataType.Color)
            };

            if (value.IsToken)
            {
                prop.tokenHash = value.Token.Hash;
            }
            else if (!value.IsNull)
            {
                prop.colorValue = value.Value.u32;
            }

            Properties.Add(prop);
            return this;
        }

        public StyleBlock SetFloat(HashedString key, StyleValue<float> value)
        {
            RemoveProperty(key.Hash);

            var prop = new StyleProperty
            {
                category = StyleCategory.HighLevelToken,
                key = key.Hash,
                dataType = value.Keyword == StyleKeyword.Null ? StyleDataType.Null : (value.IsToken ? StyleDataType.HashedString : StyleDataType.Float)
            };

            if (value.IsToken)
            {
                prop.tokenHash = value.Token.Hash;
            }
            else if (!value.IsNull)
            {
                prop.floatValue = value.Value;
            }

            Properties.Add(prop);
            return this;
        }

        public StyleBlock SetVector2(HashedString key, StyleValue<Vector2> value)
        {
            RemoveProperty(key.Hash);

            var prop = new StyleProperty
            {
                category = StyleCategory.HighLevelToken,
                key = key.Hash,
                dataType = value.Keyword == StyleKeyword.Null ? StyleDataType.Null : (value.IsToken ? StyleDataType.HashedString : StyleDataType.Vector2)
            };

            if (value.IsToken)
            {
                prop.tokenHash = value.Token.Hash;
            }
            else if (!value.IsNull)
            {
                prop.vector2Value = value.Value;
            }

            Properties.Add(prop);
            return this;
        }

        private void RemoveProperty(int keyHash)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                if (Properties[i].key == keyHash)
                {
                    Properties.RemoveAt(i);
                    return;
                }
            }
        }

        // --- Fluent Syntax Sugar ---

        public StyleBlock BackgroundColor(StyleValue<Color> value) => SetColor(VisualElement.StyleKey.BackgroundColor, value);
        public StyleBlock TextColor(StyleValue<Color> value) => SetColor(VisualElement.StyleKey.TextColor, value);
        public StyleBlock DisabledTextColor(StyleValue<Color> value) => SetColor(VisualElement.StyleKey.DisabledTextColor, value);
        public StyleBlock SelectionColor(StyleValue<Color> value) => SetColor(VisualElement.StyleKey.SelectionColor, value);
        public StyleBlock BorderColor(StyleValue<Color> value) => SetColor(VisualElement.StyleKey.BorderColor, value);

        public StyleBlock BorderRadius(StyleValue<float> value) => SetFloat(VisualElement.StyleKey.BorderRadius, value);
    }
}
