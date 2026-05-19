using ImGuiNET;
using System;
using System.Reflection;

public class DockBuilderTest {
    public static void Main() {
        var methods = typeof(ImGui).GetMethods();
        foreach (var m in methods) {
            if (m.Name.Contains("Dock")) {
                Console.WriteLine(m.Name);
            }
        }
    }
}
