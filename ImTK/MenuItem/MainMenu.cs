using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var matched = new List<(MethodInfo method, MainMenuAttribute attr)>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
                    continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (var method in methods)
                        {
                            if (method.GetParameters().Length > 0 || method.ReturnType != typeof(void))
                                continue;

                            var attr = method.GetCustomAttribute<MainMenuAttribute>();
                            if (attr != null)
                            {
                                matched.Add((method, attr));
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore loading errors from inaccessible types
                }
            }

            foreach(var match in matched.OrderByDescending(item => item.attr.priority))
            {
                var action = (Action)Delegate.CreateDelegate(typeof(Action), match.method);
                RegisterItem(match.attr.path, action);
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
            item.clicked += onClick;
            return item;
        }

        private class Module : ImTKModule
        {
            private Module() { }

            public override void Update(double deltaTime)
            {
                // MainMenu currently has no update logic
            }

            public override void Render(double deltaTime)
            {
                ImGui.BeginMainMenuBar();
                root.RenderMenuTree();
                ImGui.EndMainMenuBar();
            }
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
