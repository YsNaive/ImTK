using System;

namespace ImTK;

public interface IFieldElement
{
    void RegisterValueChanged(Action callback);
    void UnregisterValueChanged(Action callback);
}
