using VContainer;
using ReactiveFlowEngine.Behaviors;

namespace ReactiveFlowEngine.DI
{
    public static class BehaviorInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<BehaviorFactory>(Lifetime.Singleton);
        }
    }
}
