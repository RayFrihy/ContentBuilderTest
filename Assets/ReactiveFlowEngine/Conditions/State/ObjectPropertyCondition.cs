using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class ObjectPropertyCondition : IStateCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly string _propertyName;
        private readonly object _expectedValue;

        public string StateKey => _propertyName;

        public ObjectPropertyCondition(ISceneObjectResolver resolver, string targetObjectId, string propertyName, object expectedValue)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _expectedValue = expectedValue;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => CheckProperty())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool CheckProperty()
        {
            var target = _resolver.Resolve(_targetObjectId);
            if (target == null)
                return false;

            return _propertyName switch
            {
                "activeSelf" => CompareValue(target.gameObject.activeSelf),
                "activeInHierarchy" => CompareValue(target.gameObject.activeInHierarchy),
                "position" => CompareValue(target.position),
                "rotation" => CompareValue(target.rotation),
                "localScale" => CompareValue(target.localScale),
                "tag" => CompareValue(target.tag),
                "name" => CompareValue(target.name),
                _ => CheckComponentProperty(target)
            };
        }

        private bool CheckComponentProperty(Transform target)
        {
            var components = target.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var type = component.GetType();
                var property = type.GetProperty(_propertyName);
                if (property != null)
                {
                    var value = property.GetValue(component);
                    return CompareValue(value);
                }

                var field = type.GetField(_propertyName);
                if (field != null)
                {
                    var value = field.GetValue(component);
                    return CompareValue(value);
                }
            }
            return false;
        }

        private bool CompareValue(object actual)
        {
            if (actual == null && _expectedValue == null)
                return true;
            if (actual == null || _expectedValue == null)
                return false;
            return actual.Equals(_expectedValue);
        }
    }
}
