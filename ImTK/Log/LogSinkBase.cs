using System;
using System.Collections.Generic;

namespace ImTK.Log;

public abstract class LogSinkBase : ILogSink
{
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
    public HashSet<string> ExcludedContexts { get; } = new HashSet<string>();
    public Func<LogEntry, string> Formatter { get; set; }

    public void Emit(LogEntry entry)
    {
        if (entry.Level < MinimumLevel) return;
        if (ExcludedContexts.Contains(entry.ContextName)) return;

        string formattedMsg = Formatter != null ? Formatter(entry) : entry.Message;
        WriteToTarget(formattedMsg, entry);
    }

    protected abstract void WriteToTarget(string formattedMsg, LogEntry originalEntry);
}
