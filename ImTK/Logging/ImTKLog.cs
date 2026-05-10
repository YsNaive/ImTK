using System.Collections.Concurrent;

namespace ImTK.Logging;

/// <summary>
/// The global log dispatcher.
/// Receives log entries from LogContexts and dispatches them to registered ILogSinks.
/// </summary>
public static class ImTKLog
{
    private static readonly ConcurrentBag<ILogSink> s_sinks = new();

    /// <summary>
    /// Registers a new log sink.
    /// </summary>
    public static void AddSink(ILogSink sink)
    {
        s_sinks.Add(sink);
    }

    /// <summary>
    /// Dispatches a log entry to all registered sinks.
    /// This method is thread-safe and can be called from any thread.
    /// </summary>
    public static void Dispatch(LogEntry entry)
    {
        foreach (var sink in s_sinks)
        {
            sink.Emit(entry);
        }
    }
}