using System;

namespace ImTK.Log;

public readonly struct LogEntry
{
    public readonly DateTime Timestamp;
    public readonly LogLevel Level;
    public readonly string ContextName;
    public readonly string Message;
    public readonly Exception Exception;

    public LogEntry(DateTime timestamp, LogLevel level, string contextName, string message, Exception exception = null)
    {
        Timestamp = timestamp;
        Level = level;
        ContextName = contextName;
        Message = message;
        Exception = exception;
    }
}
