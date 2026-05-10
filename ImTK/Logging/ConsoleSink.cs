using System;

namespace ImTK.Logging;

/// <summary>
/// A log sink that writes formatted log entries to the standard console with basic color coding.
/// </summary>
public class ConsoleSink : LogSinkBase
{
    private static readonly object s_lock = new();

    protected override void WriteToTarget(string formattedMsg, LogEntry originalEntry)
    {
        lock (s_lock)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForLevel(originalEntry.Level);

            Console.WriteLine(formattedMsg);

            Console.ForegroundColor = originalColor;
        }
    }

    private ConsoleColor GetColorForLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => ConsoleColor.DarkGray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => ConsoleColor.White,
        };
    }
}