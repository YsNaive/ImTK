using ImTK.Core;

namespace ImTK.UI
{
    public class TreeNodeSelectedEvent : UIEventBase
    {
        public TreeNode node;

        protected internal override void Init()
        {
            base.Init();
            node = null;
        }

        public override void Dispose()
        {
            node = null;
        }
    }

    public class TreeNodeExpandedEvent : UIEventBase
    {
        public TreeNode node;
        public bool isExpanded;

        protected internal override void Init()
        {
            base.Init();
            node = null;
            isExpanded = false;
        }

        public override void Dispose()
        {
            node = null;
        }
    }
}
