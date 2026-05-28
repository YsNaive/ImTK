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

            // 1. Time [HH:mm:ss]
            ImGui.TextColored(theme.normalColor.subText.rgba, $"[{m_entry.Timestamp:HH:mm:ss}]");
            if (ImGui.IsItemHovered())
            {
                var mousePos = ImGui.GetMousePos();
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(mousePos.X, mousePos.Y - 4.0f), ImGuiCond.Always, new System.Numerics.Vector2(0.0f, 1.0f));
                if (ImGui.BeginTooltip())
                {
                    ImGui.TextColored(theme.infoColor.text.rgba, m_entry.ContextName);
                    ImGui.EndTooltip();
                }
            }
            ImGui.SameLine();

            // 2. Message
            Color msgColor;
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
            ImGui.TextColored(msgColor.rgba, m_entry.Message);
            ImGui.PopTextWrapPos();
        }
    }
}
