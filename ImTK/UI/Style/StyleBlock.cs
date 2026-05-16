using System.Collections.Generic;
using ImGuiNET;
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

        public StyleBlock SetColor(ImGuiCol col, StyleValue<Color> value)
        {
            RemoveProperty(StyleVarType.Color, (int)col);

            var prop = new StyleProperty
            {
                Key = (int)col,
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

        public StyleBlock SetVar(ImGuiStyleVar styleVar, StyleValue<float> value)
        {
            RemoveProperty(StyleVarType.Float, (int)styleVar);

            var prop = new StyleProperty
            {
                Key = (int)styleVar,
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

        public StyleBlock SetVar(ImGuiStyleVar styleVar, StyleValue<System.Numerics.Vector2> value)
        {
            RemoveProperty(StyleVarType.Vector2, (int)styleVar);

            var prop = new StyleProperty
            {
                Key = (int)styleVar,
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
    }
}
