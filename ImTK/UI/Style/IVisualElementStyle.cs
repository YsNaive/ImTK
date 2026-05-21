namespace ImTK.UI
{
    public interface IVisualElementStyle
    {
        void PushToImGui(ResolvedStyle resolvedStyle);
        void PopFromImGui();
    }
}
