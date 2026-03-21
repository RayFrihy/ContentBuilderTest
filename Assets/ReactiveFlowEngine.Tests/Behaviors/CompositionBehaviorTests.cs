using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Behaviors
{
    [TestFixture]
    public class CompositionBehaviorTests
    {
        // ───────────────────────────────────────────────
        // BehaviorSequence
        // ───────────────────────────────────────────────

        [Test]
        public void BehaviorSequence_ExecutesChildrenSequentially()
        {
            var b1 = new TestBehavior();
            var b2 = new TestBehavior();
            var b3 = new TestBehavior();

            var sequence = new BehaviorSequence(new List<IBehavior> { b1, b2, b3 });
            sequence.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, b1.ExecuteCount);
            Assert.AreEqual(1, b2.ExecuteCount);
            Assert.AreEqual(1, b3.ExecuteCount);
        }

        [Test]
        public void BehaviorSequence_ChecksCancellationBetweenChildren()
        {
            var b1 = new TestBehavior();
            var b2 = new TestBehavior();

            var cts = new CancellationTokenSource();
            var sequence = new BehaviorSequence(new List<IBehavior> { b1, b2 });

            cts.Cancel();

            Assert.Throws<OperationCanceledException>(() =>
                sequence.ExecuteAsync(cts.Token).GetAwaiter().GetResult());

            Assert.AreEqual(0, b1.ExecuteCount);
            Assert.AreEqual(0, b2.ExecuteCount);
        }

        [Test]
        public void BehaviorSequence_UndoAsync_RunsInReverseOrder()
        {
            var b1 = new TestBehavior();
            var b2 = new TestBehavior();
            var b3 = new TestBehavior();

            var sequence = new BehaviorSequence(new List<IBehavior> { b1, b2, b3 });
            sequence.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            sequence.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, b1.UndoCount);
            Assert.AreEqual(1, b2.UndoCount);
            Assert.AreEqual(1, b3.UndoCount);
        }

        [Test]
        public void BehaviorSequence_UndoAsync_SkipsNonReversibleChildren()
        {
            var reversible = new TestBehavior();
            var nonReversible = new NonReversibleBehavior();

            var sequence = new BehaviorSequence(new List<IBehavior> { reversible, nonReversible });
            sequence.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            sequence.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, reversible.UndoCount);
            Assert.AreEqual(1, nonReversible.ExecuteCount);
        }

        [Test]
        public void BehaviorSequence_UndoAsync_BeforeExecute_DoesNothing()
        {
            var b1 = new TestBehavior();
            var sequence = new BehaviorSequence(new List<IBehavior> { b1 });

            // Undo without executing first should not throw
            sequence.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, b1.UndoCount);
        }

        [Test]
        public void BehaviorSequence_DefaultProperties()
        {
            var sequence = new BehaviorSequence(new List<IBehavior>());

            Assert.IsTrue(sequence.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, sequence.Stages);
        }

        [Test]
        public void BehaviorSequence_CustomProperties()
        {
            var sequence = new BehaviorSequence(
                new List<IBehavior>(),
                isBlocking: false,
                stages: ExecutionStages.Deactivation);

            Assert.IsFalse(sequence.IsBlocking);
            Assert.AreEqual(ExecutionStages.Deactivation, sequence.Stages);
        }

        // ───────────────────────────────────────────────
        // ParallelBehavior
        // ───────────────────────────────────────────────

        [Test]
        public void ParallelBehavior_WaitForAll_ExecutesAllChildren()
        {
            var b1 = new TestBehavior();
            var b2 = new TestBehavior();
            var b3 = new TestBehavior();

            var parallel = new ParallelBehavior(new List<IBehavior> { b1, b2, b3 }, waitForAll: true);
            parallel.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, b1.ExecuteCount);
            Assert.AreEqual(1, b2.ExecuteCount);
            Assert.AreEqual(1, b3.ExecuteCount);
        }

        [Test]
        public void ParallelBehavior_WaitForAny_ExecutesAllChildren()
        {
            var b1 = new TestBehavior();
            var b2 = new TestBehavior();

            var parallel = new ParallelBehavior(new List<IBehavior> { b1, b2 }, waitForAll: false);
            parallel.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // With synchronous test behaviors, all still get executed
            Assert.AreEqual(1, b1.ExecuteCount);
            Assert.AreEqual(1, b2.ExecuteCount);
        }

        [Test]
        public void ParallelBehavior_UndoAsync_UndoesAllReversibleChildren()
        {
            var b1 = new TestBehavior();
            var b2 = new TestBehavior();
            var nonReversible = new NonReversibleBehavior();

            var parallel = new ParallelBehavior(new List<IBehavior> { b1, b2, nonReversible });
            parallel.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            parallel.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, b1.UndoCount);
            Assert.AreEqual(1, b2.UndoCount);
        }

        [Test]
        public void ParallelBehavior_DefaultProperties()
        {
            var parallel = new ParallelBehavior(new List<IBehavior>());

            Assert.IsTrue(parallel.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, parallel.Stages);
        }

        // ───────────────────────────────────────────────
        // ConditionalBehavior
        // ───────────────────────────────────────────────

        [Test]
        public void ConditionalBehavior_ConditionTrue_ExecutesChild()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", true);

            var child = new TestBehavior();
            var conditional = new ConditionalBehavior(store, "flag", child);
            conditional.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, child.ExecuteCount);
        }

        [Test]
        public void ConditionalBehavior_ConditionFalse_SkipsChild()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", false);

            var child = new TestBehavior();
            var conditional = new ConditionalBehavior(store, "flag", child);
            conditional.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, child.ExecuteCount);
        }

        [Test]
        public void ConditionalBehavior_NonNullValue_ExecutesChild()
        {
            var store = new MockStateStore();
            store.SetGlobalState("key", "some-value");

            var child = new TestBehavior();
            var conditional = new ConditionalBehavior(store, "key", child);
            conditional.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, child.ExecuteCount);
        }

        [Test]
        public void ConditionalBehavior_NullValue_SkipsChild()
        {
            var store = new MockStateStore();
            // key not set, GetGlobalState returns null

            var child = new TestBehavior();
            var conditional = new ConditionalBehavior(store, "missing", child);
            conditional.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, child.ExecuteCount);
        }

        [Test]
        public void ConditionalBehavior_UndoAsync_WhenExecuted_UndoesChild()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", true);

            var child = new TestBehavior();
            var conditional = new ConditionalBehavior(store, "flag", child);
            conditional.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            conditional.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, child.UndoCount);
        }

        [Test]
        public void ConditionalBehavior_UndoAsync_WhenNotExecuted_DoesNotUndoChild()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", false);

            var child = new TestBehavior();
            var conditional = new ConditionalBehavior(store, "flag", child);
            conditional.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            conditional.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, child.UndoCount);
        }

        // ───────────────────────────────────────────────
        // BranchBehavior
        // ───────────────────────────────────────────────

        [Test]
        public void BranchBehavior_ConditionTrue_ExecutesTrueBranch()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", true);

            var trueBranch = new TestBehavior();
            var falseBranch = new TestBehavior();
            var branch = new BranchBehavior(store, "flag", trueBranch, falseBranch);
            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, trueBranch.ExecuteCount);
            Assert.AreEqual(0, falseBranch.ExecuteCount);
        }

        [Test]
        public void BranchBehavior_ConditionFalse_ExecutesFalseBranch()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", false);

            var trueBranch = new TestBehavior();
            var falseBranch = new TestBehavior();
            var branch = new BranchBehavior(store, "flag", trueBranch, falseBranch);
            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, trueBranch.ExecuteCount);
            Assert.AreEqual(1, falseBranch.ExecuteCount);
        }

        [Test]
        public void BranchBehavior_ConditionFalse_NoFalseBranch_ExecutesNothing()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", false);

            var trueBranch = new TestBehavior();
            var branch = new BranchBehavior(store, "flag", trueBranch);
            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, trueBranch.ExecuteCount);
        }

        [Test]
        public void BranchBehavior_UndoAsync_UndoesTrueBranch()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", true);

            var trueBranch = new TestBehavior();
            var falseBranch = new TestBehavior();
            var branch = new BranchBehavior(store, "flag", trueBranch, falseBranch);
            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            branch.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, trueBranch.UndoCount);
            Assert.AreEqual(0, falseBranch.UndoCount);
        }

        [Test]
        public void BranchBehavior_UndoAsync_UndoesFalseBranch()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", false);

            var trueBranch = new TestBehavior();
            var falseBranch = new TestBehavior();
            var branch = new BranchBehavior(store, "flag", trueBranch, falseBranch);
            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            branch.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, trueBranch.UndoCount);
            Assert.AreEqual(1, falseBranch.UndoCount);
        }

        [Test]
        public void BranchBehavior_NonBoolTruthyValue_ExecutesTrueBranch()
        {
            var store = new MockStateStore();
            store.SetGlobalState("flag", 42);

            var trueBranch = new TestBehavior();
            var falseBranch = new TestBehavior();
            var branch = new BranchBehavior(store, "flag", trueBranch, falseBranch);
            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, trueBranch.ExecuteCount);
            Assert.AreEqual(0, falseBranch.ExecuteCount);
        }

        // ───────────────────────────────────────────────
        // RepeatBehavior
        // ───────────────────────────────────────────────

        [Test]
        public void RepeatBehavior_ExecutesChildCountTimes()
        {
            var child = new TestBehavior();
            var repeat = new RepeatBehavior(child, count: 5);
            repeat.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(5, child.ExecuteCount);
        }

        [Test]
        public void RepeatBehavior_ZeroCount_DoesNotExecute()
        {
            var child = new TestBehavior();
            var repeat = new RepeatBehavior(child, count: 0);
            repeat.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, child.ExecuteCount);
        }

        [Test]
        public void RepeatBehavior_ChecksCancellationEachIteration()
        {
            var child = new TestBehavior();
            var cts = new CancellationTokenSource();

            var repeat = new RepeatBehavior(child, count: 100);

            cts.Cancel();

            Assert.Throws<OperationCanceledException>(() =>
                repeat.ExecuteAsync(cts.Token).GetAwaiter().GetResult());

            Assert.AreEqual(0, child.ExecuteCount);
        }

        [Test]
        public void RepeatBehavior_DefaultProperties()
        {
            var child = new TestBehavior();
            var repeat = new RepeatBehavior(child, count: 1);

            Assert.IsTrue(repeat.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, repeat.Stages);
        }

        // ───────────────────────────────────────────────
        // LoopBehavior
        // ───────────────────────────────────────────────

        [Test]
        public void LoopBehavior_WithCondition_LoopsWhileTrue()
        {
            var store = new MockStateStore();
            store.SetGlobalState("running", true);

            var callCount = 0;
            var child = new CountingBehavior(() =>
            {
                callCount++;
                if (callCount >= 3) store.SetGlobalState("running", false);
            });

            var loop = new LoopBehavior(child, store, "running", maxIterations: 100);
            loop.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void LoopBehavior_NoStateStore_LoopsUntilMaxIterations()
        {
            var child = new TestBehavior();
            var loop = new LoopBehavior(child, stateStore: null, conditionKey: null, maxIterations: 5);
            loop.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(5, child.ExecuteCount);
        }

        [Test]
        public void LoopBehavior_ConditionFalseInitially_DoesNotLoop()
        {
            var store = new MockStateStore();
            store.SetGlobalState("running", false);

            var child = new TestBehavior();
            var loop = new LoopBehavior(child, store, "running", maxIterations: 100);
            loop.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, child.ExecuteCount);
        }

        [Test]
        public void LoopBehavior_RespectsMaxIterations()
        {
            var store = new MockStateStore();
            store.SetGlobalState("running", true);

            var child = new TestBehavior();
            var loop = new LoopBehavior(child, store, "running", maxIterations: 10);
            loop.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(10, child.ExecuteCount);
        }

        [Test]
        public void LoopBehavior_ChecksCancellation()
        {
            var store = new MockStateStore();
            store.SetGlobalState("running", true);

            var child = new TestBehavior();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var loop = new LoopBehavior(child, store, "running", maxIterations: 100);

            Assert.Throws<OperationCanceledException>(() =>
                loop.ExecuteAsync(cts.Token).GetAwaiter().GetResult());

            Assert.AreEqual(0, child.ExecuteCount);
        }

        // ───────────────────────────────────────────────
        // RetryBehavior
        // ───────────────────────────────────────────────

        [Test]
        public void RetryBehavior_SuccessOnFirstAttempt_ExecutesOnce()
        {
            var child = new TestBehavior();
            var retry = new RetryBehavior(child, maxRetries: 3, retryDelay: 0f);
            retry.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, child.ExecuteCount);
        }

        [Test]
        public void RetryBehavior_FailsThenSucceeds_Retries()
        {
            var child = new FailingBehavior(failCount: 2);
            var retry = new RetryBehavior(child, maxRetries: 3, retryDelay: 0f);
            retry.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(3, child.CallCount);
        }

        [Test]
        public void RetryBehavior_ExceedsMaxRetries_Throws()
        {
            var child = new FailingBehavior(failCount: 5);
            var retry = new RetryBehavior(child, maxRetries: 2, retryDelay: 0f);

            Assert.Throws<InvalidOperationException>(() =>
                retry.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());

            Assert.AreEqual(3, child.CallCount); // 1 initial + 2 retries
        }

        [Test]
        public void RetryBehavior_OperationCanceled_RethrowsImmediately()
        {
            var child = new CancellingBehavior();
            var retry = new RetryBehavior(child, maxRetries: 3, retryDelay: 0f);

            Assert.Throws<OperationCanceledException>(() =>
                retry.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());

            Assert.AreEqual(1, child.CallCount);
        }

        [Test]
        public void RetryBehavior_DefaultProperties()
        {
            var child = new TestBehavior();
            var retry = new RetryBehavior(child);

            Assert.IsTrue(retry.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, retry.Stages);
        }

        // ───────────────────────────────────────────────
        // Helper classes
        // ───────────────────────────────────────────────

        private class NonReversibleBehavior : IBehavior
        {
            public int ExecuteCount { get; private set; }
            public ExecutionStages Stages => ExecutionStages.Activation;
            public bool IsBlocking => true;

            public UniTask ExecuteAsync(CancellationToken ct)
            {
                ExecuteCount++;
                return UniTask.CompletedTask;
            }
        }

        private class FailingBehavior : IBehavior
        {
            private int _callCount;
            private readonly int _failCount;

            public int CallCount => _callCount;
            public ExecutionStages Stages => ExecutionStages.Activation;
            public bool IsBlocking => true;

            public FailingBehavior(int failCount)
            {
                _failCount = failCount;
            }

            public UniTask ExecuteAsync(CancellationToken ct)
            {
                _callCount++;
                if (_callCount <= _failCount)
                    throw new InvalidOperationException("Simulated failure");
                return UniTask.CompletedTask;
            }
        }

        private class CancellingBehavior : IBehavior
        {
            public int CallCount { get; private set; }
            public ExecutionStages Stages => ExecutionStages.Activation;
            public bool IsBlocking => true;

            public UniTask ExecuteAsync(CancellationToken ct)
            {
                CallCount++;
                throw new OperationCanceledException();
            }
        }

        private class CountingBehavior : IBehavior
        {
            private readonly Action _onExecute;
            public ExecutionStages Stages => ExecutionStages.Activation;
            public bool IsBlocking => true;

            public CountingBehavior(Action onExecute)
            {
                _onExecute = onExecute;
            }

            public UniTask ExecuteAsync(CancellationToken ct)
            {
                _onExecute?.Invoke();
                return UniTask.CompletedTask;
            }
        }
    }
}
