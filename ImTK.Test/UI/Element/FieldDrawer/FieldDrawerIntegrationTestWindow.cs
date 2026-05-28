using ImTK.UI;
using ImTK.Log;
using ImTK.Core;
using Hexa.NET.ImGui;

namespace ImTK.Test.UI.Element.FieldDrawer
{
    public class TestPersonData
    {
        public string name = "John Doe";
        public int age = 30;
    }

    public class FieldDrawerIntegrationTestWindow : Window
    {

        private ObjectDrawer m_drawer;
        private TestPersonData m_data;

        public FieldDrawerIntegrationTestWindow() : base("Field Drawer Integration Test")
        {
            m_data = new TestPersonData();

            m_drawer = new ObjectDrawer();
            m_drawer.label = "Person Data";
            m_drawer.value = m_data;

            m_drawer.RegisterCallback<ValueChangedEvent<object>>(evt =>
            {
                ImTKLog.Info($"Object Drawer Triggered: isInternalChange={evt.isInternalChange}");
            });

            Add(m_drawer);

            var checkBtn = new Button("Check Current Data", evt =>
            {
                ImTKLog.Info($"Current Data - Name: {m_data.name}, Age: {m_data.age}");
            });
            Add(checkBtn);

            var assignExtBtn = new Button("Simulate External Change Without Notify", evt =>
            {
                var newData = new TestPersonData { name = "Jane Doe", age = 25 };
                m_data = newData;
                m_drawer.SetValueWithoutNotify(newData);
                ImTKLog.Info("Assigned new external data without notify.");
            });
            Add(assignExtBtn);
        }
    }

    public class FieldDrawerTestModule : ImTKModule
    {
        private FieldDrawerTestModule() { }

        protected internal override void OnInitializeSelf()
        {
            Window.Open<FieldDrawerIntegrationTestWindow>();
        }
    }
}
