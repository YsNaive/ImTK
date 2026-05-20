namespace ImTK.Event
{
    /// <summary>
    /// Event triggered when the ImTKFontManager has rebuilt the font atlas.
    /// Graphics bridges (like ImTKSilk) should subscribe to this event to recreate device font textures.
    /// </summary>
    public struct OnFontChangedEvent : IImTKEvent
    {
    }
}
