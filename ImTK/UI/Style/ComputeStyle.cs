using System.Collections.Generic;
using ImGuiNET;
using ImTK.Core;
using ImTK.Log;

namespace ImTK.UI.Style
{
    public static class ComputeStyle
    {
        private static readonly LogContext s_log = new LogContext("ComputeStyle");

        public static List<StyleProperty> Overlay(VisualElement element)
        {
            var computed = new List<StyleProperty>();
            var mergedProperties = new Dictionary<int, StyleProperty>(); // Key: (Type << 16) | Key

            // Helper to add/override properties
            void MergeProperties(IEnumerable<StyleProperty> properties)
            {
                if (properties == null) return;
                foreach (var prop in properties)
                {
                    if (prop.IsNull)
                    {
                        mergedProperties.Remove((((int)prop.Type) << 16) | prop.Key);
                    }
                    else
                    {
                        mergedProperties[(((int)prop.Type) << 16) | prop.Key] = prop;
                    }
                }
            }

            // 1. Collect classes from elements
            var classes = new List<HashedString>(element.classList.GetClasses());

            // 2. Resolve Global and Local StyleSheets for these classes
            // We search from global down to local (so local overrides global)
            // Note: Our requirement is "local overrides global", so we merge Global first, then Local

            // Global
            foreach (var cls in classes)
            {
                if (StyleSheet.Global.TryGetBlock(cls, out var block))
                {
                    MergeProperties(block.Properties);
                }
            }

            // Local (Traverse up the hierarchy, top-down merge)
            var ancestorSheets = new List<StyleSheet>();
            var current = element;
            while (current != null)
            {
                if (current.localStyleSheet != null)
                {
                    ancestorSheets.Add(current.localStyleSheet);
                }
                current = current.parent;
            }

            // Merge ancestors from root to element
            for (int i = ancestorSheets.Count - 1; i >= 0; i--)
            {
                foreach (var cls in classes)
                {
                    if (ancestorSheets[i].TryGetBlock(cls, out var block))
                    {
                        MergeProperties(block.Properties);
                    }
                }
            }

            // 3. Merge Inline Styles (Highest priority)
            if (element.style.m_overrideStyles != null)
            {
                MergeProperties(element.style.m_overrideStyles);
            }

            // 4. Resolve Tokens from Theme
            var activeTheme = element.theme;
            foreach (var kvp in mergedProperties)
            {
                var prop = kvp.Value;
                if (prop.IsToken)
                {
                    if (prop.Type == StyleVarType.Color)
                    {
                        if (activeTheme.TryGetColorToken(prop.TokenHash, out var color))
                        {
                            prop.ColorValue = color.u32;
                        }
                        else
                        {
                            s_log.Warning($"Style System: Missing color token hash '{prop.TokenHash}'. Falling back to Magenta.");
                            prop.ColorValue = Color.Magenta.u32;
                        }
                    }
                    else if (prop.Type == StyleVarType.Float)
                    {
                        if (activeTheme.TryGetFloatToken(prop.TokenHash, out var val))
                        {
                            prop.FloatValue = val;
                        }
                        else
                        {
                            s_log.Warning($"Style System: Missing float token hash '{prop.TokenHash}'. Falling back to 0.");
                            prop.FloatValue = 0f;
                        }
                    }
                    else if (prop.Type == StyleVarType.Vector2)
                    {
                        if (activeTheme.TryGetVector2Token(prop.TokenHash, out var val))
                        {
                            prop.Vector2Value = val;
                        }
                        else
                        {
                            s_log.Warning($"Style System: Missing Vector2 token hash '{prop.TokenHash}'. Falling back to Zero.");
                            prop.Vector2Value = System.Numerics.Vector2.Zero;
                        }
                    }
                }
                computed.Add(prop);
            }

            return computed;
        }

        public static void Push(List<StyleProperty> computedStyles, ImTK.UI.Style.StyleMapping mapping, out int pushedColors, out int pushedVars)
        {
            pushedColors = 0;
            pushedVars = 0;

            if (computedStyles == null) return;

            for (int i = 0; i < computedStyles.Count; i++)
            {
                var prop = computedStyles[i];
                if (prop.Type == StyleVarType.Color)
                {
                    int imguiTarget = mapping.colorTargets[prop.Key];
                    if (imguiTarget != -1)
                    {
                        ImGui.PushStyleColor((ImGuiCol)imguiTarget, prop.ColorValue);
                        pushedColors++;
                    }
                }
                else if (prop.Type == StyleVarType.Float)
                {
                    int imguiTarget = mapping.floatTargets[prop.Key];
                    if (imguiTarget != -1)
                    {
                        ImGui.PushStyleVar((ImGuiStyleVar)imguiTarget, prop.FloatValue);
                        pushedVars++;
                    }
                }
                else if (prop.Type == StyleVarType.Vector2)
                {
                    int imguiTarget = mapping.vector2Targets[prop.Key];
                    if (imguiTarget != -1)
                    {
                        ImGui.PushStyleVar((ImGuiStyleVar)imguiTarget, prop.Vector2Value);
                        pushedVars++;
                    }
                }
            }
        }

        public static void Pop(int pushedColors, int pushedVars)
        {
            if (pushedColors > 0) ImGui.PopStyleColor(pushedColors);
            if (pushedVars > 0) ImGui.PopStyleVar(pushedVars);
        }
    }
}
