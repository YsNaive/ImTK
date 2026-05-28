using System;
using System.Collections.Generic;
using System.Text;

namespace ImTK.Log;

public class LogFormatterBuilder
{
    private readonly List<Action<StringBuilder, LogEntry>> m_actions = new();

    public LogFormatterBuilder Text(string text)
    {
        m_actions.Add((sb, entry) => sb.Append(text));
        return this;
    }

    public LogFormatterBuilder Timestamp(string format = "HH:mm:ss", string prefix = "[", string postfix = "]")
    {
        m_actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);
            sb.Append(entry.Timestamp.ToString(format));
            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    public LogFormatterBuilder TimeSinceStartup(string format = @"hh\:mm\:ss", string prefix = "[", string postfix = "]")
    {
        m_actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);
            var span = entry.Timestamp - ImTK.Core.Time.StartupTime;
            sb.Append(span.ToString(format));
            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    public LogFormatterBuilder Level(string prefix = "[", string postfix = "]", int width = 7)
    {
        m_actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);

            string levelStr = entry.Level.ToString();
            int totalPad = width - levelStr.Length;
            if (totalPad > 0)
            {
                int leftPad = totalPad / 2;
                int rightPad = totalPad - leftPad;
                sb.Append(' ', leftPad).Append(levelStr).Append(' ', rightPad);
            }
            else
            {
                sb.Append(levelStr);
            }

            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    public LogFormatterBuilder ContextName(string prefix = "[", string postfix = "]")
    {
        m_actions.Add((sb, entry) =>
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
        m_actions.Add((sb, entry) =>
        {
            if (prefix != null) sb.Append(prefix);
            sb.Append(entry.Message);
            if (entry.Exception != null)
            {
                sb.AppendLine();
                if (entry.IncludeStackTrace)
                {
                    sb.Append(entry.Exception.ToString());
                }
                else
                {
                    sb.Append($"[{entry.Exception.GetType().Name}] {entry.Exception.Message}");
                }
            }
            if (postfix != null) sb.Append(postfix);
        });
        return this;
    }

    [ThreadStatic]
    private static StringBuilder t_sb;

    public Func<LogEntry, string> Build()
    {
        // Copy actions to array to avoid allocation during enumeration
        var actionsArray = m_actions.ToArray();
        return entry =>
        {
            if (t_sb == null) t_sb = new StringBuilder(512);
            t_sb.Clear();
            for (int i = 0; i < actionsArray.Length; i++)
            {
                actionsArray[i](t_sb, entry);
            }
            return t_sb.ToString();
        };
    }
}
