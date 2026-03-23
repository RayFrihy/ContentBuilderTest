using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockInputProvider : IInputProvider
    {
        public bool IsActive { get; private set; } = true;
        public int EnableCount { get; private set; }
        public int DisableCount { get; private set; }

        public void Enable() { IsActive = true; EnableCount++; }
        public void Disable() { IsActive = false; DisableCount++; }
    }
}
