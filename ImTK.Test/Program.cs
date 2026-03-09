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
        // 註冊時直接傳入邏輯
        ImTKSilk.Initialize(new ImTKSilkConstant
        {
            WindowTitle = "ImTK Action Test",
            WindowWidth = 1280,
            WindowHeight = 720,
            OnUpdate = (dt) =>
            {
                // 更新邏輯
            },
            OnRender = (dt) =>
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
            }
        });

        ImTKSilk.Start();
    }
}