using System;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;

namespace ReactiveFlowEngine.Behaviors
{
    public class BehaviorFactory
    {
        private readonly ISceneObjectResolver _sceneResolver;

        public BehaviorFactory(ISceneObjectResolver sceneResolver)
        {
            _sceneResolver = sceneResolver;
        }

        public IBehavior Create(BehaviorDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            return definition.TypeName switch
            {
                "MoveObjectBehavior" => new MoveObjectBehavior(
                    _sceneResolver,
                    definition.GetString("TargetObject"),
                    definition.GetString("FinalPosition"),
                    definition.GetFloat("Duration"),
                    definition.GetAnimationCurve("AnimationCurve"),
                    definition.GetBool("IsBlocking"),
                    (ExecutionStages)definition.GetInt("ExecutionStages")
                ),
                "ExecuteChapterBehavior" => new ExecuteChapterBehavior(
                    definition.GetChapter("Chapter"),
                    null // Runner delegate set at runtime
                ),
                "DelayBehavior" => new DelayBehavior(definition.GetFloat("Duration")),
                _ => throw new ArgumentException($"Unknown behavior type: {definition.TypeName}")
            };
        }
    }
}
