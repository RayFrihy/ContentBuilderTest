using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Model
{
    public sealed class ProcessModel : IProcess
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ChapterModel> ChapterModels { get; set; } = new List<ChapterModel>();
        public ChapterModel FirstChapterModel { get; set; }

        IReadOnlyList<IChapter> IProcess.Chapters => ChapterModels;
        IChapter IProcess.FirstChapter => FirstChapterModel;
    }
}
