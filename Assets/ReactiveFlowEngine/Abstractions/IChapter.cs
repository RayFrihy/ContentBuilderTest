using System.Collections.Generic;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IChapter
    {
        string Id { get; }
        string Name { get; }
        IStep FirstStep { get; }
        IReadOnlyList<IStep> Steps { get; }
        IReadOnlyList<IChapter> SubChapters { get; }
    }
}
