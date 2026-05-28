using Hexa.NET.ImGui;
using ImTK.UI;
using ImTK.UI.Persistence;
using ImTK.Test.Framework;

namespace ImTK.Test.UI.Persistence
{
    public struct NestedStructConfig
    {
        public float Ratio;
        public int Size;
        public string Name;
    }

    public class NestedClassConfig
    {
        public bool IsEnabled;
        public float Alpha;
    }

    public class PersistentDemoElement : VisualElement
    {
        [Persistent]
        private float m_myFloat = 0.5f;

        [Persistent("CustomIntKey")]
        public int MyInt { get; set; } = 10;

        [Persistent]
        private string m_myString = "Hello";

        [Persistent]
        private bool m_myBool = true;

        [Persistent]
        private System.Numerics.Vector2 m_unsupportedVector = new System.Numerics.Vector2();

        [Persistent(IncludeAllMembers = true)]
        private NestedStructConfig m_structConfig = new NestedStructConfig { Ratio = 0.5f, Size = 100, Name = "StructTest" };

        [Persistent(IncludeAllMembers = true)]
        private NestedClassConfig m_classConfig = new NestedClassConfig { IsEnabled = true, Alpha = 1.0f };

        public PersistentDemoElement()
        {
            // Require a persistenceKey for persistence to work
            persistenceKey = "DemoElementKey";
            style.minHeight = 400;
        }

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            return new System.Numerics.Vector2(350, 400);
        }

        public override void OnRender()
        {
            ImGui.Text($"Float: {m_myFloat}");
            ImGui.SliderFloat("##float", ref m_myFloat, 0f, 1f);

            int myInt = MyInt;
            ImGui.Text($"Int: {myInt}");
            if (ImGui.SliderInt("##int", ref myInt, 0, 100)) MyInt = myInt;

            ImGui.Text($"String: {m_myString}");
            ImGui.InputText("##str", ref m_myString, 100);

            ImGui.Text($"Bool: {m_myBool}");
            ImGui.Checkbox("##bool", ref m_myBool);
            
            ImGui.Separator();
            ImGui.Text("--- Nested Struct (IncludeAllMembers) ---");
            ImGui.SliderFloat("Ratio", ref m_structConfig.Ratio, 0f, 1f);
            ImGui.SliderInt("Size", ref m_structConfig.Size, 10, 200);
            ImGui.InputText("Struct Name", ref m_structConfig.Name, 50);

            ImGui.Separator();
            ImGui.Text("--- Nested Class (IncludeAllMembers) ---");
            ImGui.Checkbox("IsEnabled", ref m_classConfig.IsEnabled);
            ImGui.SliderFloat("Alpha", ref m_classConfig.Alpha, 0f, 1f);

            ImGui.Separator();
            ImGui.Text("Close and reopen the window to see values persist!");
        }
    }

    public class PersistenceSystemWindow : Window, IIntegrationTest
    {
        public string TestCategory => "Persistence";
        public string TestName => "UI State Persistence Demo";
        public bool IsManualOnly => true;

        public PersistenceSystemWindow() : base("Persistence Demo")
        {
            flags.alwaysAutoResize = true;
            
            Add(new PersistentDemoElement());
        }

        public void Run()
        {
            Open();
        }
    }
}
