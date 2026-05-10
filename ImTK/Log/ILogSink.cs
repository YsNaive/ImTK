namespace ImTK.Log;

public interface ILogSink
{
    void Emit(LogEntry entry);
}
