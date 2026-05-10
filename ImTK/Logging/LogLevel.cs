namespace ImTK.Logging;

/// <summary>
/// Defines the severity levels for log entries.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Most detailed information. Expect these to be written to logs only.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Information that is useful for debugging.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// General flow of the application.
    /// </summary>
    Info = 2,

    /// <summary>
    /// Abnormal or unexpected events, but the application can continue.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Errors and exceptions that should be investigated.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical errors causing application failure.
    /// </summary>
    Fatal = 5
}