using ImGuiNET;

namespace ImTK;

public class MenuItem : Hierarchy<MenuItem>
{
    public MenuItem(string name) : this(name, null) { }
    public MenuItem(string name, Action clicked)
    {
        this.m_name   = name;
        this.clicked += clicked;
    }

    public string name { get { return m_name; } }
    private string m_name;

    public event Action clicked;

    /// <summary>
    /// if true, will invoke ImGui.CloseCurrentPopup()
    /// </summary>
    public bool isPopup {  get; set; }

    public void RenderMenuTree(bool renderSelf = false)
    {
        if (renderSelf)
            RenderMenuTreeRecursive(this);
        else
        {
            foreach (var item in Children())
            {
                RenderMenuTreeRecursive(item);
            }
        }
    }
    private static void RenderMenuTreeRecursive(MenuItem root)
    {
        if (root.clicked != null)
        {
            if (ImGui.MenuItem(root.name))
            {
                root.clicked.Invoke();
            }
        }
        if (root.childrenCount > 0)
        {
            if (ImGui.BeginMenu(root.name))
            {
                foreach (var childMenuItem in root.Children())
                    childMenuItem.RenderMenuTree(true);
                ImGui.EndMenu();
            }
        }
    }

    /// <summary>
    /// Get or Create MenuItem on related path
    /// </summary>
    /// <param name="path">"Foo/Foo2/..." or "Foo\\Foo2\\..."</param>
    /// <returns>The leaf MenuItem of the created path</returns>
    public MenuItem MakePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return this;
        }

        string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        MenuItem current = this;

        foreach (string part in parts)
        {
            MenuItem next = m_children.Find(item => item.name == part);

            if (next == null)
            {
                // 若不存在則建立新的 MenuItem
                // 由於 onClick 在中間路徑通常為 null，此處傳入 null
                next = new MenuItem(part, null);
                current.Add(next);
            }
            current = next;
        }

        return current;
    }

    /// <param name="path">"Foo/Foo2/..." 或 "Foo\\Foo2\\..."</param>
    /// <returns>If path exist return true，otherwise false</returns>
    public bool ExistPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return true;
        }

        string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        MenuItem current = this;

        foreach (string part in parts)
        {
            MenuItem next = current.m_children.Find(item => item.name == part);

            if (next == null)
            {
                return false;
            }
            current = next;
        }

        return true;
    }
}
