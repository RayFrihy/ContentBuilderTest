using UnityEngine;

namespace ReactiveFlowEngine.Abstractions
{
    public interface ISceneObjectResolver
    {
        Transform Resolve(string guid);
    }
}
