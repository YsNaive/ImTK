namespace ImTK.UI
{
    public interface IValueChangedEvent
    {
        object previousValueObj { get; }
        object newValueObj { get; }
        bool isInternalChange { get; }
    }
}
