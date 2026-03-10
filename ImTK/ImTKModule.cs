using System;
using System.Collections.Generic;

namespace ImTK;

public abstract class ImTKModule
{
    private static readonly List<ImTKModule> modules = new List<ImTKModule>();

    protected ImTKModule() { }

    private static void Register(ImTKModule module)
    {
        if (module != null && !modules.Contains(module))
        {
            modules.Add(module);
        }
    }

    public static void InitializeAll()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
                continue;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(ImTKModule)))
                    {
                        var module = (ImTKModule)Activator.CreateInstance(type, nonPublic: true);
                        Register(module);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore assembly loading errors
            }
        }
    }

    public static void UpdateAll(double deltaTime)
    {
        // Safe iteration in case a module modifies the list
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Update(deltaTime);
        }
    }

    public static void RenderAll(double deltaTime)
    {
        // Safe iteration
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Render(deltaTime);
        }
    }

    public abstract void Update(double deltaTime);
    public abstract void Render(double deltaTime);
}
