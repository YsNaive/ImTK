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
            var originalColor = Console.ForegroundColor;
            switch (entry.Level)
            {
                case LogLevel.Trace: Console.ForegroundColor = ConsoleColor.DarkGray; break;
                case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.Gray; break;
                case LogLevel.Info: Console.ForegroundColor = ConsoleColor.White; break;
                case LogLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogLevel.Error:
                case LogLevel.Fatal: Console.ForegroundColor = ConsoleColor.Red; break;
            }
            Console.WriteLine(m_formatter(entry));
            Console.ForegroundColor = originalColor;
        }
    }
}
