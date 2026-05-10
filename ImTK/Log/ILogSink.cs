namespace ImTK.Log;

public interface ILogSink
{
    bool enabled { get; set; }
    void Emit(LogEntry entry);
}
