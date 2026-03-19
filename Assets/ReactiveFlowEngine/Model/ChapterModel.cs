using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Model
{
    public sealed class ChapterModel : IChapter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StepModel FirstStepModel { get; set; }
        public List<StepModel> StepModels { get; set; } = new List<StepModel>();
        public List<ChapterModel> SubChapterModels { get; set; } = new List<ChapterModel>();

        IStep IChapter.FirstStep => FirstStepModel;
        IReadOnlyList<IStep> IChapter.Steps => StepModels;
        IReadOnlyList<IChapter> IChapter.SubChapters => SubChapterModels;
    }
}
