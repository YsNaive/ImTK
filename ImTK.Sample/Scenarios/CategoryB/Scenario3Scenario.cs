using System;
using System.Collections.Generic;
using ImTK.Sample.Framework;
using ImTK.UI;
using Hexa.NET.ImGui;

namespace ImTK.Sample.Scenarios.CategoryB
{
    public class Scenario3Scenario : SampleScenarioBase
    {
        public override string Description => "This is Dummy Scenario 3 in Category B. It should appear in a different collapsible header.";

        public override string Category => "Category B";
        public override int Order => 10;

        public override IEnumerable<Type> SeeAlso => new[] { typeof(ImTK.Sample.Scenarios.CategoryA.Scenario1Scenario) };

        public override Window Open()
        {
            return Window.Open<Scenario3Window>();
        }
    }

    public class Scenario3Window : Window
    {
        public Scenario3Window() : base("Scenario 3 Demo") { }
        public override void OnRender() { ImGui.Text("Hello from Scenario 3!"); }
    }
}
