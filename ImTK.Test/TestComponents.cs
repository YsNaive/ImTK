using ImGuiNET;
using ImTK.UI;

namespace ImTK.Test
{
    // Extended UIEventBase to allow setting source in same assembly context or use method
    public static class EventExtensions
    {
        public static void SetSource(this UIEventBase evt, VisualElement source)
        {
            // Reflection or internal internals visible to
        }
    }

    // A simple button wrapper that generates a ClickEvent
    public class TestButtonElement : VisualElement
    {
        private readonly string m_label;

        public TestButtonElement(string label)
        {
            m_label = label;
        }

        protected override void OnRenderSelf()
        {
            if (ImGui.Button(m_label))
            {
                SendEvent(EventPool<ClickEvent>.Get());
            }
        }
    }

    // A composite container to test the dual-layer shadow DOM logic
    public class TestCompositeContainer : VisualElement
    {
        private readonly VisualElement m_innerContainer;
        private readonly string m_name;

        // Override contentContainer to forward logically added children to m_innerContainer
        public override VisualElement contentContainer => m_innerContainer;

        public TestCompositeContainer(string name)
        {
            m_name = name;
            m_innerContainer = new VisualElement();

            // Add the inner container to the physical tree
            hierarchy.Add(m_innerContainer);
        }

        protected override void OnRenderLayout()
        {
            ImGui.Text($"Composite Container: {m_name}");
            ImGui.Separator();

            ImGui.Indent();
            base.OnRenderLayout(); // Here the base logic renders the internal children
            ImGui.Unindent();
        }
    }
}
