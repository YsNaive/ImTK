using System;

namespace ImTK.UI
{
    public abstract class ValueChangedEvent : UIEventBase
    {
        public abstract object previousValueObj { get; }
        public abstract object newValueObj { get; }
        public bool isInternalChange { get; protected internal set; }
        public bool bubbles { get; internal set; } = false;

        protected internal override void Init()
        {
            base.Init();
            isInternalChange = false;
            bubbles = false;
        }
    }

    public class ValueChangedEvent<T> : ValueChangedEvent
    {
        public T previousValue { get; private set; }
        public T newValue { get; private set; }

        public override object previousValueObj => previousValue;
        public override object newValueObj => newValue;

        protected internal override void Init()
        {
            base.Init();
            previousValue = default;
            newValue = default;
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
