using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Behaviors
{
    [TestFixture]
    public class StateBehaviorTests
    {
        private MockStateStore _stateStore;

        [SetUp]
        public void SetUp()
        {
            _stateStore = new MockStateStore();
        }

        [TearDown]
        public void TearDown()
        {
            _stateStore.Clear();
        }

        // ── SetStateBehavior ──────────────────────────────────────────

        [Test]
        public void SetState_ExecuteAsync_SetsValueInStore()
        {
            var behavior = new SetStateBehavior(_stateStore, "score", 42);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(42, _stateStore.GetGlobalState("score"));
        }

        [Test]
        public void SetState_ExecuteAsync_OverwritesExistingValue()
        {
            _stateStore.SetGlobalState("score", 10);
            var behavior = new SetStateBehavior(_stateStore, "score", 42);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(42, _stateStore.GetGlobalState("score"));
        }

        [Test]
        public void SetState_UndoAsync_RestoresPreviousValue()
        {
            _stateStore.SetGlobalState("score", 10);
            var behavior = new SetStateBehavior(_stateStore, "score", 42);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(10, _stateStore.GetGlobalState("score"));
        }

        [Test]
        public void SetState_UndoAsync_RemovesKeyIfNoPreviousValue()
        {
            var behavior = new SetStateBehavior(_stateStore, "score", 42);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_stateStore.HasGlobalState("score"));
        }

        [Test]
        public void SetState_CaptureState_ReturnsDictionary()
        {
            var behavior = new SetStateBehavior(_stateStore, "score", 42);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("score", captured["Key"]);
            Assert.AreEqual(42, captured["Value"]);
            Assert.AreEqual(false, captured["HadPreviousValue"]);
        }

        [Test]
        public void SetState_IsBlocking_DefaultsToTrue()
        {
            var behavior = new SetStateBehavior(_stateStore, "key", "val");

            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void SetState_Stages_DefaultsToActivation()
        {
            var behavior = new SetStateBehavior(_stateStore, "key", "val");

            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        // ── UpdateStateBehavior ───────────────────────────────────────

        [Test]
        public void UpdateState_ExecuteAsync_UpdatesExistingKey()
        {
            _stateStore.SetGlobalState("name", "old");
            var behavior = new UpdateStateBehavior(_stateStore, "name", "new");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("new", _stateStore.GetGlobalState("name"));
        }

        [Test]
        public void UpdateState_ExecuteAsync_CreatesKeyIfMissing()
        {
            var behavior = new UpdateStateBehavior(_stateStore, "name", "new");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("new", _stateStore.GetGlobalState("name"));
        }

        [Test]
        public void UpdateState_UndoAsync_RestoresPreviousValue()
        {
            _stateStore.SetGlobalState("name", "old");
            var behavior = new UpdateStateBehavior(_stateStore, "name", "new");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("old", _stateStore.GetGlobalState("name"));
        }

        [Test]
        public void UpdateState_UndoAsync_RemovesKeyIfDidNotExist()
        {
            var behavior = new UpdateStateBehavior(_stateStore, "name", "new");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_stateStore.HasGlobalState("name"));
        }

        [Test]
        public void UpdateState_CaptureState_ContainsExpectedKeys()
        {
            _stateStore.SetGlobalState("name", "old");
            var behavior = new UpdateStateBehavior(_stateStore, "name", "new");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("name", captured["Key"]);
            Assert.AreEqual("new", captured["Value"]);
            Assert.AreEqual("old", captured["PreviousValue"]);
            Assert.AreEqual(true, captured["HadPreviousValue"]);
        }

        // ── IncrementStateBehavior ────────────────────────────────────

        [Test]
        public void IncrementState_ExecuteAsync_IncrementsExistingValue()
        {
            _stateStore.SetGlobalState("counter", 5f);
            var behavior = new IncrementStateBehavior(_stateStore, "counter", 3f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(8f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void IncrementState_ExecuteAsync_DefaultsToZeroIfKeyMissing()
        {
            var behavior = new IncrementStateBehavior(_stateStore, "counter", 2f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void IncrementState_ExecuteAsync_DefaultAmountIsOne()
        {
            var behavior = new IncrementStateBehavior(_stateStore, "counter");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void IncrementState_UndoAsync_SubtractsAmount()
        {
            var behavior = new IncrementStateBehavior(_stateStore, "counter", 5f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void IncrementState_CaptureState_ContainsAmountAndPrevious()
        {
            _stateStore.SetGlobalState("counter", 10f);
            var behavior = new IncrementStateBehavior(_stateStore, "counter", 3f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("counter", captured["Key"]);
            Assert.AreEqual(3f, captured["Amount"]);
            Assert.AreEqual(10f, captured["PreviousFloatValue"]);
        }

        // ── DecrementStateBehavior ────────────────────────────────────

        [Test]
        public void DecrementState_ExecuteAsync_DecrementsExistingValue()
        {
            _stateStore.SetGlobalState("counter", 10f);
            var behavior = new DecrementStateBehavior(_stateStore, "counter", 3f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(7f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void DecrementState_ExecuteAsync_DefaultsToZeroIfKeyMissing()
        {
            var behavior = new DecrementStateBehavior(_stateStore, "counter", 2f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(-2f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void DecrementState_ExecuteAsync_DefaultAmountIsOne()
        {
            var behavior = new DecrementStateBehavior(_stateStore, "counter");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(-1f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void DecrementState_UndoAsync_AddsAmountBack()
        {
            _stateStore.SetGlobalState("counter", 10f);
            var behavior = new DecrementStateBehavior(_stateStore, "counter", 4f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(10f, _stateStore.GetGlobalState("counter"));
        }

        [Test]
        public void DecrementState_CaptureState_ContainsAmountAndPrevious()
        {
            var behavior = new DecrementStateBehavior(_stateStore, "counter", 5f);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("counter", captured["Key"]);
            Assert.AreEqual(5f, captured["Amount"]);
            Assert.AreEqual(0f, captured["PreviousFloatValue"]);
        }

        // ── ToggleStateBehavior ───────────────────────────────────────

        [Test]
        public void ToggleState_ExecuteAsync_TogglesFromFalseToTrue()
        {
            _stateStore.SetGlobalState("flag", false);
            var behavior = new ToggleStateBehavior(_stateStore, "flag");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, _stateStore.GetGlobalState("flag"));
        }

        [Test]
        public void ToggleState_ExecuteAsync_TogglesFromTrueToFalse()
        {
            _stateStore.SetGlobalState("flag", true);
            var behavior = new ToggleStateBehavior(_stateStore, "flag");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(false, _stateStore.GetGlobalState("flag"));
        }

        [Test]
        public void ToggleState_ExecuteAsync_DefaultsToFalseIfKeyMissing()
        {
            var behavior = new ToggleStateBehavior(_stateStore, "flag");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, _stateStore.GetGlobalState("flag"));
        }

        [Test]
        public void ToggleState_UndoAsync_RestoresOriginalValue()
        {
            _stateStore.SetGlobalState("flag", true);
            var behavior = new ToggleStateBehavior(_stateStore, "flag");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, _stateStore.GetGlobalState("flag"));
        }

        [Test]
        public void ToggleState_CaptureState_ContainsOriginalBoolValue()
        {
            _stateStore.SetGlobalState("flag", true);
            var behavior = new ToggleStateBehavior(_stateStore, "flag");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("flag", captured["Key"]);
            Assert.AreEqual(true, captured["OriginalBoolValue"]);
        }

        // ── ClearStateBehavior ────────────────────────────────────────

        [Test]
        public void ClearState_ExecuteAsync_NullKey_ClearsAllState()
        {
            _stateStore.SetGlobalState("a", 1);
            _stateStore.SetGlobalState("b", 2);
            var behavior = new ClearStateBehavior(_stateStore);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_stateStore.HasGlobalState("a"));
            Assert.IsFalse(_stateStore.HasGlobalState("b"));
        }

        [Test]
        public void ClearState_ExecuteAsync_WithKey_RemovesSingleKey()
        {
            _stateStore.SetGlobalState("a", 1);
            _stateStore.SetGlobalState("b", 2);
            var behavior = new ClearStateBehavior(_stateStore, "a");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_stateStore.HasGlobalState("a"));
            Assert.IsTrue(_stateStore.HasGlobalState("b"));
        }

        [Test]
        public void ClearState_UndoAsync_NullKey_RestoresAllState()
        {
            _stateStore.SetGlobalState("a", 1);
            _stateStore.SetGlobalState("b", 2);
            var behavior = new ClearStateBehavior(_stateStore);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _stateStore.GetGlobalState("a"));
            Assert.AreEqual(2, _stateStore.GetGlobalState("b"));
        }

        [Test]
        public void ClearState_UndoAsync_WithKey_RestoresSingleKey()
        {
            _stateStore.SetGlobalState("a", 1);
            var behavior = new ClearStateBehavior(_stateStore, "a");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _stateStore.GetGlobalState("a"));
        }

        [Test]
        public void ClearState_CaptureState_NullKey_ContainsSnapshot()
        {
            _stateStore.SetGlobalState("a", 1);
            var behavior = new ClearStateBehavior(_stateStore);

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.IsNull(captured["Key"]);
            Assert.IsNotNull(captured["StateSnapshot"]);
        }

        [Test]
        public void ClearState_CaptureState_WithKey_ContainsPreviousValue()
        {
            _stateStore.SetGlobalState("a", 1);
            var behavior = new ClearStateBehavior(_stateStore, "a");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("a", captured["Key"]);
            Assert.AreEqual(1, captured["PreviousValue"]);
            Assert.AreEqual(true, captured["HadPreviousValue"]);
        }

        // ── CopyStateBehavior ─────────────────────────────────────────

        [Test]
        public void CopyState_ExecuteAsync_CopiesSourceToDestination()
        {
            _stateStore.SetGlobalState("src", "hello");
            var behavior = new CopyStateBehavior(_stateStore, "src", "dst");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("hello", _stateStore.GetGlobalState("dst"));
        }

        [Test]
        public void CopyState_ExecuteAsync_OverwritesExistingDestination()
        {
            _stateStore.SetGlobalState("src", "hello");
            _stateStore.SetGlobalState("dst", "old");
            var behavior = new CopyStateBehavior(_stateStore, "src", "dst");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("hello", _stateStore.GetGlobalState("dst"));
        }

        [Test]
        public void CopyState_UndoAsync_RestoresPreviousDestValue()
        {
            _stateStore.SetGlobalState("src", "hello");
            _stateStore.SetGlobalState("dst", "old");
            var behavior = new CopyStateBehavior(_stateStore, "src", "dst");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("old", _stateStore.GetGlobalState("dst"));
        }

        [Test]
        public void CopyState_UndoAsync_RemovesDestIfNoPreviousValue()
        {
            _stateStore.SetGlobalState("src", "hello");
            var behavior = new CopyStateBehavior(_stateStore, "src", "dst");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_stateStore.HasGlobalState("dst"));
        }

        [Test]
        public void CopyState_CaptureState_ContainsSourceAndDestKeys()
        {
            _stateStore.SetGlobalState("src", "hello");
            var behavior = new CopyStateBehavior(_stateStore, "src", "dst");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("src", captured["SourceKey"]);
            Assert.AreEqual("dst", captured["DestinationKey"]);
            Assert.AreEqual(false, captured["HadPreviousDestValue"]);
        }

        // ── SaveStateBehavior ─────────────────────────────────────────

        [Test]
        public void SaveState_ExecuteAsync_SavesSnapshotUnderKey()
        {
            _stateStore.SetGlobalState("a", 1);
            _stateStore.SetGlobalState("b", 2);
            var behavior = new SaveStateBehavior(_stateStore, "mySnapshot");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            var snapshot = _stateStore.GetGlobalState("mySnapshot") as Dictionary<string, object>;
            Assert.IsNotNull(snapshot);
            Assert.AreEqual(1, snapshot["a"]);
            Assert.AreEqual(2, snapshot["b"]);
        }

        [Test]
        public void SaveState_UndoAsync_RemovesSnapshotKey()
        {
            _stateStore.SetGlobalState("a", 1);
            var behavior = new SaveStateBehavior(_stateStore, "mySnapshot");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(_stateStore.HasGlobalState("mySnapshot"));
        }

        [Test]
        public void SaveState_CaptureState_ContainsSnapshotKey()
        {
            var behavior = new SaveStateBehavior(_stateStore, "mySnapshot");

            var captured = behavior.CaptureState();

            Assert.AreEqual("mySnapshot", captured["SnapshotKey"]);
        }

        [Test]
        public void SaveState_IsBlocking_DefaultsToTrue()
        {
            var behavior = new SaveStateBehavior(_stateStore, "snap");

            Assert.IsTrue(behavior.IsBlocking);
        }

        // ── LoadStateBehavior ─────────────────────────────────────────

        [Test]
        public void LoadState_ExecuteAsync_LoadsSnapshotIntoState()
        {
            var snapshot = new Dictionary<string, object> { { "x", 10 }, { "y", 20 } };
            _stateStore.SetGlobalState("snap", snapshot);
            var behavior = new LoadStateBehavior(_stateStore, "snap");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(10, _stateStore.GetGlobalState("x"));
            Assert.AreEqual(20, _stateStore.GetGlobalState("y"));
        }

        [Test]
        public void LoadState_ExecuteAsync_SnapshotNotFound_DoesNotThrow()
        {
            var behavior = new LoadStateBehavior(_stateStore, "missing");

            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void LoadState_UndoAsync_RestoresPreLoadState()
        {
            _stateStore.SetGlobalState("a", 1);
            var snapshot = new Dictionary<string, object> { { "x", 10 } };
            _stateStore.SetGlobalState("snap", snapshot);
            var behavior = new LoadStateBehavior(_stateStore, "snap");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _stateStore.GetGlobalState("a"));
            Assert.IsTrue(_stateStore.HasGlobalState("snap"));
        }

        [Test]
        public void LoadState_CaptureState_ContainsSnapshotKeyAndPreLoad()
        {
            _stateStore.SetGlobalState("a", 1);
            var snapshot = new Dictionary<string, object> { { "x", 10 } };
            _stateStore.SetGlobalState("snap", snapshot);
            var behavior = new LoadStateBehavior(_stateStore, "snap");

            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            var captured = behavior.CaptureState();

            Assert.AreEqual("snap", captured["SnapshotKey"]);
            Assert.IsNotNull(captured["PreLoadState"]);
        }

        [Test]
        public void LoadState_Stages_CustomStage()
        {
            var behavior = new LoadStateBehavior(_stateStore, "snap",
                stages: ExecutionStages.Deactivation);

            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void LoadState_IsBlocking_CanBeSetToFalse()
        {
            var behavior = new LoadStateBehavior(_stateStore, "snap", isBlocking: false);

            Assert.IsFalse(behavior.IsBlocking);
        }
    }
}
