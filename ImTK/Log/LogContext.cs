using System;
using System.Runtime.CompilerServices;

namespace ImTK.Log;

public class LogContext
{
    public string ModuleName { get; }

    public LogContext(string moduleName)
    {
        ModuleName = moduleName;
    }

    private void Log(LogLevel level, string message, Exception exception = null)
    {
        var entry = new LogEntry(DateTime.Now, level, ModuleName, message, exception);
        ImTKLog.Emit(entry);
    }

    public void Trace(string message) => Log(LogLevel.Trace, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Error(string message) => Log(LogLevel.Error, message);
    public void Error(Exception ex, string message) => Log(LogLevel.Error, message, ex);
    public void Fatal(string message) => Log(LogLevel.Fatal, message);
    public void Fatal(Exception ex, string message) => Log(LogLevel.Fatal, message, ex);

    public void TraceIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message)
    {
        if (condition) Log(LogLevel.Trace, message.GetFormattedText());
    }

    public void DebugIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message)
    {
        if (condition) Log(LogLevel.Debug, message.GetFormattedText());
    }

    public void InfoIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message)
    {
        if (condition) Log(LogLevel.Info, message.GetFormattedText());
    }

    public void WarningIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message)
    {
        if (condition) Log(LogLevel.Warning, message.GetFormattedText());
    }

    public void ErrorIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message)
    {
        if (condition) Log(LogLevel.Error, message.GetFormattedText());
    }

    public void FatalIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler message)
    {
        if (condition) Log(LogLevel.Fatal, message.GetFormattedText());
    }
}
