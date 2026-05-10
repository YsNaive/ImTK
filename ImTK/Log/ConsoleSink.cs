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

    public override Func<LogEntry, string> formatter => _formatter;

    protected ConsoleSink() { }

    protected override void WriteToTarget(string formattedMsg, LogEntry originalEntry)
    {
        lock (_lock)
        {
            // TODO: Implement color formatting based on originalEntry.Level
            // when the VisualElement Style system is developed.
            Console.WriteLine(formattedMsg);
        }
    }
}
