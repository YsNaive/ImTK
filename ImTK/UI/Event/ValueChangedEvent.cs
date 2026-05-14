using System;

namespace ImTK.UI
{
    public class ValueChangedEvent<T> : UIEventBase, IValueChangedEvent
    {
        public T previousValue { get; private set; }
        public T newValue { get; private set; }
        public bool isInternalChange { get; private set; }
        public bool bubbles { get; internal set; } = false;

        public object previousValueObj => previousValue;
        public object newValueObj => newValue;

        protected internal override void Init()
        {
            base.Init();
            previousValue = default;
            newValue = default;
            isInternalChange = false;
            bubbles = false;
        }

        public override void Dispose()
        {
            EventPool<ValueChangedEvent<T>>.Release(this);
        }

        public static ValueChangedEvent<T> GetPooled(T previousValue, T newValue, bool isInternalChange = false)
        {
            var evt = EventPool<ValueChangedEvent<T>>.Get();
            evt.previousValue = previousValue;
            evt.newValue = newValue;

            if (isInternalChange)
            {
                evt.isInternalChange = true;
            }
            else
            {
                evt.isInternalChange = typeof(T).IsClass && ReferenceEquals(previousValue, newValue);
            }

            return evt;
        }
    }
}
