namespace ReactiveFlowEngine.Abstractions
{
    public interface IInputProvider
    {
        bool IsActive { get; }
        void Enable();
        void Disable();
    }
}
