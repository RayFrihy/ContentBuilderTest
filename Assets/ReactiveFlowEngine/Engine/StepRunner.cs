using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveFlowEngine.Abstractions;
using UnityEngine;

namespace ReactiveFlowEngine.Engine
{
    public class StepRunner : IStepRunner
    {
        private readonly ITransitionEvaluator _transitionEvaluator;

        // Track the most recently started step's CTS for CancelCurrentStep().
        // This is volatile because sub-chapter execution is reentrant.
        private volatile CancellationTokenSource _activeStepCts;

        public StepRunner(ITransitionEvaluator transitionEvaluator)
        {
            _transitionEvaluator = transitionEvaluator ?? throw new ArgumentNullException(nameof(transitionEvaluator));
        }

        public async UniTask<ITransition> RunStepAsync(IStep step, CancellationToken ct)
        {
            if (step == null)
                return null;

            // All state is local to support reentrant calls from sub-chapter execution
            var stepCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var stepCt = stepCts.Token;
            var conditionSubscriptions = new CompositeDisposable();

            // Track as the active step for CancelCurrentStep()
            _activeStepCts = stepCts;

            try
            {
                Debug.Log($"[RFE] Entering step: {step.Name} ({step.Id})");

                // Phase 1: Separate behaviors by blocking status and execution stage
                var blockingBehaviors = new List<IBehavior>();
                var nonBlockingBehaviors = new List<IBehavior>();

                if (step.Behaviors != null)
                {
                    foreach (var behavior in step.Behaviors)
                    {
                        if (behavior == null)
                            continue;

                        if (behavior.Stages.HasFlag(ExecutionStages.Activation))
                        {
                            if (behavior.IsBlocking)
                                blockingBehaviors.Add(behavior);
                            else
                                nonBlockingBehaviors.Add(behavior);
                        }
                    }
                }

                // Phase 2: Fire non-blocking behaviors (fire-and-forget)
                var nonBlockingTasks = new List<UniTask>();
                foreach (var behavior in nonBlockingBehaviors)
                {
                    nonBlockingTasks.Add(behavior.ExecuteAsync(stepCt).SuppressCancellationThrow());
                }

                // Phase 3: Execute blocking behaviors in sequence
                foreach (var behavior in blockingBehaviors)
                {
                    stepCt.ThrowIfCancellationRequested();
                    await behavior.ExecuteAsync(stepCt);
                }

                // Phase 4: Evaluate transitions
                ITransition winner = await EvaluateTransitionsAsync(
                    step.Transitions, stepCt, conditionSubscriptions);

                // Phase 5: Cancel non-blocking behaviors on transition
                if (!stepCts.IsCancellationRequested)
                {
                    stepCts.Cancel();
                }

                // Wait for all non-blocking tasks to complete or be cancelled
                if (nonBlockingTasks.Count > 0)
                    await UniTask.WhenAll(nonBlockingTasks);

                if (winner != null)
                {
                    Debug.Log($"[RFE] Step {step.Name} transitioning to: {(winner.TargetStep?.Name ?? "END")}");
                }
                else
                {
                    Debug.Log($"[RFE] Step {step.Name} completed with no transition.");
                }

                return winner;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[RFE] Step {step.Name} was cancelled.");
                return null;
            }
            finally
            {
                // Cleanup condition subscriptions
                conditionSubscriptions.Dispose();

                // Dispose conditions in transitions
                if (step?.Transitions != null)
                {
                    foreach (var transition in step.Transitions)
                    {
                        if (transition?.Conditions == null) continue;
                        foreach (var condition in transition.Conditions)
                        {
                            if (condition is IDisposable disposable)
                                disposable.Dispose();
                        }
                    }
                }

                // Clean up step CTS
                stepCts.Dispose();

                // Clear active reference only if it's still ours
                if (_activeStepCts == stepCts)
                    _activeStepCts = null;
            }
        }

        public void CancelCurrentStep()
        {
            var cts = _activeStepCts;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        private async UniTask<ITransition> EvaluateTransitionsAsync(
            IReadOnlyList<ITransition> transitions,
            CancellationToken ct,
            CompositeDisposable subscriptions)
        {
            if (transitions == null || transitions.Count == 0)
                return null;

            var transitionObservable = _transitionEvaluator.Evaluate(transitions);

            ITransition winner = null;
            var subscription = transitionObservable
                .Subscribe(t => winner = t);

            subscriptions.Add(subscription);

            // Wait for transition to fire or cancellation
            try
            {
                await UniTask.WaitUntil(() => winner != null || ct.IsCancellationRequested,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                // Expected when step is cancelled
            }

            return winner;
        }
    }
}
