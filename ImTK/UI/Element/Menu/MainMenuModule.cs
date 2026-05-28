using System;
using System.Numerics;
using ImGuiNET;
using ImTK.Core;
using System.Linq;
using System.Reflection;
using ImTK.Log;

namespace ImTK.UI
{
    public class MainMenuModule : ImTKModule
    {
        private MenuView m_rootMenu;
        private Rect m_reservedRect;


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
            ScanAndRegisterMenuAttributes();
        }

        private void ScanAndRegisterMenuAttributes()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
                .SelectMany(a => {
                    try { return a.GetTypes(); } catch { return new Type[0]; }
                });

            foreach (var type in allTypes)
            {
                // Scan Methods
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<MainMenuAttribute>();
                    if (attr != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0)
                        {
                            Action<ClickEvent> wrapper = (evt) => method.Invoke(null, null);
                            AddItem(attr.path, wrapper, attr.priority);
                        }
                        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ClickEvent))
                        {
                            Action<ClickEvent> wrapper = (Action<ClickEvent>)Delegate.CreateDelegate(typeof(Action<ClickEvent>), method);
                            AddItem(attr.path, wrapper, attr.priority);
                        }
                        else
                        {
                            ImTKLog.Error($"Method '{method.Name}' with MainMenuAttribute must have either no parameters or a single ClickEvent parameter.");
                        }
                    }
                }

                // Scan Fields
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    var attr = field.GetCustomAttribute<MainMenuAttribute>();
                    if (attr != null)
                    {
                        if (typeof(MenuView).IsAssignableFrom(field.FieldType))
                        {
                            var menuView = field.GetValue(null) as MenuView;
                            AddMenu(attr.path, menuView, attr.priority);
                        }
                        else
                        {
                            ImTKLog.Error($"Field '{field.Name}' with MainMenuAttribute must be of type MenuView.");
                        }
                    }
                }

                // Scan Properties
                var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var prop in properties)
                {
                    var attr = prop.GetCustomAttribute<MainMenuAttribute>();
                    if (attr != null)
                    {
                        if (typeof(MenuView).IsAssignableFrom(prop.PropertyType) && prop.CanRead)
                        {
                            var menuView = prop.GetValue(null) as MenuView;
                            AddMenu(attr.path, menuView, attr.priority);
                        }
                        else
                        {
                            ImTKLog.Error($"Property '{prop.Name}' with MainMenuAttribute must be of type MenuView and be readable.");
                        }
                    }
                }
            }
        }

        private Rect RequireMenuArea(Rect available)
        {
            float menuHeight = ImGui.GetFrameHeight() * 1.25f; // 上下 padding 0.125

            // 記錄自己取得的空間，供 OnGuiRender 繪製使用
            m_reservedRect = new Rect(available.x, available.y, available.width, menuHeight);

            // 回傳剩餘空間給 Panel
            return new Rect(available.x, available.y + menuHeight, available.width, available.height - menuHeight);
        }

        protected internal override void OnGuiRender()
        {
            // 由於 m_reservedRect 的高度為 FrameHeight * 1.25f (包含 padding)
            // 若視窗高度也設為 1.25f，多出的 0.25f 透明區域會阻擋滑鼠點擊下方的 Docking 標題列。
            // 因此，視窗本身的高度只設為 MenuBar 需要的標準 FrameHeight，並根據 padding 調整位置。
            float frameHeight = ImGui.GetFrameHeight();
            float padding = frameHeight * 0.125f;

            // 在保留的區域內開啟無邊框視窗，位置往下推 padding，高度只取 frameHeight
            ImGui.SetNextWindowPos(new Vector2(m_reservedRect.x, m_reservedRect.y + padding));
            ImGui.SetNextWindowSize(new Vector2(m_reservedRect.width, frameHeight));

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoBackground;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            // 覆寫 ImGui 預設的 WindowMinSize，避免因預設值大於 frameHeight 而強行擴張高度，導致 Hit-box 擋住下方物件。
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(0.0f, 0.0f));

            bool isOpen = ImGui.Begin("##MainMenuBarContainer", windowFlags);

            ImGui.PopStyleVar(4);

            if (isOpen)
            {
                // 因為是 MenuBar，ImGui 要求在 BeginMenuBar 之前必須處於支援 MenuBar 的 Window 中
                // 我們在上面已經加上了 ImGuiWindowFlags.MenuBar。
                if (m_rootMenu != null)
                {
                    m_rootMenu.RenderCache.Update(m_rootMenu);
                    RenderEngine.ComputeStyleFlat(m_rootMenu.RenderCache.renderList);
                    RenderEngine.RenderFlat(m_rootMenu.RenderCache.renderList);
                }
            }

            // ImGui.Begin() 無論回傳 true 或 false，都必須配對呼叫 ImGui.End()
            ImGui.End();
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

        /// <summary>
        /// 提供將動態 MenuView 實例掛載到全域 MainMenu 指定路徑下的語法糖。
        /// </summary>
        public void AddMenu(string parentPath, MenuView view, int priority = 0)
        {
            m_rootMenu.AddMenu(parentPath, view, priority);
        }
    }
}
