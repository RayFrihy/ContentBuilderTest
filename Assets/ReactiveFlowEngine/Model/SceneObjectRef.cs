namespace ReactiveFlowEngine.Model
{
    public sealed class SceneObjectRef
    {
        public string Guid { get; set; }

        public SceneObjectRef() { }

        public SceneObjectRef(string guid)
        {
            Guid = guid;
        }
    }
}
