using System.Collections.Generic;
using System.Linq;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Engine
{
    public class TransitionEvaluator : ITransitionEvaluator
    {
        public Observable<ITransition> Evaluate(IReadOnlyList<ITransition> transitions)
        {
            if (transitions == null || transitions.Count == 0)
                return Observable.Empty<ITransition>();

            var transitionStreams = new List<Observable<ITransition>>();

            for (int i = 0; i < transitions.Count; i++)
            {
                var transition = transitions[i];

                if (transition == null)
                    continue;

                if (transition.IsUnconditional)
                {
                    // Unconditional transitions fire immediately
                    transitionStreams.Add(Observable.Return(transition));
                }
                else
                {
                    // Conditional transitions require all conditions to be true
                    if (transition.Conditions == null || transition.Conditions.Count == 0)
                    {
                        // No conditions means unconditional
                        transitionStreams.Add(Observable.Return(transition));
                        continue;
                    }

                    var conditionStreams = new List<Observable<bool>>();

                    for (int j = 0; j < transition.Conditions.Count; j++)
                    {
                        var condition = transition.Conditions[j];
                        if (condition != null)
                        {
                            conditionStreams.Add(condition.Evaluate());
                        }
                    }

                    if (conditionStreams.Count == 0)
                    {
                        // No valid conditions means unconditional
                        transitionStreams.Add(Observable.Return(transition));
                        continue;
                    }

                    // Create a stream that waits for all conditions to be true
                    var capturedTransition = transition;
                    var stream = Observable.CombineLatest(conditionStreams)
                        .Select(values =>
                        {
                            // Check if all conditions are true
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (!values[j])
                                    return false;
                            }
                            return true;
                        })
                        .DistinctUntilChanged()
                        .Where(allTrue => allTrue)
                        .Select(_ => capturedTransition)
                        .Take(1);

                    transitionStreams.Add(stream);
                }
            }

            if (transitionStreams.Count == 0)
                return Observable.Empty<ITransition>();

            // Return the first transition that fires
            return Observable.Merge(transitionStreams.ToArray())
                .Take(1);
        }
    }
}
