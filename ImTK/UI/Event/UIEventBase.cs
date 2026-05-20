using System;

namespace ImTK.UI
{
    public abstract class UIEventBase : IDisposable
    {
        public VisualElement source { get; internal set; }
        public VisualElement current { get; internal set; }
        public bool IsPropagationStopped { get; private set; }
        public virtual bool bubbles { get; } = true;

        public void StopPropagation()
        {
            IsPropagationStopped = true;
        }

        protected internal virtual void Init()
        {
            source = null;
            current = null;
            IsPropagationStopped = false;
        }

        public abstract void Dispose();
    }
}
