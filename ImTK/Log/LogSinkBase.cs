using System;
using System.Collections.Generic;

namespace ImTK.Log;

public abstract class LogSinkBase : ILogSink
{
    public bool enabled { get; set; } = true;
    public LogLevel minimumLevel { get; set; } = LogLevel.Debug;
    public HashSet<string> excludedContexts { get; } = new HashSet<string>();

    public abstract string description { get; }

    public void Emit(LogEntry entry)
    {
        if (!enabled) return;
        if (entry.Level < minimumLevel) return;
        if (excludedContexts.Contains(entry.ContextName)) return;

        WriteToTarget(entry);
    }

    protected abstract void WriteToTarget(LogEntry entry);
}
