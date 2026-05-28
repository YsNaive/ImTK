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
}
