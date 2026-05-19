using System;
using System.Collections.Generic;
using ImTK.Sample.Framework;
using ImTK.UI;
using ImGuiNET;

namespace ImTK.Sample.Scenarios.CategoryA
{
    public class Scenario1Scenario : SampleScenarioBase
    {
        public override string Description => "This is Dummy Scenario 1 in Category A.";

        public override string Category => "Category A";
        public override int Order => 10;

        public override IEnumerable<Type> SeeAlso => new[] { typeof(Scenario2Scenario), typeof(ImTK.Sample.Scenarios.CategoryB.Scenario3Scenario) };

        public override void Open()
        {
            Window.Open<Scenario1Window>();
        }
    }

    public class Scenario1Window : Window
    {
        public Scenario1Window() : base("Scenario 1 Demo") { }
        protected override void OnRenderSelf() { ImGui.Text("Hello from Scenario 1!"); }
    }
}
