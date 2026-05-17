using System;
using System.Collections.Generic;
using ImTK.Core;

namespace ImTK.UI.Style
{
    public class StyleClass
    {
        private HashSet<HashedString> m_classes;

        public Action OnClassChanged { get; set; }

        public bool Has(HashedString className)
        {
            if (m_classes == null) return false;
            return m_classes.Contains(className);
        }

        public void Add(HashedString className)
        {
            if (m_classes == null) m_classes = new HashSet<HashedString>();

            if (m_classes.Add(className))
            {
                OnClassChanged?.Invoke();
            }
        }

        public void Remove(HashedString className)
        {
            if (m_classes != null && m_classes.Remove(className))
            {
                OnClassChanged?.Invoke();
            }
        }

        public void Toggle(HashedString className)
        {
            if (Has(className))
            {
                Remove(className);
            }
            else
            {
                Add(className);
            }
        }

        public void Clear()
        {
            if (m_classes != null && m_classes.Count > 0)
            {
                m_classes.Clear();
                OnClassChanged?.Invoke();
            }
        }

        public IEnumerable<HashedString> GetClasses()
        {
            if (m_classes == null) yield break;
            foreach (var c in m_classes)
            {
                yield return c;
            }
        }
    }
}
