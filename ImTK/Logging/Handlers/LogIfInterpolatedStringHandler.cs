using System.Runtime.CompilerServices;

namespace ImTK.Logging;

/// <summary>
/// A custom interpolated string handler for conditional logging.
/// It prevents the evaluation and allocation of the interpolated string
/// if the condition provided is false.
/// </summary>
[InterpolatedStringHandler]
public ref struct LogIfInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _handler;

    public LogIfInterpolatedStringHandler(int literalLength, int formattedCount, bool condition, out bool isAppended)
    {
        if (condition)
        {
            _handler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
            isAppended = true;
        }
        else
        {
            _handler = default;
            isAppended = false;
        }
    }

    public void AppendLiteral(string s)
    {
        _handler.AppendLiteral(s);
    }

    public void AppendFormatted<T>(T t)
    {
        _handler.AppendFormatted(t);
    }

    public void AppendFormatted<T>(T t, string format)
    {
        _handler.AppendFormatted(t, format);
    }

    public void AppendFormatted<T>(T t, int alignment)
    {
        _handler.AppendFormatted(t, alignment);
    }

    public void AppendFormatted<T>(T t, int alignment, string format)
    {
        _handler.AppendFormatted(t, alignment, format);
    }

    // Support for string explicitly (often handled by T but sometimes better explicit)
    public void AppendFormatted(string s)
    {
        _handler.AppendFormatted(s);
    }

    internal string GetFormattedText()
    {
        return _handler.ToStringAndClear();
    }
}