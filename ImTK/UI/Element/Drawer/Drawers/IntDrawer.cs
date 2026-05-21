using ImGuiNET;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(int), allowInheritType: false)]
    public class IntDrawer : FieldDrawer<int>
    {
        protected override void OnRenderSelf()
        {
            base.OnRenderSelf();

            int tempValue = m_value;
            // hide default ImGui label since we draw it in base class
            if (ImGui.InputInt("##" + label, ref tempValue))
            {
                SetValueWithChanged(tempValue);
            }
        }
    }
}
