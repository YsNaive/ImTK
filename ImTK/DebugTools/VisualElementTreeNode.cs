using ImTK.UI;
using System.Collections.Generic;

namespace ImTK.DebugTools
{
    public class VisualElementTreeNode : TreeNode
    {
        public VisualElement targetElement { get; private set; }

        public VisualElementTreeNode(VisualElement target) 
            : base($"{target.GetType().Name} [ID:{target.m_elementId}]" + (string.IsNullOrEmpty(target.persistenceKey) ? "" : $" ({target.persistenceKey})"))
        {
            this.targetElement = target;
            UpdateIconVisibility();
        }

        public override bool isLeaf => targetElement == null || targetElement.hierarchy.childCount == 0;

        protected override void OnExpand()
        {
            base.OnExpand();
            
            ImTK.Core.ImTKApplication.ScheduleDeferred(() =>
            {
                // Generate child nodes lazily
                this.Clear();

                if (targetElement != null)
                {
                    int childCount = targetElement.hierarchy.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        var child = targetElement.hierarchy.ChildAt(i);
                        this.Add(new VisualElementTreeNode(child));
                    }
                }
            });
        }
    }
}
