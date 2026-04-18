using ImGuiNET;

namespace ImTK;

public class TextDrawer : RuntimeDrawer<string>
{
    public uint maxLength = 256;

    public TextDrawer(string label = "", string initialValue = "") : base(label, initialValue)
    {
    }

    protected override void OnRenderDrawer()
    {
        string v = m_value ?? string.Empty;
        ImGui.PushItemWidth(-1);
        if (ImGui.InputText($"##{GetHashCode()}", ref v, maxLength))
        {
            value = v;
        }
        ImGui.PopItemWidth();
    }
}
