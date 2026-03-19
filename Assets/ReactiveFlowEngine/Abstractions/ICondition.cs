using System;
using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface ICondition : IDisposable
    {
        Observable<bool> Evaluate();
        void Reset();
    }
}
