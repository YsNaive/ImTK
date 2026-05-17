using ImGuiNET;

namespace ImTK.UI.Style
{
    public class StyleMapping
    {
        public readonly int[] colorTargets;
        public readonly int[] floatTargets;
        public readonly int[] vector2Targets;

        public StyleMapping()
        {
            int maxCount = (int)ImTKStyleKey.MaxCount;
            colorTargets = new int[maxCount];
            floatTargets = new int[maxCount];
            vector2Targets = new int[maxCount];

            // Initialize all targets to -1 (Not Supported)
            for (int i = 0; i < maxCount; i++)
            {
                colorTargets[i] = -1;
                floatTargets[i] = -1;
                vector2Targets[i] = -1;
            }

            // Default Mapping (Base visual element)
            colorTargets[(int)ImTKStyleKey.BackgroundColor] = (int)ImGuiCol.WindowBg;
            colorTargets[(int)ImTKStyleKey.TextColor] = (int)ImGuiCol.Text;
            colorTargets[(int)ImTKStyleKey.BorderColor] = (int)ImGuiCol.Border;
            floatTargets[(int)ImTKStyleKey.BorderRadius] = (int)ImGuiStyleVar.WindowRounding;
        }

        protected StyleMapping(StyleMapping baseMapping)
        {
            colorTargets = (int[])baseMapping.colorTargets.Clone();
            floatTargets = (int[])baseMapping.floatTargets.Clone();
            vector2Targets = (int[])baseMapping.vector2Targets.Clone();
        }
    }
}
