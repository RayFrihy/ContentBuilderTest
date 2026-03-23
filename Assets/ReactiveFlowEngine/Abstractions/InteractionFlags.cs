using System;

namespace ReactiveFlowEngine.Abstractions
{
    [Flags]
    public enum InteractionFlags
    {
        None = 0,
        Hoverable = 1,
        Touchable = 2,
        Grabbable = 4,
        Usable = 8,
        Selectable = 16,
        All = Hoverable | Touchable | Grabbable | Usable | Selectable
    }
}
