using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Composite
{
	public sealed class XorCondition : ICompositeCondition
	{
		private readonly ICondition[] _children;

		public IReadOnlyList<ICondition> Children => _children;

		public XorCondition(params ICondition[] children)
		{
			_children = children ?? Array.Empty<ICondition>();
		}

		public Observable<bool> Evaluate()
		{
			if (_children.Length == 0)
				return Observable.Return(false);

			return Observable.CombineLatest(
				_children.Select(c => c.Evaluate()).ToArray()
			).Select(EvaluateXor);
		}

		private static bool EvaluateXor(IList<bool> values)
		{
			int trueCount = 0;
			for (int i = 0; i < values.Count; i++)
			{
				if (values[i])
					trueCount++;
			}
			return trueCount == 1;
		}

		public void Reset()
		{
			foreach (var child in _children)
				child.Reset();
		}

		public void Dispose()
		{
			foreach (var child in _children)
				child.Dispose();
		}
	}
}
