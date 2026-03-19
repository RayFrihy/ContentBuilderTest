using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IEventBus
    {
        void Publish(string eventName, object payload = null);
        Observable<object> On(string eventName);
    }
}
