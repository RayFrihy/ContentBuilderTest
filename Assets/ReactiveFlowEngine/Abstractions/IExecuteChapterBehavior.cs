using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IExecuteChapterBehavior : IReversibleBehavior
    {
        void SetChapterRunner(Func<IChapter, CancellationToken, UniTask> chapterRunner);
        IChapter GetSubChapter();
    }
}
