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
            RemoveProperty(StyleVarType.Color, (int)key);

            var prop = new StyleProperty
            {
                Key = (int)key,
                Type = StyleVarType.Color,
                Keyword = value.Keyword
            };

            if (value.IsToken)
            {
                prop.TokenHash = value.Token.Hash;
            }
            else
            {
                prop.ColorValue = value.Value.u32;
            }

            Properties.Add(prop);
            return this;
        }

        public StyleBlock SetFloat(ImTKStyleKey key, StyleValue<float> value)
        {
            RemoveProperty(StyleVarType.Float, (int)key);

            var prop = new StyleProperty
            {
                Key = (int)key,
                Type = StyleVarType.Float,
                Keyword = value.Keyword
            };

            if (value.IsToken)
            {
                prop.TokenHash = value.Token.Hash;
            }
            else
            {
                prop.FloatValue = value.Value;
            }

            Properties.Add(prop);
            return this;
        }

        public StyleBlock SetVector2(ImTKStyleKey key, StyleValue<Vector2> value)
        {
            RemoveProperty(StyleVarType.Vector2, (int)key);

            var prop = new StyleProperty
            {
                Key = (int)key,
                Type = StyleVarType.Vector2,
                Keyword = value.Keyword
            };

            if (value.IsToken)
            {
                prop.TokenHash = value.Token.Hash;
            }
            else
            {
                prop.Vector2Value = value.Value;
            }

            Properties.Add(prop);
            return this;
        }

        private void RemoveProperty(StyleVarType type, int key)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                if (Properties[i].Type == type && Properties[i].Key == key)
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
