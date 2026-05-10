using System;
using System.Collections.Generic;
using System.Text;

namespace ImTK.Log;

public class LogFormatterBuilder
{
    private readonly List<Action<StringBuilder, LogEntry>> _actions = new();

    public LogFormatterBuilder Text(string text)
    {
        _actions.Add((sb, entry) => sb.Append(text));
        return this;
    }

    public LogFormatterBuilder Timestamp(string format = "HH:mm:ss", string prefix = "[", string postfix = "]")
    {
        _actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);
            sb.Append(entry.Timestamp.ToString(format));
            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    public LogFormatterBuilder Level(string prefix = "[", string postfix = "]", int rightPadding = 7)
    {
        _actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);
            sb.Append(entry.Level.ToString().PadRight(rightPadding));
            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    public LogFormatterBuilder ContextName(string prefix = "[", string postfix = "]")
    {
        _actions.Add((sb, entry) =>
        {
            if (!string.IsNullOrEmpty(entry.ContextName))
            {
                if (prefix != null) sb.Append(prefix);
                sb.Append(entry.ContextName);
                if (postfix != null) sb.Append(postfix);
            }
        });
        return this;
    }

    public LogFormatterBuilder Message(string prefix = null, string postfix = null)
    {
        _actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);
            sb.Append(entry.Message);
            if (entry.Exception != null)
            {
                sb.AppendLine();
                sb.Append(entry.Exception.ToString());
            }
            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    public Func<LogEntry, string> Build()
    {
        // Copy actions to array to avoid allocation during enumeration
        var actionsArray = _actions.ToArray();
        return entry =>
        {
            var sb = new StringBuilder();
            for (int i = 0; i < actionsArray.Length; i++)
            {
                actionsArray[i](sb, entry);
            }
            return sb.ToString();
        };
    }
}
