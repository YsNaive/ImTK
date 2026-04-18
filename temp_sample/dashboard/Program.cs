using ImTK.Silk;

namespace dashboard
{
    internal class Program
    {
        static void Main(string[] args)
        {
            dashboard.Dashboard.Core.CacheHandler.Initialize();
            dashboard.Dashboard.Core.Registry.Initialize();

            ImTKSilkConstant constant = new()
            {
                title  = "VEX Dashboard",
                width  = 800,
                height = 600,
                fontSize = 18,
                fontPath = "C:\\Windows\\Fonts\\jf-openhuninn-2.1.ttf"
            };
            ImTKSilk.Initialize(constant);
            ImTKSilk.Start();
        }
    }
}
