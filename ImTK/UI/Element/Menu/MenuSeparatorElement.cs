using Hexa.NET.ImGui;

namespace ImTK.UI
{
    public class MenuSeparatorElement : VisualElement, IMenuElement
    {
        public string name { get; set; } = "Separator";
        public int priority { get; set; }

        public MenuSeparatorElement()
        {
            m_useNativeLayout = true;
        }

        public override void OnRender()
        {
            ImGui.Separator();
        }
    }
}
