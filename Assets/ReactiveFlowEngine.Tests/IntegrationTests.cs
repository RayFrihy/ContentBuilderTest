using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Conditions.Interaction;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Navigation;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private MockStepRunner _stepRunner;
        private MockStateStore _stateStore;
        private MockEventBus _eventBus;
        private HistoryStack _historyService;
        private ChapterRunner _chapterRunner;
        private FlowEngine _engine;
        private NavigationService _navigationService;

        [SetUp]
        public void SetUp()
        {
            _stepRunner = new MockStepRunner();
            _stateStore = new MockStateStore();
            _eventBus = new MockEventBus();
            _historyService = new HistoryStack();
            _chapterRunner = new ChapterRunner(_stepRunner, _stateStore, _historyService);
            _engine = new FlowEngine(_stepRunner, _stateStore, _chapterRunner, _historyService);
            _navigationService = new NavigationService(
                _engine, _engine, _stepRunner, _stateStore, _historyService);
        }

        [TearDown]
        public void TearDown()
        {
            _navigationService.Dispose();
            _engine.Dispose();
            _eventBus.Dispose();
        }

        // -------------------------------------------------------------------
        // 1. Full process flow: verify step execution order across chapters
        // -------------------------------------------------------------------

        [Test]
        public void FullProcessFlow_ExecutesStepsInOrder()
        {
            var builder = new TestProcessBuilder("FullFlow");
            builder
                .AddChapter("Ch1")
                .AddStep("Step1", "s1")
                .AddStep("Step2", "s2")
                .AddStep("Step3", "s3");

            var process = builder.Build();

            var s1 = process.ChapterModels[0].StepModels[0];
            var s2 = process.ChapterModels[0].StepModels[1];
            var s3 = process.ChapterModels[0].StepModels[2];

            var t1 = new TransitionModel { TargetStepModel = s2 };
            s1.TransitionModels.Add(t1);

            var t2 = new TransitionModel { TargetStepModel = s3 };
            s2.TransitionModels.Add(t2);

            var t3 = new TransitionModel { TargetStepModel = null };
            s3.TransitionModels.Add(t3);

            _stepRunner.EnqueueTransition(t1);
            _stepRunner.EnqueueTransition(t2);
            _stepRunner.EnqueueTransition(t3);

            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(3, _stepRunner.RunSteps.Count);
            Assert.AreEqual("s1", _stepRunner.RunSteps[0].Id);
            Assert.AreEqual("s2", _stepRunner.RunSteps[1].Id);
            Assert.AreEqual("s3", _stepRunner.RunSteps[2].Id);
            Assert.AreEqual(EngineState.Completed, _engine.State.CurrentValue);
        }

        [Test]
        public void FullProcessFlow_MultipleChapters_RunsSequentially()
        {
            var builder = new TestProcessBuilder("MultiChapter");
            builder
                .AddChapter("Ch1")
                .AddStep("Ch1Step1", "c1s1")
                .AddChapter("Ch2")
                .AddStep("Ch2Step1", "c2s1");

            var process = builder.Build();

            var c1s1 = process.ChapterModels[0].StepModels[0];
            var c2s1 = process.ChapterModels[1].StepModels[0];

            var t1 = new TransitionModel { TargetStepModel = null }; // end ch1
            c1s1.TransitionModels.Add(t1);

            var t2 = new TransitionModel { TargetStepModel = null }; // end ch2
            c2s1.TransitionModels.Add(t2);

            _stepRunner.EnqueueTransition(t1);
            _stepRunner.EnqueueTransition(t2);

            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _stepRunner.RunSteps.Count);
            Assert.AreEqual("c1s1", _stepRunner.RunSteps[0].Id);
            Assert.AreEqual("c2s1", _stepRunner.RunSteps[1].Id);
            Assert.AreEqual(EngineState.Completed, _engine.State.CurrentValue);
        }

        // -------------------------------------------------------------------
        // 2. Navigation with history
        // -------------------------------------------------------------------

        [Test]
        public void NavigationWithHistory_PushesEntriesAndCanGoBack()
        {
            var builder = new TestProcessBuilder("NavHistory");
            builder
                .AddChapter("Ch1", "ch1")
                .AddStep("Step1", "s1")
                .AddStep("Step2", "s2")
                .AddStep("Step3", "s3");

            var process = builder.Build();
            var chapter = process.ChapterModels[0];

            var s1 = chapter.StepModels[0];
            var s2 = chapter.StepModels[1];
            var s3 = chapter.StepModels[2];

            var t1 = new TransitionModel { TargetStepModel = s2 };
            s1.TransitionModels.Add(t1);

            var t2 = new TransitionModel { TargetStepModel = s3 };
            s2.TransitionModels.Add(t2);

            var t3 = new TransitionModel { TargetStepModel = null };
            s3.TransitionModels.Add(t3);

            _stepRunner.EnqueueTransition(t1);
            _stepRunner.EnqueueTransition(t2);
            _stepRunner.EnqueueTransition(t3);

            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();

            // Engine pushes history for each step that transitions
            var entries = _historyService.GetAll();
            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual("s1", entries[0].StepId);
            Assert.AreEqual("s2", entries[1].StepId);
            Assert.AreEqual("s3", entries[2].StepId);

            // Verify Pop returns most recent
            var popped = _historyService.Pop();
            Assert.AreEqual("s3", popped.StepId);
            Assert.IsTrue(_historyService.CanGoBack);
        }

        [Test]
        public void NavigationService_PreviousStep_RestoresSnapshot()
        {
            var builder = new TestProcessBuilder("NavRestore");
            builder
                .AddChapter("Ch1", "ch1")
                .AddStep("Step1", "s1")
                .AddStep("Step2", "s2");

            var process = builder.Build();
            var chapter = process.ChapterModels[0];
            var s1 = chapter.StepModels[0];
            var s2 = chapter.StepModels[1];

            // Set up NavigationService with the process so FindStep works
            _navigationService.SetCurrentProcess(process);

            // Manually simulate engine having run through s1 and being on s2
            ((IEngineController)_engine).SetCurrentStep(s2);
            ((IEngineController)_engine).SetCurrentChapter(chapter);

            // Simulate: set some state before s1 snapshot
            _stateStore.SetGlobalState("color", "red");
            _stateStore.CaptureSnapshot(s1);

            // Push history entry for s1
            _historyService.Push(new HistoryEntry("ch1", "s1"));

            // Change state (simulating s2 execution)
            _stateStore.SetGlobalState("color", "blue");

            // Navigate back
            _navigationService.PreviousStepAsync(CancellationToken.None).GetAwaiter().GetResult();

            // After going back, the snapshot for s1 should be restored
            Assert.AreEqual("red", _stateStore.GetGlobalState("color"));
            Assert.AreEqual("s1", _engine.CurrentStep.CurrentValue.Id);
        }

        // -------------------------------------------------------------------
        // 3. State persistence across steps
        // -------------------------------------------------------------------

        [Test]
        public void StatePersistence_SetStateInOneStep_PersistsToNext()
        {
            // SetStateBehavior sets state in the store;
            // we verify it persists when the next step runs.
            var setStateBehavior = new SetStateBehavior(_stateStore, "score", 42);

            // Execute the behavior directly
            setStateBehavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(_stateStore.HasGlobalState("score"));
            Assert.AreEqual(42, _stateStore.GetGlobalState("score"));

            // Capture snapshot after step1
            var step1 = new StepModel { Id = "s1", Name = "Step1" };
            step1.BehaviorList.Add(setStateBehavior);
            _stateStore.CaptureSnapshot(step1);

            // Next step can read the state
            Assert.AreEqual(42, _stateStore.GetGlobalState("score"));

            // Even after snapshot capture, global state remains
            var snapshot = _stateStore.GetSnapshot("s1");
            Assert.IsNotNull(snapshot);
            Assert.AreEqual(42, snapshot.State["score"]);
        }

        [Test]
        public void StatePersistence_UndoSetState_RestoresPreviousValue()
        {
            _stateStore.SetGlobalState("count", 10);

            var setStateBehavior = new SetStateBehavior(_stateStore, "count", 99);
            setStateBehavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(99, _stateStore.GetGlobalState("count"));

            // Undo reverts to old value
            setStateBehavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(10, _stateStore.GetGlobalState("count"));
        }

        // -------------------------------------------------------------------
        // 4. Conditional branching
        // -------------------------------------------------------------------

        [Test]
        public void BranchBehavior_TrueBranch_WhenStateIsTrue()
        {
            var trueBehavior = new TestBehavior();
            var falseBehavior = new TestBehavior();

            _stateStore.SetGlobalState("isReady", true);

            var branch = new BranchBehavior(
                _stateStore, "isReady", trueBehavior, falseBehavior);

            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, trueBehavior.ExecuteCount);
            Assert.AreEqual(0, falseBehavior.ExecuteCount);
        }

        [Test]
        public void BranchBehavior_FalseBranch_WhenStateIsFalse()
        {
            var trueBehavior = new TestBehavior();
            var falseBehavior = new TestBehavior();

            _stateStore.SetGlobalState("isReady", false);

            var branch = new BranchBehavior(
                _stateStore, "isReady", trueBehavior, falseBehavior);

            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, trueBehavior.ExecuteCount);
            Assert.AreEqual(1, falseBehavior.ExecuteCount);
        }

        [Test]
        public void BranchBehavior_FalseBranch_WhenStateNotSet()
        {
            var trueBehavior = new TestBehavior();
            var falseBehavior = new TestBehavior();

            // "missingKey" is not set, so GetGlobalState returns null => false branch
            var branch = new BranchBehavior(
                _stateStore, "missingKey", trueBehavior, falseBehavior);

            branch.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, trueBehavior.ExecuteCount);
            Assert.AreEqual(1, falseBehavior.ExecuteCount);
        }

        // -------------------------------------------------------------------
        // 5. Sub-chapter execution
        // -------------------------------------------------------------------

        [Test]
        public void ExecuteChapterBehavior_RunsSubChapterSteps()
        {
            // Build a sub-chapter with its own step
            var subStep = new StepModel { Id = "sub-s1", Name = "SubStep1" };
            var subTransition = new TransitionModel { TargetStepModel = null };
            subStep.TransitionModels.Add(subTransition);

            var subChapter = new ChapterModel { Id = "sub-ch", Name = "SubChapter" };
            subChapter.StepModels.Add(subStep);
            subChapter.FirstStepModel = subStep;

            // Create a MockStepRunner just for the sub-chapter runner
            var subStepRunner = new MockStepRunner();
            subStepRunner.EnqueueTransition(subTransition);

            var subChapterRunner = new ChapterRunner(subStepRunner, _stateStore, _historyService);

            var execBehavior = new ExecuteChapterBehavior(subChapter, null);
            execBehavior.SetChapterRunner(async (chapter, ct) =>
            {
                await subChapterRunner.RunAsync(chapter, ct);
            });

            execBehavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, subStepRunner.RunSteps.Count);
            Assert.AreEqual("sub-s1", subStepRunner.RunSteps[0].Id);
        }

        // -------------------------------------------------------------------
        // 6. Multiple behavior execution stages
        // -------------------------------------------------------------------

        [Test]
        public void BehaviorExecutionStages_ActivationAndDeactivation()
        {
            var activationBehavior = new TestBehavior(
                isBlocking: true, stages: ExecutionStages.Activation);
            var deactivationBehavior = new TestBehavior(
                isBlocking: true, stages: ExecutionStages.Deactivation);
            var bothBehavior = new TestBehavior(
                isBlocking: true,
                stages: ExecutionStages.Activation | ExecutionStages.Deactivation);

            // Verify stage flags are correctly set
            Assert.AreEqual(ExecutionStages.Activation, activationBehavior.Stages);
            Assert.AreEqual(ExecutionStages.Deactivation, deactivationBehavior.Stages);
            Assert.IsTrue(bothBehavior.Stages.HasFlag(ExecutionStages.Activation));
            Assert.IsTrue(bothBehavior.Stages.HasFlag(ExecutionStages.Deactivation));

            // Simulate activation phase: execute only Activation-stage behaviors
            var step = new StepModel { Id = "s1", Name = "Step1" };
            step.BehaviorList.Add(activationBehavior);
            step.BehaviorList.Add(deactivationBehavior);
            step.BehaviorList.Add(bothBehavior);

            IStep istep = step;
            foreach (var behavior in istep.Behaviors)
            {
                if (behavior.Stages.HasFlag(ExecutionStages.Activation))
                {
                    behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
            }

            Assert.AreEqual(1, activationBehavior.ExecuteCount);
            Assert.AreEqual(0, deactivationBehavior.ExecuteCount);
            Assert.AreEqual(1, bothBehavior.ExecuteCount);

            // Simulate deactivation phase
            foreach (var behavior in istep.Behaviors)
            {
                if (behavior.Stages.HasFlag(ExecutionStages.Deactivation))
                {
                    behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
            }

            Assert.AreEqual(1, activationBehavior.ExecuteCount);
            Assert.AreEqual(1, deactivationBehavior.ExecuteCount);
            Assert.AreEqual(2, bothBehavior.ExecuteCount);
        }

        // -------------------------------------------------------------------
        // 7. Cancellation propagation
        // -------------------------------------------------------------------

        [Test]
        public void Cancellation_StopAsync_SetsIdleAndCancelsStep()
        {
            var builder = new TestProcessBuilder("CancelTest");
            builder
                .AddChapter("Ch1")
                .AddStep("Step1", "s1");

            var process = builder.Build();
            var s1 = process.ChapterModels[0].StepModels[0];

            // No transition enqueued: step runner returns null => engine stops
            // But let's test StopAsync as explicit cancellation

            // First, start a process that completes immediately (null transition)
            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();

            // Engine should complete with no transition
            Assert.AreEqual(EngineState.Completed, _engine.State.CurrentValue);

            // Now test StopAsync resets to Idle
            _engine.StopAsync().GetAwaiter().GetResult();

            Assert.AreEqual(EngineState.Idle, _engine.State.CurrentValue);
            Assert.IsNull(_engine.CurrentStep.CurrentValue);
            Assert.IsNull(_engine.CurrentChapter.CurrentValue);
            Assert.AreEqual(1, _stepRunner.CancelCount);
        }

        [Test]
        public void Cancellation_TokenCancelled_EngineReturnsToIdle()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel(); // pre-cancel

            var builder = new TestProcessBuilder("PreCancelled");
            builder
                .AddChapter("Ch1")
                .AddStep("Step1", "s1");

            var process = builder.Build();
            var s1 = process.ChapterModels[0].StepModels[0];
            var t1 = new TransitionModel { TargetStepModel = null };
            s1.TransitionModels.Add(t1);
            _stepRunner.EnqueueTransition(t1);

            _engine.StartProcessAsync(process, cts.Token).GetAwaiter().GetResult();

            // Pre-cancelled token causes OperationCanceledException => Idle
            Assert.AreEqual(EngineState.Idle, _engine.State.CurrentValue);
        }

        // -------------------------------------------------------------------
        // 8. Event-driven flow
        // -------------------------------------------------------------------

        [Test]
        public void TriggerEventBehavior_PublishesEvent_ReceivedByEventBus()
        {
            string receivedPayload = null;
            _eventBus.On("TestEvent").Subscribe(payload =>
            {
                receivedPayload = payload as string;
            });

            var triggerBehavior = new TriggerEventBehavior(
                _eventBus, "TestEvent", "hello");

            triggerBehavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("hello", receivedPayload);
        }

        [Test]
        public void EventBusCondition_EvaluatesTrue_WhenMatchingEventPublished()
        {
            var condition = new EventBusCondition(_eventBus, "ButtonPressed", "btn-ok");

            bool conditionResult = false;
            condition.Evaluate().Subscribe(value =>
            {
                conditionResult = value;
            });

            // Initially false (Prepend(false))
            Assert.IsFalse(conditionResult);

            // Publish matching event
            _eventBus.Publish("ButtonPressed", "btn-ok");
            Assert.IsTrue(conditionResult);

            // Publish non-matching payload
            _eventBus.Publish("ButtonPressed", "btn-cancel");
            Assert.IsFalse(conditionResult);

            condition.Dispose();
        }
    }
}
