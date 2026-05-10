using System;

namespace ImTK.Logging;

/// <summary>
/// Represents a single log event.
/// Designed as a readonly struct to minimize GC pressure and ensure thread safety.
/// </summary>
public readonly struct LogEntry
{
    public readonly DateTime Timestamp;
    public readonly LogLevel Level;
    public readonly string ContextName;
    public readonly string Message;
    public readonly Exception Exception;

    public LogEntry(LogLevel level, string contextName, string message, Exception exception = null)
    {
        Timestamp = DateTime.Now;
        Level = level;
        ContextName = contextName;
        Message = message;
        Exception = exception;
    }
}