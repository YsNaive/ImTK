using System;
using System.Linq;
using System.Reflection;
using ImGuiNET;

public class FindImGuiMethods {
    public static void Main() {
        var methods = typeof(ImGui).GetMethods().Select(m => m.Name).Where(n => n.Contains("Dock")).Distinct();
        foreach (var m in methods) {
            Console.WriteLine(m);
        }

        // Also look at structs that contain Dock
        var types = typeof(ImGui).Assembly.GetTypes().Where(t => t.Name.Contains("Dock"));
        foreach(var t in types) {
            Console.WriteLine("Type: " + t.Name);
            foreach(var m in t.GetMethods()) {
                Console.WriteLine("  Method: " + m.Name);
            }
        }
    }
}
