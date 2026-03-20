using System;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class BehaviorFactoryTests
    {
        private BehaviorFactory _factory;
        private MockSceneObjectResolver _resolver;
        private MockStateStore _stateStore;
        private MockStepRunner _stepRunner;
        private MockEventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _resolver = new MockSceneObjectResolver();
            _stateStore = new MockStateStore();
            _stepRunner = new MockStepRunner();
            _eventBus = new MockEventBus();

            // Create a minimal condition factory for behaviors that need it
            var condFactory = new Conditions.ConditionFactory(_resolver, _eventBus, _stateStore, null);

            _factory = new BehaviorFactory(
                _resolver, _stateStore, null, null, _stepRunner, _eventBus, condFactory);
        }

        [Test]
        public void Create_NullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.Create(null));
        }

        [Test]
        public void Create_UnknownType_ThrowsArgumentException()
        {
            var def = new BehaviorDefinition { TypeName = "NonExistentBehavior" };
            Assert.Throws<ArgumentException>(() => _factory.Create(def));
        }

        [Test]
        public void Create_DelayBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "DelayBehavior" };
            def.Parameters["Duration"] = 2.5f;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<DelayBehavior>(behavior);
        }

        [Test]
        public void Create_EnableObjectBehavior_ReturnsSetActiveBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "EnableObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetActiveBehavior>(behavior);
        }

        [Test]
        public void Create_DisableObjectBehavior_ReturnsSetActiveBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "DisableObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetActiveBehavior>(behavior);
        }

        [Test]
        public void Create_ShowObjectBehavior_ReturnsSetRendererVisibilityBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "ShowObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IncludeChildren"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetRendererVisibilityBehavior>(behavior);
        }

        [Test]
        public void Create_HideObjectBehavior_ReturnsSetRendererVisibilityBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "HideObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IncludeChildren"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetRendererVisibilityBehavior>(behavior);
        }

        [Test]
        public void Create_SetStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SetStateBehavior" };
            def.Parameters["Key"] = "testKey";
            def.Parameters["Value"] = "testValue";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetStateBehavior>(behavior);
        }

        [Test]
        public void Create_TriggerEventBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "TriggerEventBehavior" };
            def.Parameters["EventName"] = "TestEvent";
            def.Parameters["IsBlocking"] = false;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<TriggerEventBehavior>(behavior);
        }

        [Test]
        public void Create_LoadSceneBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "LoadSceneBehavior" };
            def.Parameters["SceneName"] = "TestScene";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<LoadSceneBehavior>(behavior);
        }
    }
}
