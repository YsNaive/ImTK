using System;
using System.Collections.Generic;

namespace ImTK;

public interface IVisualElementHierarchy<T>
{
    void Add(T ve);
    void AddRange(IEnumerable<T> ves);
    void Insert(int index, T ve);
    void Remove(T ve);
    void Clear();
    int IndexOf(T ve);
    T ChildAt(int index);
    IEnumerable<T> Children();
    IEnumerable<T> ChildrenRecursive();

    int childrenCount { get; }
    T parent { get; }
}

public class VisualElement : IVisualElementHierarchy<VisualElement>
{
    public VisualElement()
    {
        hierarchy = new Hierarchy(this);

    }

    public bool enable = true;

    public virtual void Update(double deltaTime)
    {

    }
    public void UpdateVisualTree(double deltaTime)
    {
        this.Update(deltaTime);
        foreach (var ve in hierarchy.Children())
        {
            if (ve.enable)
            {
                ve.UpdateVisualTree(deltaTime);
            }
        }
    }

    /// <summary>
    ///  override this to render ImGui content ONLY.<br/>
    ///  if need Begin()/End(), override RenderVisualTree() instead 
    /// </summary>
    public virtual void Render(double deltaTime)
    {

    }

    /// <summary>
    /// override this to use Begin()/End(), remember to render content. <br/>
    /// Example: <br/>
    /// <br/>
    /// <code>
    /// public override void RenderVisualTree()
    /// {
    ///     ImGui.Begin("My Window");
    ///     base.RenderVisualTree();
    ///     ImGui.End("My Window");
    /// }
    /// </code>
    /// </summary>
    public virtual void RenderVisualTree(double deltaTime)
    {
        this.Render(deltaTime);
        foreach(var ve in hierarchy.Children())
        {
            if (ve.enable)
            {
                ve.RenderVisualTree(deltaTime);
            }
        }
    }

    #region Hierarchy Logic

    public class Hierarchy : IVisualElementHierarchy<VisualElement>
    {
        private List<VisualElement> m_children = new List<VisualElement>();
        private VisualElement m_owner;

        public Hierarchy(VisualElement owner)
        {
            m_owner = owner;
        }

        public void Add(VisualElement ve)
        {
            if (m_children.Contains(ve))
            {
                m_children.Remove(ve);
            }
            m_children.Add(ve);
            ve.m_physicalParent = m_owner;
        }

        public void AddRange(IEnumerable<VisualElement> ves)
        {
            foreach (var ve in ves)
            {
                Add(ve);
            }
        }

        public void Insert(int index, VisualElement ve)
        {
            if (m_children.Contains(ve))
            {
                int currentIndex = m_children.IndexOf(ve);
                m_children.Remove(ve);
                if (currentIndex < index)
                {
                    index--;
                }
            }

            if (index >= m_children.Count)
            {
                m_children.Add(ve);
            }
            else
            {
                m_children.Insert(index, ve);
            }
            ve.m_physicalParent = m_owner;
        }

        public void Remove(VisualElement ve)
        {
            if (m_children.Remove(ve))
            {
                ve.m_physicalParent = null;
            }
        }

        public void Clear()
        {
            foreach (var child in m_children)
            {
                child.m_physicalParent = null;
            }
            m_children.Clear();
        }

        public int IndexOf(VisualElement ve)
        {
            return m_children.IndexOf(ve);
        }

        public VisualElement ChildAt(int index)
        {
            if (index < 0 || index >= m_children.Count)
                return null;
            return m_children[index];
        }

        public IEnumerable<VisualElement> Children()
        {
            foreach (var child in m_children)
            {
                yield return child;
            }
        }

        public IEnumerable<VisualElement> ChildrenRecursive()
        {
            foreach (var child in m_children)
            {
                yield return child;
                foreach (var descendant in child.hierarchy.ChildrenRecursive())
                {
                    yield return descendant;
                }
            }
        }

        public int childrenCount
        {
            get { return m_children.Count; }
        }

        public VisualElement parent
        {
            get { return m_owner.m_physicalParent; }
        }
    }

    public readonly Hierarchy hierarchy;
    private VisualElement m_logicalParent = null;
    private VisualElement m_physicalParent = null;
    public virtual VisualElement contentContainer
    {
        get { return this; }
    }
    public VisualElement parent
    {
        get { return m_logicalParent; }
    }

    private bool IsAncestorOf(VisualElement ve)
    {
        VisualElement current = this;
        while (current != null)
        {
            if (current == ve)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    public void Add(VisualElement ve)
    {
        if (ve == null || ve.IsAncestorOf(this))
        {
            return;
        }

        if (ve.parent != null)
        {
            ve.parent.Remove(ve);
        }

        ve.m_logicalParent = this;
        _add(ve);
    }

    private void _add(VisualElement ve)
    {
        if (contentContainer == this)
        {
            hierarchy.Add(ve);
        }
        else
        {
            contentContainer._add(ve);
        }
    }

    public void AddRange(IEnumerable<VisualElement> ves)
    {
        foreach (var ve in ves)
        {
            Add(ve);
        }
    }

    public void Insert(int index, VisualElement ve)
    {
        if (ve == null || ve.IsAncestorOf(this))
        {
            return;
        }

        if (ve.parent != null)
        {
            ve.parent.Remove(ve);
        }

        ve.m_logicalParent = this;
        _insert(index, ve);
    }

    private void _insert(int logicalIndex, VisualElement ve)
    {
        if (contentContainer == this)
        {
            hierarchy.Insert(logicalIndex, ve);
        }
        else
        {
            contentContainer._insert(logicalIndex, ve);
        }
    }

    public void Remove(VisualElement ve)
    {
        if (ve == null || ve.parent != this)
        {
            return;
        }

        _remove(ve);
        ve.m_logicalParent = null;
    }

    private void _remove(VisualElement ve)
    {
        if (contentContainer == this)
        {
            hierarchy.Remove(ve);
        }
        else
        {
            contentContainer._remove(ve);
        }
    }

    public void Clear()
    {
        List<VisualElement> elementsToRemove = new List<VisualElement>();
        foreach (var child in Children())
        {
            elementsToRemove.Add(child);
        }

        foreach (var child in elementsToRemove)
        {
            Remove(child);
        }
    }

    public int IndexOf(VisualElement ve)
    {
        if (ve == null || ve.parent != this)
        {
            return -1;
        }

        int index = 0;
        foreach (var child in Children())
        {
            if (child == ve) return index;
            index++;
        }
        return -1;
    }

    public VisualElement ChildAt(int index)
    {
        if (index < 0) return null;

        int currentIndex = 0;
        foreach (var child in Children())
        {
            if (currentIndex == index) return child;
            currentIndex++;
        }
        return null;
    }

    public IEnumerable<VisualElement> Children()
    {
        foreach (var child in contentContainer.hierarchy.Children())
        {
            if (child.parent == this)
            {
                yield return child;
            }
        }
    }

    public IEnumerable<VisualElement> ChildrenRecursive()
    {
        foreach (var child in Children())
        {
            yield return child;
            foreach (var descendant in child.ChildrenRecursive())
            {
                yield return descendant;
            }
        }
    }

    public int childrenCount
    {
        get
        {
            int count = 0;
            foreach (var child in contentContainer.hierarchy.Children())
            {
                if (child.parent == this)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public VisualElement rootElement
    {
        get
        {
            VisualElement current = this;
            while (current.parent != null)
            {
                current = current.parent;
            }
            return current;
        }
    }
    #endregion
}
