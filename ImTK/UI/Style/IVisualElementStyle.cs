namespace ImTK.UI.Style
{
    public interface IVisualElementStyle
    {
        void PushToImGui(ResolvedStyle resolvedStyle);
        void PopFromImGui();
    }
}
