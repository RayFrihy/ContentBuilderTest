using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Model
{
    public sealed class TransitionModel : ITransition
    {
        public int Priority { get; set; }
        public List<ICondition> ConditionList { get; set; } = new List<ICondition>();
        public StepModel TargetStepModel { get; set; }

        IReadOnlyList<ICondition> ITransition.Conditions => ConditionList;
        IStep ITransition.TargetStep => TargetStepModel;
        bool ITransition.IsUnconditional => ConditionList.Count == 0;
    }
}
