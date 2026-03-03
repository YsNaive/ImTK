using System;
using System.Collections.Generic;
using System.Text;

namespace ImTK;

public interface IHierarchy<T>
{
    T parent { get; }
    int childrenCount { get; }
    IEnumerable<T> Children();
    void Add(T item);
    void Remove(T item);
    void Insert(int index, T item);
    void Clear();
}

public class Hierarchy<T> : IHierarchy<T> where T : Hierarchy<T>
{
    protected T m_parent;
    protected List<T> m_children;
    public int childrenCount => m_children.Count;

    public T parent
    {
        get => m_parent;
    }


    public Hierarchy()
    {
        m_children = new List<T>();
    }

    public IEnumerable<T> Children()
    {
        return m_children;
    }

    public void Add(T item)
    {
        // 檢查 null 以及是否已存在相同引用 (Set behavior)
        if (item != null && !m_children.Contains(item))
        {
            // 若該項目已有父節點，應先從原父節點移除
            item.m_parent?.Remove(item);

            item.m_parent = (T)this;
            m_children.Add(item);
        }
    }

    public void Remove(T item)
    {
        if (item != null && m_children.Remove(item))
        {
            item.m_parent = null;
        }
    }

    public void Insert(int index, T item)
    {
        if (item != null && !m_children.Contains(item))
        {
            item.m_parent?.Remove(item);

            item.m_parent = (T)this;
            m_children.Insert(index, item);
        }
    }

    public void Clear()
    {
        foreach (var child in m_children)
        {
            child.m_parent = null;
        }
        m_children.Clear();
    }
}