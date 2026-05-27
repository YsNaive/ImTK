using System;
using ImGuiNET;

namespace ImTK.UI
{
    public abstract class DropdownItem<TValue> : VisualElement
    {
        public TValue value { get; internal set; }
        
        public string displayString { get; internal set; }
        
        public bool isSelected { get; internal set; }
        
        public Action<TValue> onSelected { get; internal set; }

        public DropdownItem()
        {
            this.useNativeLayout = true;
            this.style.positionType = PositionType.Absolute;
        }

        public override void OnRender()
        {
            if (ImGui.Selectable(displayString, isSelected))
            {
                onSelected?.Invoke(this.value);
            }

            if (isSelected)
            {
                ImGui.SetItemDefaultFocus();
            }
        }
    }
}
