using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Model
{
    public sealed class StepModel : IStep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StepType Type { get; set; }
        public List<IBehavior> BehaviorList { get; set; } = new List<IBehavior>();
        public List<TransitionModel> TransitionModels { get; set; } = new List<TransitionModel>();
        public ChapterModel SubChapterModel { get; set; }

        IReadOnlyList<IBehavior> IStep.Behaviors => BehaviorList;
        IReadOnlyList<ITransition> IStep.Transitions => TransitionModels;
        IChapter IStep.SubChapter => SubChapterModel;
    }
}
