using System;

namespace ImTK.Log;

public class ConsoleSink : LogSinkBase
{
    private readonly object _lock = new object();

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
