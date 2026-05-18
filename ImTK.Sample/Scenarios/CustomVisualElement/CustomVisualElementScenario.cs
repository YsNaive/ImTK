using ImGuiNET;
using ImTK.Core;
using ImTK.Sample.Framework;
using ImTK.UI;
using ImTK.UI.Style;

namespace ImTK.Sample.Scenarios.CustomVisualElement
{
    public class CustomVisualElementScenario : ISampleScenario
    {
        public string ScenarioName => "Custom VisualElement & StyleSOP";
        public string Description => "Demonstrates how to build a custom VisualElement with specific TStyle and StyleKeys.";
        public string DocumentationPath => "Scenarios/CustomVisualElement/README.md";

        public void Open()
        {
            Window.Open<CustomElementDemoWindow>();
        }
    }

    public class Badge : VisualElement<Badge.Style>
    {
        public new class StyleKey : VisualElement.StyleKey
        {
            public static readonly HashedString BadgeColor = new HashedString("BadgeColor");
        }

        public new class Style : VisualElement.Style
        {
            private int m_pushedColors = 0;

            public override void PushToImGui(ResolvedStyle resolvedStyle)
            {
                base.PushToImGui(resolvedStyle);
                m_pushedColors = 0;

                Color? badgeColor = resolvedStyle.GetColor(StyleKey.BadgeColor);
                if (badgeColor.HasValue)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, badgeColor.Value.u32);
                    m_pushedColors++;
                }
            }

            public override void PopFromImGui()
            {
                if (m_pushedColors > 0)
                {
                    ImGui.PopStyleColor(m_pushedColors);
                    m_pushedColors = 0;
                }
                base.PopFromImGui();
            }

            public StyleValue<Color>? badgeColor
            {
                get => GetOverrideColor(StyleKey.BadgeColor);
                set
                {
                    if (value.HasValue) SetColor(StyleKey.BadgeColor, value.Value);
                    else Clear(StyleKey.BadgeColor);
                }
            }
        }

        public string text { get; set; }

        public Badge(string text)
        {
            this.text = text;
            classList.Add("Badge");
        }

        protected override void OnRenderSelf()
        {
            ImGui.SmallButton(text);
        }
    }

    public class CustomElementDemoWindow : Window
    {
        public CustomElementDemoWindow() : base("Custom Element Demo")
        {
            var badge1 = new Badge("Default Badge");
            Add(badge1);

            var badge2 = new Badge("Red Badge");
            badge2.style.badgeColor = Color.Red;
            Add(badge2);

            var badge3 = new Badge("Theme Danger Badge");
            badge3.style.badgeColor = "--danger-color"; // Using Theme Token mapping fallback manually or resolved properly
            Add(badge3);
        }
    }
}
