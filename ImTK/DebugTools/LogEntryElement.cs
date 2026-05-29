using Hexa.NET.ImGui;
using ImTK.Log;
using ImTK.UI;

namespace ImTK.DebugTools
{
    public class LogEntryElement : VisualElement
    {
        private LogEntry m_entry;

        public LogEntryElement()
        {
            classList.Add("log-entry");
            // Set some layout padding if necessary
            // style.padding = new Thickness(0, 2);
        }

        public void SetData(LogEntry entry)
        {
            m_entry = entry;
        }

        public override void OnRender()
        {
            if (m_entry.Message == null) return;

            var theme = ImTKTheme.GlobalTheme;

            ImGui.BeginGroup();
            
            RenderEngine.TextColoredBuffered(theme.normalColor.subText.rgba, $"[{m_entry.Timestamp:HH:mm:ss}]");
            
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5f);
            
            if (!string.IsNullOrEmpty(m_entry.ContextName))
            {
                if (m_entry.ContextName == "Global")
                {
                    RenderEngine.TextColoredBuffered(theme.infoColor.text.rgba, $"{m_entry.ContextName}");
                }
                else
                {
                    RenderEngine.TextColoredBuffered(theme.infoColor.text.rgba, $"{m_entry.ContextName}");
                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5f);
            }
            
            Color msgColor = theme.normalColor.text;
            switch (m_entry.Level)
            {
                case LogLevel.Warning:
                    msgColor = theme.warningColor.text;
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    msgColor = theme.dangerColor.text;
                    break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                    msgColor = theme.normalColor.subText;
                    break;
                default: // Info
                    msgColor = theme.normalColor.text;
                    break;
            }
            
            // 使用 TextWrapped，讓過長的 Log 可以自動換行
            ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
            RenderEngine.TextColoredBuffered(msgColor.rgba, $"{m_entry.Message}");
            ImGui.PopTextWrapPos();
            
            ImGui.EndGroup();
        }
    }
}
