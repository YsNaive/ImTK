namespace ImTK.Logging;

/// <summary>
/// Defines a target that receives log entries.
/// </summary>
public interface ILogSink
{
    /// <summary>
    /// Processes a log entry.
    /// </summary>
    /// <param name="entry">The log entry to process.</param>
    void Emit(LogEntry entry);
}