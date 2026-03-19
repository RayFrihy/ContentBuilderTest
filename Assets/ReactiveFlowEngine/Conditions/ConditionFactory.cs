using System;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;

namespace ReactiveFlowEngine.Conditions
{
    public class ConditionFactory
    {
        public ICondition Create(ConditionDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            return definition.TypeName switch
            {
                "TimeoutCondition" => new TimeoutCondition(definition.GetFloat("Timeout")),
                "CompositeAndCondition" => CreateCompositeAnd(definition),
                "CompositeOrCondition" => CreateCompositeOr(definition),
                "CompositeNotCondition" => CreateCompositeNot(definition),
                _ => throw new ArgumentException($"Unknown condition type: {definition.TypeName}")
            };
        }

        private ICondition CreateCompositeAnd(ConditionDefinition definition)
        {
            // Composite conditions would need child definitions - for now return empty
            return new CompositeAndCondition();
        }

        private ICondition CreateCompositeOr(ConditionDefinition definition)
        {
            return new CompositeOrCondition();
        }

        private ICondition CreateCompositeNot(ConditionDefinition definition)
        {
            // Would need inner condition definition
            return new CompositeNotCondition(new TimeoutCondition(0f));
        }
    }
}
