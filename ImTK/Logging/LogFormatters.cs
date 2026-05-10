using System;

namespace ImTK.Logging;

/// <summary>
/// Provides predefined formatting templates for log entries.
/// </summary>
public static class LogFormatters
{
    /// <summary>
    /// A minimal format: [Level] Message
    /// e.g., [Info] System initialized.
    /// </summary>
    public static string Minimal(LogEntry e)
    {
        return e.Exception == null
            ? $"[{e.Level}] {e.Message}"
            : $"[{e.Level}] {e.Message}\n{e.Exception}";
    }

    /// <summary>
    /// A standard format: [HH:mm:ss][Level][ContextName] Message
    /// e.g., [15:30:00][Info][Graphics] Texture loaded.
    /// </summary>
    public static string Standard(LogEntry e)
    {
        return e.Exception == null
            ? $"[{e.Timestamp:HH:mm:ss}][{e.Level}][{e.ContextName}] {e.Message}"
            : $"[{e.Timestamp:HH:mm:ss}][{e.Level}][{e.ContextName}] {e.Message}\n{e.Exception}";
    }
}