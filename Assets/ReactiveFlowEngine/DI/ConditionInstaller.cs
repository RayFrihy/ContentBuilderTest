using VContainer;
using ReactiveFlowEngine.Conditions;

namespace ReactiveFlowEngine.DI
{
    public static class ConditionInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<ConditionFactory>(Lifetime.Singleton);
        }
    }
}
