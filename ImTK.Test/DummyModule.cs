using System;
using ImTK.Core;

namespace ImTK.Test
{
    // A simple module to test the reflection loading and lifecycle
    public class DummyModule : ImTKModule
    {
        private DummyModule() { } // Private constructor, required by ImTKApplication

        protected override void OnInitializeSelf()
        {
            Console.WriteLine("[DummyModule] OnInitializeSelf called.");
        }

        protected override void OnLogicUpdate()
        {
            // Just test time accessibility
            if (Time.DeltaTime < 0) throw new Exception("Time issue");
        }

        protected override void OnClose()
        {
            Console.WriteLine("[DummyModule] OnClose called.");
        }
    }
}
