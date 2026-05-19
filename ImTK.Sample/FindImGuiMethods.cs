using System;
using System.Linq;
using System.Reflection;
using ImGuiNET;

public class FindImGuiMethods {
    public static void Run() {
        var methods = typeof(ImGui).GetMethods().Select(m => m.Name).Where(n => n.Contains("Dock")).Distinct();
        foreach (var m in methods) {
            Console.WriteLine(m);
        }
    }
}
