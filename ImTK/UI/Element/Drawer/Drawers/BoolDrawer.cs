using ImGuiNET;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(bool), allowInheritType: false)]
    public class BoolDrawer : FieldDrawer<bool>
    {
        public BoolDrawer()
        {
        }

        public override string label
        {
            get => base.label;
            set
            {
                base.label = value;
            }
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
        }

        public override bool value
        {
            get => base.value;
            set
            {
                base.value = value;
            }
        }

                protected internal override bool CheckHoverState()
        {
            return ImGuiNET.ImGui.IsItemHovered(ImGuiNET.ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        }

        public override void OnRender()
        {
            bool v = value;
            if (ImGuiNET.ImGui.Checkbox("##" + label, ref v))
            {
                SetValueWithChanged(v);
            }
            base.OnRender();
        }
    }
}
