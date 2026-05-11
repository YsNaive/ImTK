using System.Collections.Generic;

namespace ImTK.UI
{
    public interface IVisualElementHierarchy
    {
        VisualElement parent { get; }
        int childCount { get; }
        VisualElement childAt(int index);
        void Add(VisualElement child);
        void Remove(VisualElement child);
        void Clear();
        IEnumerable<VisualElement> Children();
    }
}
