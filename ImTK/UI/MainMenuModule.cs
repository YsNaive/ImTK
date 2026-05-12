using System;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;

namespace ImTK.UI
{
    public class MainMenuModule : ImTKModule
    {
        private MenuView m_rootMenu;
        private ImRect m_reservedRect;
        private static readonly System.Reflection.MethodInfo s_renderMethod = typeof(VisualElement).GetMethod("Render", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        protected MainMenuModule()
        {
            m_rootMenu = new MenuView("MainMenu") { isMenuBar = true };
        }

        protected internal override void OnInitializeSelf()
        {
            // 向 Panel 註冊保留區域，設定較高的優先權
            ImTKApplication.GetModule<Panel>().RequireArea(RequireMenuArea, priority: 100);
        }

        protected internal override void OnInitializeDependencies()
        {
        }

        private ImRect RequireMenuArea(ImRect available)
        {
            float menuHeight = ImGui.GetFrameHeight() * 1.25f; // 上下 padding 0.125

            // 記錄自己取得的空間，供 OnGuiRender 繪製使用
            m_reservedRect = new ImRect(available.min, new Vector2(available.max.X, available.min.Y + menuHeight));

            // 回傳剩餘空間給 Panel
            return new ImRect(new Vector2(available.min.X, available.min.Y + menuHeight), available.max);
        }

        protected internal override void OnGuiRender()
        {
            // 在保留的區域上開啟無邊框視窗
            ImGui.SetNextWindowPos(m_reservedRect.min);
            ImGui.SetNextWindowSize(new Vector2(m_reservedRect.max.X - m_reservedRect.min.X, m_reservedRect.max.Y - m_reservedRect.min.Y));

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);

            bool isOpen = ImGui.Begin("##MainMenuBarContainer", windowFlags);

            ImGui.PopStyleVar(3);

            if (isOpen)
            {
                // 因為是 MenuBar，ImGui 要求在 BeginMenuBar 之前必須處於支援 MenuBar 的 Window 中
                // 我們在上面已經加上了 ImGuiWindowFlags.MenuBar。
                // 呼叫 MenuView 進行內部渲染。
                if (s_renderMethod != null)
                {
                    s_renderMethod.Invoke(m_rootMenu, null);
                }
                ImGui.End();
            }
        }

        protected internal override void OnLogicUpdate()
        {
        }

        protected internal override void OnClose()
        {
        }

        /// <summary>
        /// 提供透過字串路徑快速建立/尋找節點的語法糖，導向至全域的 MainMenu。
        /// </summary>
        public MenuItem AddItem(string path, Action<ClickEvent> onClicked, int priority = 0)
        {
            return m_rootMenu.AddItem(path, onClicked, priority);
        }
    }
}
