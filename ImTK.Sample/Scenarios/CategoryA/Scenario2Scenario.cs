using System;
using System.Collections.Generic;
using ImTK.Sample.Framework;
using ImTK.UI;
using ImGuiNET;

namespace ImTK.Sample.Scenarios.CategoryA
{
    public class Scenario2Scenario : SampleScenarioBase
    {
        public override string Description => "This is Dummy Scenario 2 in Category A. It has a higher order so it should appear after Scenario 1.";

        public override string Category => "Category A";
        public override int Order => 20;

        public override Window Open()
        {
            return Window.Open<Scenario2Window>();
        }
    }

    public class Scenario2Window : Window
    {
        public Scenario2Window() : base("Scenario 2 Demo") { }
        public override void OnRender() { ImGui.Text("Hello from Scenario 2!"); }
    }
}
