using System;
using System.Runtime.CompilerServices;

namespace ImTK.Logging;

/// <summary>
/// The context object used to emit log entries.
/// Developers create an instance of this context, typically one per module,
/// and use it to send logs that automatically include the module's name.
/// </summary>
public class LogContext
{
    public string ModuleName { get; }

    public LogContext(string moduleName)
    {
        ModuleName = moduleName;
    }

    private void Emit(LogLevel level, string message, Exception exception = null)
    {
        var entry = new LogEntry(level, ModuleName, message, exception);
        ImTKLog.Dispatch(entry);
    }

    // ========================================================================
    // Normal Logging Methods
    // ========================================================================

    public void Trace(string message) => Emit(LogLevel.Trace, message);
    public void Debug(string message) => Emit(LogLevel.Debug, message);
    public void Info(string message) => Emit(LogLevel.Info, message);
    public void Warning(string message) => Emit(LogLevel.Warning, message);

    // Error and Fatal support Exception overloads
    public void Error(string message) => Emit(LogLevel.Error, message);
    public void Error(Exception exception, string message) => Emit(LogLevel.Error, message, exception);

    public void Fatal(string message) => Emit(LogLevel.Fatal, message);
    public void Fatal(Exception exception, string message) => Emit(LogLevel.Fatal, message, exception);

    // ========================================================================
    // Conditional Logging Methods (xxxIf)
    // ========================================================================

    public void TraceIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Trace, handler.GetFormattedText());
    }

    public void DebugIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Debug, handler.GetFormattedText());
    }

    public void InfoIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Info, handler.GetFormattedText());
    }

    public void WarningIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Warning, handler.GetFormattedText());
    }

    public void ErrorIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Error, handler.GetFormattedText());
    }

    public void ErrorIf(bool condition, Exception exception, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Error, handler.GetFormattedText(), exception);
    }

    public void FatalIf(bool condition, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Fatal, handler.GetFormattedText());
    }

    public void FatalIf(bool condition, Exception exception, [InterpolatedStringHandlerArgument("condition")] ref LogIfInterpolatedStringHandler handler)
    {
        if (condition) Emit(LogLevel.Fatal, handler.GetFormattedText(), exception);
    }
}