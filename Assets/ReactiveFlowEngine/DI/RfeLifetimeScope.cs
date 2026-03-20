using UnityEngine;
using VContainer;
using VContainer.Unity;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Navigation;
using ReactiveFlowEngine.Serialization;
using ReactiveFlowEngine.State;

namespace ReactiveFlowEngine.DI
{
    public class RfeLifetimeScope : LifetimeScope
    {
        [SerializeField] private TextAsset _processJson;

        protected override void Configure(IContainerBuilder builder)
        {
            // Core engine
            builder.Register<FlowEngine>(Lifetime.Singleton)
                .As<IFlowEngine>()
                .As<IEngineController>()
                .AsSelf();
            builder.Register<StepRunner>(Lifetime.Singleton).As<IStepRunner>();
            builder.Register<TransitionEvaluator>(Lifetime.Singleton).As<ITransitionEvaluator>();
            builder.Register<ChapterRunner>(Lifetime.Singleton);

            // Navigation
            builder.Register<NavigationService>(Lifetime.Singleton)
                .As<INavigationService>()
                .AsSelf();

            // State & History
            builder.Register<StateStore>(Lifetime.Singleton).As<IStateStore>().AsSelf();
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            builder.Register<HistoryStack>(Lifetime.Singleton).As<IHistoryService>();

            // Serialization
            builder.Register<VRBuilderJsonLoader>(Lifetime.Singleton).As<IProcessLoader>();
            builder.Register<ModelBuilder>(Lifetime.Singleton);
            builder.Register<RfeTypeBinder>(Lifetime.Singleton);

            // Factories
            builder.Register<Conditions.ConditionFactory>(Lifetime.Singleton);
            builder.Register<Behaviors.BehaviorFactory>(Lifetime.Singleton);

            // Scene integration
            builder.Register<Runtime.SceneObjectResolver>(Lifetime.Singleton).As<ISceneObjectResolver>();

            // Configuration
            if (_processJson != null)
                builder.RegisterInstance(_processJson);

            // Entry point
            builder.RegisterEntryPoint<Runtime.ProcessRunner>();
        }
    }
}
