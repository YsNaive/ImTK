using System;

namespace ImTK.Log;

public class ConsoleSink : LogSinkBase
{
    private readonly object _lock = new object();

    public override string description => "Standard console output sink";

    private readonly Func<LogEntry, string> _formatter = new LogFormatterBuilder()
        .Timestamp()
        .Level()
        .ContextName()
        .Text(" ")
        .Message()
        .Build();

    protected ConsoleSink() { }

    protected override void WriteToTarget(LogEntry entry)
    {
        lock (_lock)
        {
            // TODO: Implement color formatting based on entry.Level
            // when the VisualElement Style system is developed.
            Console.WriteLine(_formatter(entry));
        }
    }
}
