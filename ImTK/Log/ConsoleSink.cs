using System;

namespace ImTK.Log;

public class ConsoleSink : LogSinkBase
{
    private readonly object m_lock = new object();

    public override string description => "Standard console output sink";

    private readonly Func<LogEntry, string> m_formatter = new LogFormatterBuilder()
        .TimeSinceStartup()
        .Level()
        .ContextName()
        .Text(" ")
        .Message()
        .Build();

    protected ConsoleSink() { }

    protected override void WriteToTarget(LogEntry entry)
    {
        lock (m_lock)
        {
            // TODO: Implement color formatting based on entry.Level
            // when the VisualElement Style system is developed.
            Console.WriteLine(m_formatter(entry));
        }
    }
}
