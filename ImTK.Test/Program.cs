using ImGuiNET;
using ImTK;
using ImTK.Silk;

class Program
{
    [MainMenu("Custom/Foo")]
    public static void Foo()
    {
        Console.WriteLine("Foo");
    }
    [MainMenu("Custom/Foo2")]
    public static void Foo2()
    {
        Console.WriteLine("Foo2");
    }
    [MainMenu("File/Save", 1)]
    public static void Save()
    {
        Console.WriteLine("Save");
    }
    [MainMenu("File/Load")]
    public static void Load()
    {
        Console.WriteLine("Load");
    }

    public static void Main(string[] args)
    {
        RunImTKSilk();
    }

    static void RunImTKSilk()
    {
        ImTKSilkConstant constant = new ImTKSilkConstant
        {
            title = "ImTK Action Test",
            width = 1280,
            height = 720,
        };

        ImTKSilk.onUpdate += (dt) =>
        {

        };

        ImTKSilk.onRender += (dt) =>
        {
            MainMenu.RenderAll();

            ImGui.Begin("Dock Test Window 1");
            ImGui.Text("Window 1");
            ImGui.End();

            ImGui.Begin("Dock Test Window 2");
            ImGui.Text("Window 2");
            ImGui.End();

            ImGui.Begin("Action Window");
            ImGui.Text($"Delta Time: {dt}");
            if (ImGui.Button("Close Window"))
            {
            }
            ImGui.End();
        };

        ImTKSilk.Initialize(constant);
        ImTKSilk.Start();
    }
}