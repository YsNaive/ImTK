using System.Collections.Generic;

namespace ImTK.UI
{
    public class RenderListCache
    {
        public bool isDirty = true;
        public readonly List<RenderOp> renderList = new List<RenderOp>();

        public void MarkDirty()
        {
            isDirty = true;
        }

        public void Update(VisualElement root)
        {
            if (!isDirty) return;
            renderList.Clear();
            RenderEngine.BuildRenderListRecursive(root, renderList);
            isDirty = false;
        }
    }
}
