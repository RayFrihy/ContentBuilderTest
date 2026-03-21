using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Behaviors
{
    [TestFixture]
    public class StartTimerBehaviorTests
    {
        private MockStateStore _stateStore;

        [SetUp]
        public void SetUp()
        {
            _stateStore = new MockStateStore();
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var behavior = new StartTimerBehavior(_stateStore, "test");

            Assert.IsTrue(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Constructor_CustomParameters_SetsProperties()
        {
            var behavior = new StartTimerBehavior(
                _stateStore, "test", isBlocking: false, stages: ExecutionStages.Deactivation);

            Assert.IsFalse(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ExecuteAsync_SetsTimerStartAndRunningKeys()
        {
            var behavior = new StartTimerBehavior(_stateStore, "countdown");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(_stateStore.HasGlobalState("Timer_countdown_Start"));
            Assert.IsTrue(_stateStore.HasGlobalState("Timer_countdown_Running"));
            Assert.AreEqual(true, _stateStore.GetGlobalState("Timer_countdown_Running"));
        }

        [Test]
        public void ExecuteAsync_NullStateStore_IsNoOp()
        {
            var behavior = new StartTimerBehavior(null, "test");

            // Should not throw
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void UndoAsync_RestoresPreviousState()
        {
            _stateStore.SetGlobalState("Timer_test_Start", 10f);
            _stateStore.SetGlobalState("Timer_test_Running", false);

            var behavior = new StartTimerBehavior(_stateStore, "test");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Running should now be true after execute
            Assert.AreEqual(true, _stateStore.GetGlobalState("Timer_test_Running"));

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Should restore to previous values
            Assert.AreEqual(10f, _stateStore.GetGlobalState("Timer_test_Start"));
            Assert.AreEqual(false, _stateStore.GetGlobalState("Timer_test_Running"));
        }

        [Test]
        public void UndoAsync_NullStateStore_IsNoOp()
        {
            var behavior = new StartTimerBehavior(null, "test");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Should not throw
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void CaptureState_ReturnsTimerKeys()
        {
            var behavior = new StartTimerBehavior(_stateStore, "cap");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            var state = behavior.CaptureState();

            Assert.IsNotNull(state);
            Assert.IsTrue(state.ContainsKey("Timer_cap_Start"));
            Assert.IsTrue(state.ContainsKey("Timer_cap_Running"));
        }

        [Test]
        public void CaptureState_NullStateStore_ReturnsEmptyDictionary()
        {
            var behavior = new StartTimerBehavior(null, "test");

            var state = behavior.CaptureState();

            Assert.IsNotNull(state);
            Assert.AreEqual(0, state.Count);
        }

        [Test]
        public void ImplementsIReversibleBehavior()
        {
            var behavior = new StartTimerBehavior(_stateStore, "test");

            Assert.IsInstanceOf<IReversibleBehavior>(behavior);
        }

        [Test]
        public void ImplementsIStateCaptureBehavior()
        {
            var behavior = new StartTimerBehavior(_stateStore, "test");

            Assert.IsInstanceOf<IStateCaptureBehavior>(behavior);
        }
    }

    [TestFixture]
    public class StopTimerBehaviorTests
    {
        private MockStateStore _stateStore;

        [SetUp]
        public void SetUp()
        {
            _stateStore = new MockStateStore();
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var behavior = new StopTimerBehavior(_stateStore, "test");

            Assert.IsTrue(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Constructor_CustomParameters_SetsProperties()
        {
            var behavior = new StopTimerBehavior(
                _stateStore, "test", isBlocking: false, stages: ExecutionStages.Deactivation);

            Assert.IsFalse(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ExecuteAsync_SetsElapsedAndStopsRunning()
        {
            _stateStore.SetGlobalState("Timer_counter_Start", 100f);
            _stateStore.SetGlobalState("Timer_counter_Running", true);

            var behavior = new StopTimerBehavior(_stateStore, "counter");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(_stateStore.HasGlobalState("Timer_counter_Elapsed"));
            Assert.AreEqual(false, _stateStore.GetGlobalState("Timer_counter_Running"));
        }

        [Test]
        public void ExecuteAsync_NullStateStore_IsNoOp()
        {
            var behavior = new StopTimerBehavior(null, "test");

            // Should not throw
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void UndoAsync_RestoresPreviousState()
        {
            _stateStore.SetGlobalState("Timer_test_Start", 50f);
            _stateStore.SetGlobalState("Timer_test_Running", true);

            var behavior = new StopTimerBehavior(_stateStore, "test");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(false, _stateStore.GetGlobalState("Timer_test_Running"));

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, _stateStore.GetGlobalState("Timer_test_Running"));
        }

        [Test]
        public void UndoAsync_NullStateStore_IsNoOp()
        {
            var behavior = new StopTimerBehavior(null, "test");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Should not throw
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void CaptureState_ReturnsTimerKeys()
        {
            _stateStore.SetGlobalState("Timer_snap_Start", 10f);
            _stateStore.SetGlobalState("Timer_snap_Running", true);

            var behavior = new StopTimerBehavior(_stateStore, "snap");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            var state = behavior.CaptureState();

            Assert.IsNotNull(state);
            Assert.IsTrue(state.ContainsKey("Timer_snap_Elapsed"));
            Assert.IsTrue(state.ContainsKey("Timer_snap_Running"));
        }

        [Test]
        public void CaptureState_NullStateStore_ReturnsEmptyDictionary()
        {
            var behavior = new StopTimerBehavior(null, "test");

            var state = behavior.CaptureState();

            Assert.IsNotNull(state);
            Assert.AreEqual(0, state.Count);
        }

        [Test]
        public void ImplementsIReversibleBehavior()
        {
            var behavior = new StopTimerBehavior(_stateStore, "test");

            Assert.IsInstanceOf<IReversibleBehavior>(behavior);
        }

        [Test]
        public void ImplementsIStateCaptureBehavior()
        {
            var behavior = new StopTimerBehavior(_stateStore, "test");

            Assert.IsInstanceOf<IStateCaptureBehavior>(behavior);
        }
    }

    [TestFixture]
    public class ResetTimerBehaviorTests
    {
        private MockStateStore _stateStore;

        [SetUp]
        public void SetUp()
        {
            _stateStore = new MockStateStore();
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var behavior = new ResetTimerBehavior(_stateStore, "test");

            Assert.IsTrue(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Constructor_CustomParameters_SetsProperties()
        {
            var behavior = new ResetTimerBehavior(
                _stateStore, "test", isBlocking: false, stages: ExecutionStages.Deactivation);

            Assert.IsFalse(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ExecuteAsync_ResetsStartAndRemovesElapsed()
        {
            _stateStore.SetGlobalState("Timer_reset_Start", 10f);
            _stateStore.SetGlobalState("Timer_reset_Elapsed", 5f);

            var behavior = new ResetTimerBehavior(_stateStore, "reset");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Start should be updated to current time
            Assert.IsTrue(_stateStore.HasGlobalState("Timer_reset_Start"));
            // Elapsed should be removed
            Assert.IsFalse(_stateStore.HasGlobalState("Timer_reset_Elapsed"));
        }

        [Test]
        public void ExecuteAsync_NullStateStore_IsNoOp()
        {
            var behavior = new ResetTimerBehavior(null, "test");

            // Should not throw
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void UndoAsync_RestoresPreviousState()
        {
            _stateStore.SetGlobalState("Timer_test_Start", 10f);
            _stateStore.SetGlobalState("Timer_test_Elapsed", 5f);

            var behavior = new ResetTimerBehavior(_stateStore, "test");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Elapsed should be removed after execute
            Assert.IsFalse(_stateStore.HasGlobalState("Timer_test_Elapsed"));

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Should restore previous start and elapsed values
            Assert.AreEqual(10f, _stateStore.GetGlobalState("Timer_test_Start"));
            Assert.AreEqual(5f, _stateStore.GetGlobalState("Timer_test_Elapsed"));
        }

        [Test]
        public void UndoAsync_NullStateStore_IsNoOp()
        {
            var behavior = new ResetTimerBehavior(null, "test");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Should not throw
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void CaptureState_ReturnsTimerKeys()
        {
            _stateStore.SetGlobalState("Timer_cap_Start", 10f);
            _stateStore.SetGlobalState("Timer_cap_Elapsed", 3f);

            var behavior = new ResetTimerBehavior(_stateStore, "cap");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            var state = behavior.CaptureState();

            Assert.IsNotNull(state);
            Assert.IsTrue(state.ContainsKey("Timer_cap_Start"));
        }

        [Test]
        public void CaptureState_NullStateStore_ReturnsEmptyDictionary()
        {
            var behavior = new ResetTimerBehavior(null, "test");

            var state = behavior.CaptureState();

            Assert.IsNotNull(state);
            Assert.AreEqual(0, state.Count);
        }

        [Test]
        public void ImplementsIReversibleBehavior()
        {
            var behavior = new ResetTimerBehavior(_stateStore, "test");

            Assert.IsInstanceOf<IReversibleBehavior>(behavior);
        }

        [Test]
        public void ImplementsIStateCaptureBehavior()
        {
            var behavior = new ResetTimerBehavior(_stateStore, "test");

            Assert.IsInstanceOf<IStateCaptureBehavior>(behavior);
        }
    }
}
