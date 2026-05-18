using System.Collections.Generic;
using System.Numerics;
using ImTK.Core;

namespace ImTK.UI.Style
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

        public StyleBlock SetColor(ImTKStyleKey key, StyleValue<Color> value)
        {
            RemoveProperty((int)key);

            var prop = new StyleProperty
            {
                key = (int)key,
                type = value.Keyword == StyleKeyword.Null ? StylePropertyType.Null : (value.IsToken ? StylePropertyType.Token : StylePropertyType.ColorValue)
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

        public StyleBlock SetFloat(ImTKStyleKey key, StyleValue<float> value)
        {
            RemoveProperty((int)key);

            var prop = new StyleProperty
            {
                key = (int)key,
                type = value.Keyword == StyleKeyword.Null ? StylePropertyType.Null : (value.IsToken ? StylePropertyType.Token : StylePropertyType.FloatValue)
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

        public StyleBlock SetVector2(ImTKStyleKey key, StyleValue<Vector2> value)
        {
            RemoveProperty((int)key);

            var prop = new StyleProperty
            {
                key = (int)key,
                type = value.Keyword == StyleKeyword.Null ? StylePropertyType.Null : (value.IsToken ? StylePropertyType.Token : StylePropertyType.Vector2Value)
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

        private void RemoveProperty(int key)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                if (Properties[i].key == key)
                {
                    Properties.RemoveAt(i);
                    return;
                }
            }
        }

        // --- Fluent Syntax Sugar ---

        public StyleBlock BackgroundColor(StyleValue<Color> value) => SetColor(ImTKStyleKey.BackgroundColor, value);
        public StyleBlock TextColor(StyleValue<Color> value) => SetColor(ImTKStyleKey.TextColor, value);
        public StyleBlock HoverColor(StyleValue<Color> value) => SetColor(ImTKStyleKey.HoverColor, value);
        public StyleBlock ActiveColor(StyleValue<Color> value) => SetColor(ImTKStyleKey.ActiveColor, value);
        public StyleBlock BorderColor(StyleValue<Color> value) => SetColor(ImTKStyleKey.BorderColor, value);

        public StyleBlock BorderRadius(StyleValue<float> value) => SetFloat(ImTKStyleKey.BorderRadius, value);
    }
}
