using ImGuiNET;
using ImTK.UI;
using ImTK.UI.Style;
using ImTK.Test.Framework;

namespace ImTK.Test.UI.Style
{
    public class StyleSystemWindow : Window, IIntegrationTest
    {
        public string TestCategory => "Style";
        public string TestName => "Style Cascading & Token Demo";
        public bool IsManualOnly => true;

        private StyleSheet m_localSheet;

        public StyleSystemWindow() : base("Style System Demo")
        {
            flags.alwaysAutoResize = true;

            // Setup a local stylesheet
            m_localSheet = new StyleSheet();
            var localBtn = m_localSheet.AddBlock("local-btn");
            localBtn.BackgroundColor(Color.Magenta);

            // Create a button using global style
            var globalBtn = new Button("Global Style Button");
            // The "Button" class is automatically added in the Button constructor and registered in DefaultStyles

            // Create a button using local style
            var localStyleBtn = new Button("Local Style Button");
            localStyleBtn.classList.Add("local-btn");
            localStyleBtn.localStyleSheet = m_localSheet;

            // Create a button using inline override
            var inlineBtn = new Button("Inline Override Button");
            inlineBtn.style.backgroundColor = Color.Yellow;

            // Create a toggle button
            var toggleBtn = new Button("Toggle Local Class");
            toggleBtn.onClicked += (e) =>
            {
                globalBtn.classList.Toggle("local-btn");
                globalBtn.localStyleSheet = m_localSheet; // Apply local sheet dynamically
            };

            Add(globalBtn);
            Add(localStyleBtn);
            Add(inlineBtn);
            // Add(new ImTK.UI.Label("---"));
            Add(toggleBtn);
        }

        public void Run()
        {
            Open();
        }
    }
}
