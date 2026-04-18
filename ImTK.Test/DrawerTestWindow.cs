using System;
using System.Numerics;
using ImGuiNET;
using ImTK;

namespace ImTK.Test;

public class DrawerTestWindow : Window
{
    [MainMenu("Window/Drawer Test")]
    public static void OpenDrawerTestWindow()
    {
        Window.Open<DrawerTestWindow>();
    }

    public override string displayName => "Drawer Test";

    private Vector3Drawer m_vector3Drawer;
    private FoldoutDrawer m_foldoutDrawer;

    public DrawerTestWindow()
    {
        m_foldoutDrawer = new FoldoutDrawer("Settings", true);

        var intDrawer = new IntDrawer("Max Count", 10);
        intDrawer.RegisterValueChanged(() => Console.WriteLine($"Max Count changed to {intDrawer.value}"));
        m_foldoutDrawer.Add(intDrawer);

        var strDrawer = new TextDrawer("Name", "ImTK");
        strDrawer.RegisterValueChanged(() => Console.WriteLine($"Name changed to {strDrawer.value}"));
        m_foldoutDrawer.Add(strDrawer);

        Add(m_foldoutDrawer);

        m_vector3Drawer = new Vector3Drawer("Position", new Vector3(1, 2, 3));
        m_vector3Drawer.RegisterValueChanged(() => Console.WriteLine($"Position changed to {m_vector3Drawer.value}"));
        Add(m_vector3Drawer);
    }
}
