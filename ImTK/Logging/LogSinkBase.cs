using System;
using System.Collections.Generic;

namespace ImTK.Logging;

/// <summary>
/// A base class for log sinks that provides common functionality like minimum level filtering,
/// context exclusion, and message formatting.
/// </summary>
public abstract class LogSinkBase : ILogSink
{
    /// <summary>
    /// The minimum log level this sink will process. Defaults to Debug.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// A set of context names to exclude from processing.
    /// </summary>
    public HashSet<string> ExcludedContexts { get; } = new HashSet<string>();

    /// <summary>
    /// The formatter used to convert a LogEntry into a string.
    /// Defaults to the standard format.
    /// </summary>
    public Func<LogEntry, string> Formatter { get; set; } = LogFormatters.Standard;

    public void Emit(LogEntry entry)
    {
        if (entry.Level < MinimumLevel)
        {
            return;
        }

        if (ExcludedContexts.Contains(entry.ContextName))
        {
            return;
        }

        string formattedMsg = Formatter(entry);
        WriteToTarget(formattedMsg, entry);
    }

    /// <summary>
    /// Writes the formatted message to the target destination.
    /// </summary>
    protected abstract void WriteToTarget(string formattedMsg, LogEntry originalEntry);
}