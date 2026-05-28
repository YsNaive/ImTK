using System;
using System.Collections.Immutable;

namespace ImTK.Log;

public static class ImTKLog
{
    private static ImmutableArray<ILogSink> _sinks = ImmutableArray<ILogSink>.Empty;
    private static System.Collections.Concurrent.ConcurrentQueue<LogEntry> _earlyLogs = new System.Collections.Concurrent.ConcurrentQueue<LogEntry>();

    public static void AddSink(ILogSink sink)
    {
        if (sink == null) throw new ArgumentNullException(nameof(sink));
        
        // 將早於任何 Sink 註冊前就發出的 Log 補送到這個新加入的 Sink 中
        foreach (var earlyLog in _earlyLogs)
        {
            sink.Emit(earlyLog);
        }
        
        ImmutableInterlocked.Update(ref _sinks, sinks => sinks.Add(sink));
    }

    public static void RemoveSink(ILogSink sink)
    {
        if (sink == null) throw new ArgumentNullException(nameof(sink));
        ImmutableInterlocked.Update(ref _sinks, sinks => sinks.Remove(sink));
    }

    public static void ClearSinks()
    {
        ImmutableInterlocked.Update(ref _sinks, sinks => sinks.Clear());
    }

    public static void SetSinkEnabled<T>(bool enabled) where T : ILogSink
    {
        var currentSinks = _sinks;
        for (int i = 0; i < currentSinks.Length; i++)
        {
            if (currentSinks[i] is T targetSink)
            {
                targetSink.enabled = enabled;
            }
        }
    }

    public static void Emit(LogEntry entry)
    {
        var currentSinks = _sinks;
        if (currentSinks.Length == 0)
        {
            _earlyLogs.Enqueue(entry);
            return;
        }

        for (int i = 0; i < currentSinks.Length; i++)
        {
            currentSinks[i].Emit(entry);
        }
    }

    private static void Log(LogLevel level, string message, string filePath, Exception exception = null, bool includeStackTrace = true)
    {
        string moduleName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        var entry = new LogEntry(DateTime.Now, level, moduleName, message, exception, includeStackTrace);
        Emit(entry);
    }

    public static void Trace(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Trace, message, filePath);
    public static void Debug(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Debug, message, filePath);
    public static void Info(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Info, message, filePath);
    public static void Warning(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Warning, message, filePath);
    public static void Error(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Error, message, filePath);
    public static void Error(Exception ex, string message, bool includeStackTrace = true, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Error, message, filePath, ex, includeStackTrace);
    public static void Fatal(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Fatal, message, filePath);
    public static void Fatal(Exception ex, string message, bool includeStackTrace = true, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => Log(LogLevel.Fatal, message, filePath, ex, includeStackTrace);

    public static void TraceIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        if (condition) Log(LogLevel.Trace, message.GetFormattedText(), filePath);
    }

    public static void DebugIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        if (condition) Log(LogLevel.Debug, message.GetFormattedText(), filePath);
    }

    public static void InfoIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        if (condition) Log(LogLevel.Info, message.GetFormattedText(), filePath);
    }

    public static void WarningIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        if (condition) Log(LogLevel.Warning, message.GetFormattedText(), filePath);
    }

    public static void ErrorIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        if (condition) Log(LogLevel.Error, message.GetFormattedText(), filePath);
    }

    public static void FatalIf(bool condition, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        if (condition) Log(LogLevel.Fatal, message.GetFormattedText(), filePath);
    }
}
