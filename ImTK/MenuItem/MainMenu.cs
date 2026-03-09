using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ImTK
{
    public static class MainMenu
    {
        // [private static] 符合 s_ 前綴規範
        private static MenuItem s_root;
        public static MenuItem root { get { return s_root; } }

        static MainMenu()
        {
            s_root = new MenuItem("main-menu-root", null); 

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 略過系統組件以提升效能 (可選)
                if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    // 搜尋所有靜態方法
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    List<Tuple<MethodInfo, MainMenuAttribute>> matched = new();
                    foreach (var method in methods)
                    {
                        // 檢查是否純 Action
                        if (method.GetParameters().Length > 0)
                            continue;

                        // 檢查是否有 MainMenuAttribute
                        var attr = method.GetCustomAttribute<MainMenuAttribute>();
                        if (attr == null) 
                            continue;

                        matched.Add(new(method, attr));
                    }
                    foreach(var match in  matched.OrderBy(item => -item.Item2.priority))
                    {
                        var action = (Action)Delegate.CreateDelegate(typeof(Action), match.Item1);
                        RegisterItem(match.Item2.path, action);
                    }
                }
            }
        }

        public static MenuItem RegisterItem(string path)
        {
            var item = root.MakePath(path);
            return item;
        }

        public static MenuItem RegisterItem(string path, Action onClick)
        {
            var item = RegisterItem(path);
            item.onClick += onClick;
            return item;
        }

        public static void RenderAll()
        {
            ImGui.BeginMainMenuBar();
            foreach(var menu in root.Children())
                menu.Render();
            ImGui.EndMainMenuBar();
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MainMenuAttribute : Attribute
    {
        public MainMenuAttribute(string path, int priority = 0) {
            this.path = path;
            this.priority = priority;
        }

        public string path;
        public int priority;
    }
}
