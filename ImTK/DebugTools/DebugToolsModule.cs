using ImTK.Core;
using ImTK.UI;

namespace ImTK.DebugTools
{
    public class DebugToolsModule : ImTKModule
    {
        protected DebugToolsModule()
        {
        }

        protected internal override void OnInitializeSelf()
        {
        }

        protected internal override void OnInitializeDependencies()
        {
            if (ImTKEnvironment.IsDevelopment)
            {
                var mainMenu = ImTKApplication.GetModule<MainMenuModule>();
                if (mainMenu != null)
                {
                    mainMenu.AddItem("偵錯/日誌 (Log)", _ => Window.Open<LogViewerWindow>(LogViewerWindow.WindowId), 1000);
                    mainMenu.AddItem("偵錯/效能 (Performance)", _ => Window.Open<PerformanceMonitorWindow>(PerformanceMonitorWindow.WindowId), 1001);
                }
            }
        }
    }
}
