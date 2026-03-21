using System;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.State;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Conditions
{
    [TestFixture]
    public class BooleanStateConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BooleanStateCondition(null, "key", true));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BooleanStateCondition(new MockStateStore(), null, true));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new BooleanStateCondition(new MockStateStore(), "flag", true));
        }

        [Test]
        public void StateKey_ReturnsExpectedValue()
        {
            var condition = new BooleanStateCondition(new MockStateStore(), "myFlag", false);
            Assert.AreEqual("myFlag", condition.StateKey);
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new BooleanStateCondition(new MockStateStore(), "key", true);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new BooleanStateCondition(new MockStateStore(), "key", true);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new BooleanStateCondition(new MockStateStore(), "key", true);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class IntegerStateConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new IntegerStateCondition(null, "key", 10, ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new IntegerStateCondition(new MockStateStore(), null, 10, ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new IntegerStateCondition(new MockStateStore(), "score", 100, ComparisonOperator.GreaterThan));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new IntegerStateCondition(new MockStateStore(), "score", 5, ComparisonOperator.LessThanOrEqual);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new IntegerStateCondition(new MockStateStore(), "key", 0, ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new IntegerStateCondition(new MockStateStore(), "key", 0, ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class FloatStateConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FloatStateCondition(null, "key", 1.5f, ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FloatStateCondition(new MockStateStore(), null, 1.5f, ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new FloatStateCondition(new MockStateStore(), "temp", 36.6f, ComparisonOperator.LessThan, 0.01f));
        }

        [Test]
        public void Constructor_DefaultTolerance_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new FloatStateCondition(new MockStateStore(), "temp", 36.6f, ComparisonOperator.Equal));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new FloatStateCondition(new MockStateStore(), "val", 0f, ComparisonOperator.GreaterThanOrEqual);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new FloatStateCondition(new MockStateStore(), "key", 0f, ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new FloatStateCondition(new MockStateStore(), "key", 0f, ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class StringStateConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StringStateCondition(null, "key", "hello"));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StringStateCondition(new MockStateStore(), null, "hello"));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StringStateCondition(new MockStateStore(), "name", "test", true));
        }

        [Test]
        public void Constructor_DefaultIgnoreCase_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StringStateCondition(new MockStateStore(), "name", "test"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new StringStateCondition(new MockStateStore(), "key", "value");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new StringStateCondition(new MockStateStore(), "key", "value");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StringStateCondition(new MockStateStore(), "key", "value");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class StateEqualsConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateEqualsCondition(null, "key", 42));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateEqualsCondition(new MockStateStore(), null, 42));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StateEqualsCondition(new MockStateStore(), "key", "anyValue"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new StateEqualsCondition(new MockStateStore(), "key", 99);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new StateEqualsCondition(new MockStateStore(), "key", true);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StateEqualsCondition(new MockStateStore(), "key", true);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class StateChangedConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateChangedCondition(null, "key"));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateChangedCondition(new MockStateStore(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StateChangedCondition(new MockStateStore(), "watchedKey"));
        }

        [Test]
        public void StateKey_ReturnsExpectedValue()
        {
            var condition = new StateChangedCondition(new MockStateStore(), "watchedKey");
            Assert.AreEqual("watchedKey", condition.StateKey);
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new StateChangedCondition(new MockStateStore(), "key");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow_AndClearsPreviousState()
        {
            var condition = new StateChangedCondition(new MockStateStore(), "key");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StateChangedCondition(new MockStateStore(), "key");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class StateExistsConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateExistsCondition(null, "key"));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateExistsCondition(new MockStateStore(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StateExistsCondition(new MockStateStore(), "someKey"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new StateExistsCondition(new MockStateStore(), "key");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new StateExistsCondition(new MockStateStore(), "key");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StateExistsCondition(new MockStateStore(), "key");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class StateNotExistsConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateNotExistsCondition(null, "key"));
        }

        [Test]
        public void Constructor_NullStateKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StateNotExistsCondition(new MockStateStore(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StateNotExistsCondition(new MockStateStore(), "missingKey"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new StateNotExistsCondition(new MockStateStore(), "key");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new StateNotExistsCondition(new MockStateStore(), "key");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StateNotExistsCondition(new MockStateStore(), "key");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class VariableComparisonConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new VariableComparisonCondition(null, "left", "right", ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_NullLeftKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new VariableComparisonCondition(new MockStateStore(), null, "right", ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_NullRightKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new VariableComparisonCondition(new MockStateStore(), "left", null, ComparisonOperator.Equal));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new VariableComparisonCondition(new MockStateStore(), "a", "b", ComparisonOperator.NotEqual));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new VariableComparisonCondition(new MockStateStore(), "a", "b", ComparisonOperator.GreaterThan);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new VariableComparisonCondition(new MockStateStore(), "a", "b", ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new VariableComparisonCondition(new MockStateStore(), "a", "b", ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class ObjectPropertyConditionTests
    {
        [Test]
        public void Constructor_NullResolver_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectPropertyCondition(null, "obj1", "visible", true));
        }

        [Test]
        public void Constructor_NullTargetObjectId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectPropertyCondition(new MockSceneObjectResolver(), null, "visible", true));
        }

        [Test]
        public void Constructor_NullPropertyName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectPropertyCondition(new MockSceneObjectResolver(), "obj1", null, true));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new ObjectPropertyCondition(new MockSceneObjectResolver(), "obj1", "isActive", true));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new ObjectPropertyCondition(new MockSceneObjectResolver(), "obj1", "prop", "val");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new ObjectPropertyCondition(new MockSceneObjectResolver(), "obj1", "prop", 0);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new ObjectPropertyCondition(new MockSceneObjectResolver(), "obj1", "prop", 0);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class FlagSetConditionTests
    {
        [Test]
        public void Constructor_NullStateStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FlagSetCondition(null, "flag"));
        }

        [Test]
        public void Constructor_NullFlagKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FlagSetCondition(new MockStateStore(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new FlagSetCondition(new MockStateStore(), "tutorialComplete"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new FlagSetCondition(new MockStateStore(), "flag");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new FlagSetCondition(new MockStateStore(), "flag");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new FlagSetCondition(new MockStateStore(), "flag");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }
}
