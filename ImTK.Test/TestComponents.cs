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

        protected override void RenderVisualTree()
        {
            if (ImGui.Button(m_label))
            {
                // UIEventBase.source is internal set. We can't set it from ImTK.Test easily unless ImTK exposes it or InternalsVisibleTo
                // To keep it clean, let's add a public SetSource method on UIEventBase or make it public setter,
                // but user said internal set.
                // Let's create an event from VisualElement itself using a protected method to send events
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

        protected override void RenderVisualTree()
        {
            ImGui.Text($"Composite Container: {m_name}");
            ImGui.Separator();
        }
    }
}
