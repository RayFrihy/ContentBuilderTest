namespace ReactiveFlowEngine.Abstractions
{
    public interface IEngineController
    {
        void SetCurrentStep(IStep step);
        void SetCurrentChapter(IChapter chapter);
    }
}
