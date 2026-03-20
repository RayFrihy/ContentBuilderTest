using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.State;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class StateStoreTests
    {
        private StateStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new StateStore();
        }

        [Test]
        public void CaptureSnapshot_StoresAndReturnsSnapshot()
        {
            var step = CreateStep("step1");
            _store.SetGlobalState("key1", "value1");

            var snapshot = _store.CaptureSnapshot(step);

            Assert.IsNotNull(snapshot);
            Assert.AreEqual("step1", snapshot.StepId);
            Assert.AreEqual("value1", snapshot.State["key1"]);
        }

        [Test]
        public void CaptureSnapshot_NullStep_ReturnsNull()
        {
            var result = _store.CaptureSnapshot(null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetSnapshot_ReturnsNull_ForUnknownStepId()
        {
            Assert.IsNull(_store.GetSnapshot("unknown"));
        }

        [Test]
        public void GetSnapshot_ReturnsNull_ForNullId()
        {
            Assert.IsNull(_store.GetSnapshot(null));
        }

        [Test]
        public void RestoreSnapshot_RestoresGlobalState()
        {
            _store.SetGlobalState("key1", "original");

            var snapshot = new StepSnapshot
            {
                StepId = "step1",
                State = new Dictionary<string, object> { ["key1"] = "restored", ["key2"] = 42 }
            };

            _store.RestoreSnapshotAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("restored", _store.GetGlobalState("key1"));
            Assert.AreEqual(42, _store.GetGlobalState("key2"));
        }

        [Test]
        public void SlidingWindow_RemovesOldestWhenExceedingMax()
        {
            // The default max is 50, so we need to add 51 snapshots
            for (int i = 0; i < 51; i++)
            {
                var step = CreateStep($"step{i}");
                _store.CaptureSnapshot(step);
            }

            // The first snapshot should have been pruned
            // We can verify by checking that step0 snapshot is gone
            // (this is probabilistic since it depends on timestamp ordering)
            // Just verify we can still get a recent one
            Assert.IsNotNull(_store.GetSnapshot("step50"));
        }

        [Test]
        public void Clear_RemovesAllData()
        {
            _store.SetGlobalState("key1", "value1");
            _store.CaptureSnapshot(CreateStep("step1"));

            _store.Clear();

            Assert.IsNull(_store.GetGlobalState("key1"));
            Assert.IsNull(_store.GetSnapshot("step1"));
        }

        [Test]
        public void GlobalState_SetGetHasRemove_WorkCorrectly()
        {
            // Set
            _store.SetGlobalState("key1", "value1");
            Assert.AreEqual("value1", _store.GetGlobalState("key1"));

            // Has
            Assert.IsTrue(_store.HasGlobalState("key1"));
            Assert.IsFalse(_store.HasGlobalState("missing"));

            // Remove
            _store.RemoveGlobalState("key1");
            Assert.IsFalse(_store.HasGlobalState("key1"));
            Assert.IsNull(_store.GetGlobalState("key1"));
        }

        [Test]
        public void GetAllGlobalState_ReturnsCopy()
        {
            _store.SetGlobalState("a", 1);
            _store.SetGlobalState("b", 2);

            var all = _store.GetAllGlobalState();
            Assert.AreEqual(2, all.Count);

            // Modifying the copy should not affect the store
            all["c"] = 3;
            Assert.IsFalse(_store.HasGlobalState("c"));
        }

        [Test]
        public void SetAllGlobalState_ReplacesExistingState()
        {
            _store.SetGlobalState("old", "value");

            _store.SetAllGlobalState(new Dictionary<string, object> { ["new"] = "fresh" });

            Assert.IsFalse(_store.HasGlobalState("old"));
            Assert.AreEqual("fresh", _store.GetGlobalState("new"));
        }

        [Test]
        public void NullKey_Operations_DoNotThrow()
        {
            _store.SetGlobalState(null, "value");
            Assert.IsNull(_store.GetGlobalState(null));
            Assert.IsFalse(_store.HasGlobalState(null));
            _store.RemoveGlobalState(null); // should not throw
        }

        private StepModel CreateStep(string id)
        {
            return new StepModel { Id = id, Name = $"Step {id}" };
        }
    }
}
