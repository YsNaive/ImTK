using System;
using System.Collections.Immutable;

namespace ImTK.Log;

public static class ImTKLog
{
    private static ImmutableArray<ILogSink> _sinks = ImmutableArray<ILogSink>.Empty;

    public static void AddSink(ILogSink sink)
    {
        if (sink == null) throw new ArgumentNullException(nameof(sink));
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

    public static void Emit(LogEntry entry)
    {
        var currentSinks = _sinks;
        for (int i = 0; i < currentSinks.Length; i++)
        {
            currentSinks[i].Emit(entry);
        }
    }
}
