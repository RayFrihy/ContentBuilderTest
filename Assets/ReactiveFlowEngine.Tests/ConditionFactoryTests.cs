using System;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions;
using ReactiveFlowEngine.Conditions.Interaction;
using ReactiveFlowEngine.Conditions.TimeBased;
using ReactiveFlowEngine.Conditions.State;
using ReactiveFlowEngine.Conditions.Composite;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class ConditionFactoryTests
    {
        private ConditionFactory _factory;
        private MockSceneObjectResolver _resolver;
        private MockEventBus _eventBus;
        private MockStateStore _stateStore;

        [SetUp]
        public void SetUp()
        {
            _resolver = new MockSceneObjectResolver();
            _eventBus = new MockEventBus();
            _stateStore = new MockStateStore();
            _factory = new ConditionFactory(_resolver, _eventBus, _stateStore, null);
        }

        [Test]
        public void Create_NullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.Create(null));
        }

        [Test]
        public void Create_UnknownType_ThrowsArgumentException()
        {
            var def = new ConditionDefinition { TypeName = "NonExistentCondition" };
            Assert.Throws<ArgumentException>(() => _factory.Create(def));
        }

        [Test]
        public void Create_TimeoutCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "TimeoutCondition" };
            def.Parameters["Timeout"] = 5.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<TimeoutCondition>(condition);
        }

        [Test]
        public void Create_ButtonPressedCondition_ReturnsEventBusCondition()
        {
            var def = new ConditionDefinition { TypeName = "ButtonPressedCondition" };
            def.Parameters["ButtonId"] = "btn1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<EventBusCondition>(condition);
        }

        [Test]
        public void Create_ObjectTouchedCondition_ReturnsEventBusCondition()
        {
            var def = new ConditionDefinition { TypeName = "ObjectTouchedCondition" };
            def.Parameters["TargetObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<EventBusCondition>(condition);
        }

        [Test]
        public void Create_BooleanStateCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "BooleanStateCondition" };
            def.Parameters["StateKey"] = "testKey";
            def.Parameters["ExpectedValue"] = true;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<BooleanStateCondition>(condition);
        }

        [Test]
        public void Create_CompositeAndCondition_WithChildren_CombinesCorrectly()
        {
            var child1 = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child1.Parameters["Timeout"] = 1.0f;

            var child2 = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child2.Parameters["Timeout"] = 2.0f;

            var def = new ConditionDefinition { TypeName = "CompositeAndCondition" };
            def.Children.Add(child1);
            def.Children.Add(child2);

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<CompositeAndCondition>(condition);
        }

        [Test]
        public void Create_DelayElapsedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "DelayElapsedCondition" };
            def.Parameters["Delay"] = 3.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<DelayElapsedCondition>(condition);
        }

        [Test]
        public void Create_AllConsolidatedInteractionConditions_ReturnEventBusCondition()
        {
            var types = new[]
            {
                ("ObjectGrabbedCondition", "TargetObjectId"),
                ("ObjectReleasedCondition", "TargetObjectId"),
                ("ObjectTouchedCondition", "TargetObjectId"),
                ("ObjectUsedCondition", "TargetObjectId"),
                ("ObjectSelectedCondition", "TargetObjectId"),
                ("ObjectDeselectedCondition", "TargetObjectId"),
                ("ButtonPressedCondition", "ButtonId"),
                ("ButtonReleasedCondition", "ButtonId"),
                ("InputActionTriggeredCondition", "ActionName"),
            };

            foreach (var (typeName, paramKey) in types)
            {
                var def = new ConditionDefinition { TypeName = typeName };
                def.Parameters[paramKey] = "test-id";

                var condition = _factory.Create(def);
                Assert.IsNotNull(condition, $"Failed for {typeName}");
                Assert.IsInstanceOf<EventBusCondition>(condition, $"Failed for {typeName}");
            }
        }
    }
}
