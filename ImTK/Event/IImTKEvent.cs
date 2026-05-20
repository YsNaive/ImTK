namespace ImTK.Event
{
    /// <summary>
    /// Base interface for all application-level events handled by ImTKEventBus.
    /// Application events are typically named with an "On" prefix (e.g., OnFileLoadedEvent)
    /// to distinguish them from UI interaction events (e.g., ClickEvent).
    /// </summary>
    public interface IImTKEvent
    {
    }
}
