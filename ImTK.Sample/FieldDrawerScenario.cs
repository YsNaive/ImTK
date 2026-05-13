using ImTK.UI;
using ImTK.Sample.Framework;
using ImTK.Log;

namespace ImTK.Sample
{
    // Define an example data structure
    public class CharacterStats
    {
        public string Name = "Hero";
        public int Level = 1;
        public int Health = 100;
    }

    public class FieldDrawerTestWindow : Window
    {
        private CharacterStats m_stats;

        public FieldDrawerTestWindow() : base("Field Drawer Interactive Demo")
        {
            m_stats = new CharacterStats();

            // Create an ObjectDrawer to auto-generate UI for CharacterStats
            var drawer = new ObjectDrawer();
            drawer.label = "Character Stats";
            drawer.value = m_stats;

            // Optionally, listen to value changes
            drawer.RegisterCallback<ValueChangedEvent<object>>(evt =>
            {
                new LogContext("FieldDrawerScenario").Info($"CharacterStats object triggered internal change: {evt.isInternalChange}");
            });

            Add(drawer);

            var checkBtn = new Button("Check Values via Button", evt =>
            {
                new LogContext("FieldDrawerScenario").Info($"Current stats: {m_stats.Name} Lvl {m_stats.Level} HP {m_stats.Health}");
            });
            Add(checkBtn);
        }
    }

    public class FieldDrawerScenario : ISampleScenario
    {
        public string ScenarioName => "Field Drawer Architecture";
        public string Description => "Demonstrates dynamic UI generation using the new FieldDrawer registry and factory architecture.";
        public string DocumentationPath => "docs/UI/Element/FieldDrawer.md";

        public void Open()
        {
            Window.Open<FieldDrawerTestWindow>();
        }
    }
}
