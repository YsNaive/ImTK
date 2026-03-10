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

        ImTKSilk.Initialize(constant);
        ImTKSilk.Start();
    }
}

public class AppTestModule : ImTKModule
{
    private AppTestModule() { }

    public override void OnLoad()
    {
        Console.WriteLine("AppTestModule Loaded! Opening windows...");
        WindowView.Open<TestWindow>();
    }

    public override void OnClose()
    {
        Console.WriteLine("AppTestModule Closed! Saving cache...");
    }

    public override void Update(double deltaTime)
    {
        // Add specific app-wide update logic if any
    }

    public override void Render(double deltaTime)
    {
        // Add specific app-wide rendering logic if any
    }
}

public class TestWindow : WindowView
{
    public override string displayName => "Custom Test Window";

    public TestWindow()
    {
        // Add a simple visual element or text (If VisualElement API supports text directly)
    }

    public override void RenderVisualTree(double deltaTime)
    {
        base.RenderVisualTree(deltaTime);
        ImGui.Text("Hello from TestWindow!");
        if (ImGui.Button("Click Me!"))
        {
            Console.WriteLine("Button Clicked in TestWindow!");
        }
    }
}